using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplashCityCarwash.Models;

namespace SplashCityCarwash.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class StaffController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StaffController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var staffList = new List<StaffViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                staffList.Add(new StaffViewModel
                {
                    User = user,
                    Role = roles.FirstOrDefault() ?? "No Role"
                });
            }

            return View(staffList);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
                TempData["Success"] = $"✅ Staff account {(user.IsActive ? "activated" : "deactivated")}!";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(string id, string newRole)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!await _roleManager.RoleExistsAsync(newRole))
                    await _roleManager.CreateAsync(new IdentityRole(newRole));

                await _userManager.AddToRoleAsync(user, newRole);
                TempData["Success"] = $"✅ Role updated to {newRole}!";
            }
            return RedirectToAction("Index");
        }
    }
}