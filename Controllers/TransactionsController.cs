using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplashCityCarwash.Data;
using SplashCityCarwash.Models;

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
                .Where(t => t.Status == WashStatus.Completed)
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
                .FirstOrDefaultAsync(t => t.TransactionID == id);

            if (transaction == null) return NotFound();
            return View(transaction);
        }
    }
}