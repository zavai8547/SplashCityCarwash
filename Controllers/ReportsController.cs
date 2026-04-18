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
        public ReportsController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var firstOfMonth = new DateTime(today.Year, today.Month, 1);
            var firstOfYear = new DateTime(today.Year, 1, 1);

            var vm = new ReportsViewModel
            {
                RevenueToday = await _db.Transactions
                    .Where(t => t.CreatedAt.Date == today &&
                               (t.Status == WashStatus.Completed ||
                                t.Status == WashStatus.Paid))
                    .SumAsync(t => (decimal?)t.TotalAmount) ?? 0,

                RevenueThisMonth = await _db.Transactions
                    .Where(t => t.CreatedAt >= firstOfMonth &&
                               (t.Status == WashStatus.Completed ||
                                t.Status == WashStatus.Paid))
                    .SumAsync(t => (decimal?)t.TotalAmount) ?? 0,

                RevenueThisYear = await _db.Transactions
                    .Where(t => t.CreatedAt >= firstOfYear &&
                               (t.Status == WashStatus.Completed ||
                                t.Status == WashStatus.Paid))
                    .SumAsync(t => (decimal?)t.TotalAmount) ?? 0,

                TotalWashesToday = await _db.Transactions
                    .CountAsync(t => t.CreatedAt.Date == today &&
                                   (t.Status == WashStatus.Completed ||
                                    t.Status == WashStatus.Paid)),

                TotalWashesMonth = await _db.Transactions
                    .CountAsync(t => t.CreatedAt >= firstOfMonth &&
                                   (t.Status == WashStatus.Completed ||
                                    t.Status == WashStatus.Paid)),

                TotalCustomers = await _db.Customers.CountAsync(),

                TotalVehicles = await _db.Vehicles.CountAsync(),

                // DateTime comparison (unchanged)
                ExpensesThisMonth = await _db.Expenses
                    .Where(e => e.ExpenseDate >= firstOfMonth)
                    .SumAsync(e => (decimal?)e.Amount) ?? 0,

                ServicePopularity = await _db.TransactionServices
                    .GroupBy(ts => ts.Service.ServiceName)
                    .Select(g => new ServicePopularityItem
                    {
                        ServiceName = g.Key,
                        Count = g.Count(),
                        Revenue = g.Sum(x => x.PriceAtTime)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync(),

                PaymentMethodBreakdown = await _db.Transactions
                    .Where(t => t.Status == WashStatus.Completed ||
                                t.Status == WashStatus.Paid)
                    .GroupBy(t => t.PaymentMethod)
                    .Select(g => new PaymentBreakdownItem
                    {
                        Method = g.Key.ToString(),
                        Count = g.Count(),
                        Total = g.Sum(x => x.TotalAmount)
                    })
                    .ToListAsync(),

                TopCustomers = await _db.Customers
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(5)
                    .ToListAsync()
            };

            vm.ProfitThisMonth = vm.RevenueThisMonth - vm.ExpensesThisMonth;
            return View(vm);
        }
    }
}