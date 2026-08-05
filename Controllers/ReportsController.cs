using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplashCityCarwash.Data;
using SplashCityCarwash.Models;

namespace SplashCityCarwash.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _db;

        public ReportsController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(
            int? month, int? year, int? branchID)
        {
            var today = DateTime.Today;
            var selectedMonth = month ?? today.Month;
            var selectedYear = year ?? today.Year;

            var firstOfMonth = new DateTime(
                selectedYear, selectedMonth, 1);
            var lastOfMonth = firstOfMonth
                .AddMonths(1).AddDays(-1);
            var firstOfYear = new DateTime(
                selectedYear, 1, 1);

            // All branches for filter dropdown
            var allBranches = await _db.Branches
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            // Base transaction query
            // filtered by branch if selected
            IQueryable<Transaction> txQuery =
                _db.Transactions
                    .Where(t =>
                        t.Status ==
                            WashStatus.Completed ||
                        t.Status == WashStatus.Paid);

            IQueryable<ShopSale> shopQuery =
                _db.ShopSales.AsQueryable();

            IQueryable<Expense> expQuery =
                _db.Expenses.AsQueryable();

            if (branchID.HasValue)
            {
                txQuery = txQuery.Where(t =>
                    t.BranchID == branchID);
                shopQuery = shopQuery.Where(s =>
                    s.BranchID == branchID);
                expQuery = expQuery.Where(e =>
                    e.BranchID == branchID);
            }

            // ── TODAY ─────────────────────────────
            var carwashRevenueToday =
                await txQuery
                    .Where(t =>
                        t.CreatedAt.Date == today)
                    .SumAsync(t =>
                        (decimal?)t.TotalAmount) ?? 0;

            var shopRevenueToday =
                await shopQuery
                    .Where(s =>
                        s.CreatedAt.Date == today)
                    .SumAsync(s =>
                        (decimal?)s.TotalAmount) ?? 0;

            var washesToday = await txQuery
                .CountAsync(t =>
                    t.CreatedAt.Date == today);

            // ── MONTH ─────────────────────────────
            var carwashRevenueMonth =
                await txQuery
                    .Where(t =>
                        t.CreatedAt >= firstOfMonth &&
                        t.CreatedAt <= lastOfMonth)
                    .SumAsync(t =>
                        (decimal?)t.TotalAmount) ?? 0;

            var washesMonth = await txQuery
                .CountAsync(t =>
                    t.CreatedAt >= firstOfMonth &&
                    t.CreatedAt <= lastOfMonth);

            var expensesMonth =
                await expQuery
                    .Where(e =>
                        e.ExpenseDate >= firstOfMonth &&
                        e.ExpenseDate <= lastOfMonth)
                    .SumAsync(e =>
                        (decimal?)e.Amount) ?? 0;

            var shopRevenueMonth =
                await shopQuery
                    .Where(s =>
                        s.CreatedAt >= firstOfMonth &&
                        s.CreatedAt <= lastOfMonth)
                    .SumAsync(s =>
                        (decimal?)s.TotalAmount) ?? 0;

            var shopProfitMonth =
                await shopQuery
                    .Where(s =>
                        s.CreatedAt >= firstOfMonth &&
                        s.CreatedAt <= lastOfMonth)
                    .SumAsync(s =>
                        (decimal?)s.TotalProfit) ?? 0;

            // ── YEAR ──────────────────────────────
            var carwashRevenueYear =
                await txQuery
                    .Where(t =>
                        t.CreatedAt >= firstOfYear)
                    .SumAsync(t =>
                        (decimal?)t.TotalAmount) ?? 0;

            var expensesYear =
                await expQuery
                    .Where(e =>
                        e.ExpenseDate >= firstOfYear)
                    .SumAsync(e =>
                        (decimal?)e.Amount) ?? 0;

            var shopRevenueYear =
                await shopQuery
                    .Where(s =>
                        s.CreatedAt >= firstOfYear)
                    .SumAsync(s =>
                        (decimal?)s.TotalAmount) ?? 0;

            var shopProfitYear =
                await shopQuery
                    .Where(s =>
                        s.CreatedAt >= firstOfYear)
                    .SumAsync(s =>
                        (decimal?)s.TotalProfit) ?? 0;

            // ── CALCULATED ────────────────────────
            var shopCostMonth =
                shopRevenueMonth - shopProfitMonth;
            var carwashProfitMonth =
                carwashRevenueMonth - expensesMonth;
            var carwashProfitYear =
                carwashRevenueYear - expensesYear;
            var totalRevenueMonth =
                carwashRevenueMonth + shopRevenueMonth;
            var totalCostsMonth =
                expensesMonth + shopCostMonth;
            var totalProfitMonth =
                carwashProfitMonth + shopProfitMonth;
            var totalRevenueYear =
                carwashRevenueYear + shopRevenueYear;
            var totalProfitYear =
                carwashProfitYear + shopProfitYear;

            // ── WEEKLY PERFORMANCE ────────────────
            // Current week Mon–Sun
            var dayOfWeek = (int)today.DayOfWeek;
            var monday = today.AddDays(
                -(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
            var sunday = monday.AddDays(6);

            var weeklyData = new List<object>();
            for (int d = 0; d < 7; d++)
            {
                var day = monday.AddDays(d);
                var dayTx = await txQuery
                    .Where(t =>
                        t.CreatedAt.Date == day)
                    .SumAsync(t =>
                        (decimal?)t.TotalAmount) ?? 0;
                var dayShop = await shopQuery
                    .Where(s =>
                        s.CreatedAt.Date == day)
                    .SumAsync(s =>
                        (decimal?)s.TotalAmount) ?? 0;
                var dayExp = await expQuery
                    .Where(e =>
                        e.ExpenseDate.Date == day)
                    .SumAsync(e =>
                        (decimal?)e.Amount) ?? 0;
                var dayWashes = await txQuery
                    .CountAsync(t =>
                        t.CreatedAt.Date == day);

                weeklyData.Add(new
                {
                    day = day.ToString("ddd dd MMM"),
                    isToday = day == today,
                    carwash = dayTx,
                    shop = dayShop,
                    total = dayTx + dayShop,
                    expenses = dayExp,
                    profit = dayTx + dayShop - dayExp,
                    washes = dayWashes
                });
            }

            // Last week for comparison
            var lastMonday = monday.AddDays(-7);
            var lastWeekTotal = 0m;
            for (int d = 0; d < 7; d++)
            {
                var day = lastMonday.AddDays(d);
                lastWeekTotal +=
                    await txQuery
                        .Where(t =>
                            t.CreatedAt.Date == day)
                        .SumAsync(t =>
                            (decimal?)t.TotalAmount)
                    ?? 0;
                lastWeekTotal +=
                    await shopQuery
                        .Where(s =>
                            s.CreatedAt.Date == day)
                        .SumAsync(s =>
                            (decimal?)s.TotalAmount)
                    ?? 0;
            }

            decimal thisWeekTotal = weeklyData
                .Sum(w => (decimal)
                    ((dynamic)w).total);

            decimal weekChange = lastWeekTotal > 0
                ? ((thisWeekTotal - lastWeekTotal)
                    / lastWeekTotal * 100)
                : 0;

            // ── BRANCH COMPARISON ─────────────────
            var branchComparison =
                new List<object>();
            foreach (var b in allBranches)
            {
                var bRevenue = await _db.Transactions
                    .Where(t =>
                        t.BranchID == b.BranchID &&
                        t.CreatedAt >= firstOfMonth &&
                        t.CreatedAt <= lastOfMonth &&
                        (t.Status ==
                            WashStatus.Completed ||
                         t.Status ==
                            WashStatus.Paid))
                    .SumAsync(t =>
                        (decimal?)t.TotalAmount) ?? 0;

                var bShop = await _db.ShopSales
                    .Where(s =>
                        s.BranchID == b.BranchID &&
                        s.CreatedAt >= firstOfMonth &&
                        s.CreatedAt <= lastOfMonth)
                    .SumAsync(s =>
                        (decimal?)s.TotalAmount) ?? 0;

                var bExpenses = await _db.Expenses
                    .Where(e =>
                        e.BranchID == b.BranchID &&
                        e.ExpenseDate >= firstOfMonth &&
                        e.ExpenseDate <= lastOfMonth)
                    .SumAsync(e =>
                        (decimal?)e.Amount) ?? 0;

                var bWashes = await _db.Transactions
                    .CountAsync(t =>
                        t.BranchID == b.BranchID &&
                        t.CreatedAt >= firstOfMonth &&
                        t.CreatedAt <= lastOfMonth &&
                        (t.Status ==
                            WashStatus.Completed ||
                         t.Status ==
                            WashStatus.Paid));

                branchComparison.Add(new
                {
                    Name = b.Name,
                    Washes = bWashes,
                    CarwashRevenue = bRevenue,
                    ShopRevenue = bShop,
                    Total = bRevenue + bShop,
                    Expenses = bExpenses,
                    Profit = bRevenue + bShop
                        - bExpenses
                });
            }

            // ── 6 MONTH TREND ─────────────────────
            var monthlyData = new List<object>();
            for (int i = 5; i >= 0; i--)
            {
                var mStart = new DateTime(
                    today.Year, today.Month, 1)
                    .AddMonths(-i);
                var mEnd = mStart.AddMonths(1)
                    .AddDays(-1);

                var mCarwash = await txQuery
                    .Where(t =>
                        t.CreatedAt >= mStart &&
                        t.CreatedAt <= mEnd)
                    .SumAsync(t =>
                        (decimal?)t.TotalAmount) ?? 0;

                var mShop = await shopQuery
                    .Where(s =>
                        s.CreatedAt >= mStart &&
                        s.CreatedAt <= mEnd)
                    .SumAsync(s =>
                        (decimal?)s.TotalAmount) ?? 0;

                var mExpenses = await expQuery
                    .Where(e =>
                        e.ExpenseDate >= mStart &&
                        e.ExpenseDate <= mEnd)
                    .SumAsync(e =>
                        (decimal?)e.Amount) ?? 0;

                var mShopProfit = await shopQuery
                    .Where(s =>
                        s.CreatedAt >= mStart &&
                        s.CreatedAt <= mEnd)
                    .SumAsync(s =>
                        (decimal?)s.TotalProfit) ?? 0;

                monthlyData.Add(new
                {
                    month = mStart.ToString("MMM yyyy"),
                    carwashRevenue = mCarwash,
                    shopRevenue = mShop,
                    totalRevenue = mCarwash + mShop,
                    expenses = mExpenses,
                    profit = (mCarwash - mExpenses)
                        + mShopProfit
                });
            }

            // ── SERVICE POPULARITY ────────────────
            var serviceQuery = _db.TransactionServices
                .Include(ts => ts.Service)
                .Include(ts => ts.Transaction)
                .Where(ts =>
                    ts.Transaction.CreatedAt >=
                        firstOfMonth &&
                    ts.Transaction.CreatedAt <=
                        lastOfMonth);

            if (branchID.HasValue)
                serviceQuery = serviceQuery.Where(
                    ts => ts.Transaction.BranchID
                        == branchID);

            var serviceStats = await serviceQuery
                .GroupBy(ts => ts.Service.ServiceName)
                .Select(g => new
                {
                    ServiceName = g.Key,
                    TimesUsed = g.Count(),
                    Revenue = g.Sum(
                        ts => ts.PriceAtTime)
                })
                .OrderByDescending(x => x.TimesUsed)
                .ToListAsync();

            // ── PAYMENT BREAKDOWN ─────────────────
            var paymentStats = await txQuery
                .Where(t =>
                    t.CreatedAt >= firstOfMonth &&
                    t.CreatedAt <= lastOfMonth)
                .GroupBy(t => t.PaymentMethod)
                .Select(g => new
                {
                    Method = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(t => t.TotalAmount)
                })
                .ToListAsync();

            // ── TOP CUSTOMERS ─────────────────────
            var topCustomers = await _db.Customers
                .OrderByDescending(c => c.TotalSpent)
                .Take(5)
                .ToListAsync();

            var totalCustomers =
                await _db.Customers.CountAsync();
            var totalVehicles =
                await _db.Vehicles.CountAsync();

            // ── VIEWBAG ───────────────────────────
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedMonthName =
                firstOfMonth.ToString("MMMM yyyy");
            ViewBag.SelectedBranch = branchID;
            ViewBag.Branches = allBranches;

            ViewBag.CarwashRevenueToday =
                carwashRevenueToday;
            ViewBag.ShopRevenueToday =
                shopRevenueToday;
            ViewBag.WashesToday = washesToday;

            ViewBag.CarwashRevenueMonth =
                carwashRevenueMonth;
            ViewBag.CarwashRevenueYear =
                carwashRevenueYear;
            ViewBag.CarwashProfitMonth =
                carwashProfitMonth;
            ViewBag.CarwashProfitYear =
                carwashProfitYear;
            ViewBag.WashesMonth = washesMonth;

            ViewBag.ExpensesMonth = expensesMonth;
            ViewBag.ExpensesYear = expensesYear;

            ViewBag.ShopRevenueMonth =
                shopRevenueMonth;
            ViewBag.ShopRevenueYear =
                shopRevenueYear;
            ViewBag.ShopProfitMonth =
                shopProfitMonth;
            ViewBag.ShopProfitYear =
                shopProfitYear;
            ViewBag.ShopCostMonth = shopCostMonth;

            ViewBag.TotalRevenueMonth =
                totalRevenueMonth;
            ViewBag.TotalCostsMonth =
                totalCostsMonth;
            ViewBag.TotalProfitMonth =
                totalProfitMonth;
            ViewBag.TotalRevenueYear =
                totalRevenueYear;
            ViewBag.TotalProfitYear =
                totalProfitYear;

            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.TotalVehicles = totalVehicles;

            ViewBag.WeeklyData = weeklyData;
            ViewBag.ThisWeekTotal = thisWeekTotal;
            ViewBag.LastWeekTotal = lastWeekTotal;
            ViewBag.WeekChange = weekChange;
            ViewBag.WeekStart =
                monday.ToString("dd MMM");
            ViewBag.WeekEnd =
                sunday.ToString("dd MMM yyyy");

            ViewBag.BranchComparison =
                branchComparison;
            ViewBag.MonthlyData = monthlyData;
            ViewBag.ServiceStats = serviceStats;
            ViewBag.PaymentStats = paymentStats;
            ViewBag.TopCustomers = topCustomers;

            return View();
        }
    }
}