using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplashCityCarwash.Data;
using SplashCityCarwash.Models;
using System.Security.Claims;

namespace SplashCityCarwash.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly AppDbContext _db;
        public TransactionsController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index(
            string? status, string? payment, string? date)
        {
            var query = _db.Transactions
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Include(t => t.Staff)
                .Include(t => t.TransactionServices)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.TransactionWashers)
                    .ThenInclude(tw => tw.Washer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) &&
                Enum.TryParse<WashStatus>(status, out var washStatus))
                query = query.Where(t => t.Status == washStatus);

            if (!string.IsNullOrEmpty(payment) &&
                Enum.TryParse<PaymentMethod>(payment, out var payMethod))
                query = query.Where(t => t.PaymentMethod == payMethod);

            if (!string.IsNullOrEmpty(date) &&
                DateTime.TryParse(date, out var filterDate))
                query = query.Where(t => t.CreatedAt.Date == filterDate.Date);

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.TotalRevenue = transactions
                .Where(t => t.Status == WashStatus.Completed ||
                            t.Status == WashStatus.Paid)
                .Sum(t => t.TotalAmount);

            ViewBag.Status = status;
            ViewBag.Payment = payment;
            ViewBag.Date = date;

            return View(transactions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _db.Transactions
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Include(t => t.Staff)
                .Include(t => t.TransactionServices)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.TransactionWashers)
                    .ThenInclude(tw => tw.Washer)
                .FirstOrDefaultAsync(t => t.TransactionID == id);

            if (transaction == null) return NotFound();
            return View(transaction);
        }

        // ── BULK ENTRY ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> BulkEntry()
        {
            ViewBag.Branches = await _db.Branches
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BulkEntry(
            DateTime entryDate,
            decimal cashAmount,
            decimal mpesaAmount,
            int? vehicleCount,
            int branchID,
            string? notes)
        {
            var staffID = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            decimal total = cashAmount + mpesaAmount;

            if (total <= 0)
            {
                TempData["Error"] =
                    "Total amount must be greater than zero.";
                ViewBag.Branches = await _db.Branches
                    .Where(b => b.IsActive).ToListAsync();
                return View();
            }

            // If both cash and MPesa, save two records
            // so payment method filtering works correctly
            if (cashAmount > 0)
            {
                _db.Transactions.Add(new Transaction
                {
                    StaffID = staffID!,
                    BranchID = branchID,
                    CustomerID = null,
                    VehicleID = null,
                    TotalAmount = cashAmount,
                    PaymentMethod = PaymentMethod.Cash,
                    IsBulkEntry = true,
                    VehicleCount = vehicleCount,
                    Notes = notes?.Trim(),
                    CreatedAt = entryDate.Date
                        .Add(DateTime.Now.TimeOfDay),
                    Status = WashStatus.Completed
                });
            }

            if (mpesaAmount > 0)
            {
                _db.Transactions.Add(new Transaction
                {
                    StaffID = staffID!,
                    BranchID = branchID,
                    CustomerID = null,
                    VehicleID = null,
                    TotalAmount = mpesaAmount,
                    PaymentMethod = PaymentMethod.MPesa,
                    IsBulkEntry = true,
                    VehicleCount = vehicleCount,
                    Notes = notes?.Trim(),
                    CreatedAt = entryDate.Date
                        .Add(DateTime.Now.TimeOfDay),
                    Status = WashStatus.Completed
                });
            }

            await _db.SaveChangesAsync();

            TempData["Success"] =
                $"Bulk entry saved. " +
                $"Total: KES {total:N0} " +
                $"(Cash: KES {cashAmount:N0} + " +
                $"M-Pesa: KES {mpesaAmount:N0})" +
                (vehicleCount.HasValue
                    ? $" — {vehicleCount} vehicles."
                    : ".");

            return RedirectToAction("Index");
        }

        // ── MARK AS PAID ───────────────────────────────
        [HttpPost]
        public async Task<IActionResult> MarkPaid(int id, string? mpesaCode)
        {
            var transaction = await _db.Transactions
                .FindAsync(id);

            if (transaction == null) return NotFound();

            transaction.Status = WashStatus.Paid;
            transaction.PaidAt = DateTime.Now;

            if (!string.IsNullOrEmpty(mpesaCode))
                transaction.MpesaCode = mpesaCode.Trim().ToUpper();

            await _db.SaveChangesAsync();
            TempData["Success"] = $"✅ Payment confirmed! KES {transaction.TotalAmount:N0}";
            return RedirectToAction("Details", new { id });
        }

        // ── EDIT TRANSACTION ───────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _db.Transactions
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .FirstOrDefaultAsync(t => t.TransactionID == id);

            if (transaction == null) return NotFound();

            var vm = new EditTransactionViewModel
            {
                TransactionID = transaction.TransactionID,
                TotalAmount = transaction.TotalAmount,
                PaymentMethod = transaction.PaymentMethod,
                Status = transaction.Status,
                MpesaCode = transaction.MpesaCode,
                Notes = transaction.Notes,
                WashDate = transaction.CreatedAt.ToString("yyyy-MM-dd"),
                WashTime = transaction.CreatedAt.ToString("HH:mm")
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(EditTransactionViewModel model)
        {
            var transaction = await _db.Transactions
                .FindAsync(model.TransactionID);

            if (transaction == null) return NotFound();

            // Parse date and time
            if (DateTime.TryParse(
                $"{model.WashDate} {model.WashTime}", out var newDateTime))
                transaction.CreatedAt = newDateTime;

            transaction.TotalAmount = model.TotalAmount;
            transaction.PaymentMethod = model.PaymentMethod;
            transaction.Status = model.Status;
            transaction.Notes = model.Notes;
            transaction.MpesaCode = string.IsNullOrEmpty(model.MpesaCode)
                ? null : model.MpesaCode.Trim().ToUpper();

            if (model.Status == WashStatus.Paid && transaction.PaidAt == null)
                transaction.PaidAt = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Success"] = "✅ Transaction updated!";
            return RedirectToAction("Details", new { id = model.TransactionID });
        }

        // ── EXPORT CSV ─────────────────────────────────
        public async Task<IActionResult> Export(
            string? status, string? payment, string? date)
        {
            var query = _db.Transactions
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Include(t => t.Staff)
                .Include(t => t.TransactionServices)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.TransactionWashers)
                    .ThenInclude(tw => tw.Washer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) &&
                Enum.TryParse<WashStatus>(status, out var washStatus))
                query = query.Where(t => t.Status == washStatus);

            if (!string.IsNullOrEmpty(payment) &&
                Enum.TryParse<PaymentMethod>(payment, out var payMethod))
                query = query.Where(t => t.PaymentMethod == payMethod);

            if (!string.IsNullOrEmpty(date) &&
                DateTime.TryParse(date, out var filterDate))
                query = query.Where(t => t.CreatedAt.Date == filterDate.Date);

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("KUJARIBUTUU CARWASH & DETAILING");
            csv.AppendLine($"Exported: {DateTime.Now:dd MMM yyyy HH:mm}");
            csv.AppendLine($"Total Records: {transactions.Count}");
            csv.AppendLine($"Total Revenue: KES {transactions.Where(t => t.Status == WashStatus.Paid || t.Status == WashStatus.Completed).Sum(t => t.TotalAmount):N0}");
            csv.AppendLine("");
            csv.AppendLine("Date,Registration,Car Model,Services,Amount (KES),Payment Method,M-Pesa Code,Washed By,Status,Cashier");

            foreach (var t in transactions)
            {
                var services = string.Join(" + ",
                    t.TransactionServices.Select(s => s.Service.ServiceName));
                var carModel = $"{t.Vehicle.Make} {t.Vehicle.CarModel}".Trim();
                var washers = t.TransactionWashers.Any()
                    ? string.Join(" + ", t.TransactionWashers.Select(w => w.Washer.FullName))
                    : "—";

                csv.AppendLine(
                    $"{t.CreatedAt:dd/MM/yyyy HH:mm}," +
                    $"{t.Vehicle.LicensePlate}," +
                    $"{(string.IsNullOrEmpty(carModel) ? "—" : carModel)}," +
                    $"\"{services}\"," +
                    $"{t.TotalAmount}," +
                    $"{t.PaymentMethod}," +
                    $"{t.MpesaCode ?? "—"}," +
                    $"\"{washers}\"," +
                    $"{t.Status}," +
                    $"{t.Staff.FullName}"
                );
            }

            var fileName = $"Kujaributuu_Transactions_{DateTime.Now:yyyyMMdd_HHmm}.csv";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", fileName);
        }
    }
}