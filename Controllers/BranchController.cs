using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplashCityCarwash.Data;
using SplashCityCarwash.Models;

namespace SplashCityCarwash.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BranchController : Controller
    {
        private readonly AppDbContext _db;

        public BranchController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var branches = await _db.Branches
                .OrderBy(b => b.Name)
                .ToListAsync();

            // Stats per branch
            var today = DateTime.Today;
            var firstOfMonth = new DateTime(
                today.Year, today.Month, 1);

            foreach (var b in branches)
            {
                b.Transactions = await _db
                    .Transactions
                    .Where(t =>
                        t.BranchID == b.BranchID &&
                        (t.Status == WashStatus.Completed
                        || t.Status == WashStatus.Paid))
                    .ToListAsync();
            }

            ViewBag.TotalBranches = branches.Count;
            ViewBag.ActiveBranches =
                branches.Count(b => b.IsActive);

            return View(branches);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(
            string name,
            string? location,
            string? phone,
            string? managerName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] =
                    "Branch name is required.";
                return View();
            }

            var branch = new Branch
            {
                Name = name.Trim(),
                Location = location?.Trim(),
                Phone = phone?.Trim(),
                ManagerName = managerName?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _db.Branches.Add(branch);
            await _db.SaveChangesAsync();

            TempData["Success"] =
                $"{branch.Name} branch added.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var branch = await _db.Branches
                .FindAsync(id);
            if (branch == null) return NotFound();
            return View(branch);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(
            int branchID,
            string name,
            string? location,
            string? phone,
            string? managerName,
            bool isActive)
        {
            var branch = await _db.Branches
                .FindAsync(branchID);
            if (branch == null) return NotFound();

            branch.Name = name.Trim();
            branch.Location = location?.Trim();
            branch.Phone = phone?.Trim();
            branch.ManagerName = managerName?.Trim();
            branch.IsActive = isActive;

            await _db.SaveChangesAsync();

            TempData["Success"] =
                $"{branch.Name} updated.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(
            int id)
        {
            var branch = await _db.Branches
                .FindAsync(id);
            if (branch == null) return NotFound();

            branch.IsActive = !branch.IsActive;
            await _db.SaveChangesAsync();

            TempData["Success"] = branch.IsActive
                ? $"{branch.Name} activated."
                : $"{branch.Name} deactivated.";

            return RedirectToAction("Index");
        }
    }
}