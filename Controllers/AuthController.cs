using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SplashCityCarwash.Models;

namespace SplashCityCarwash.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // ── LOGIN ──────────────────────────────────────
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
                return RedirectToAction("Index", "Dashboard");

            if (result.IsLockedOut)
                ModelState.AddModelError("", "Account locked. Try again later.");
            else
                ModelState.AddModelError("", "Invalid email or password.");

            return View(model);
        }

        // ── REGISTER (Admin only) ──────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Create role if it doesn't exist
            if (!await _roleManager.RoleExistsAsync(model.Role))
                await _roleManager.CreateAsync(new IdentityRole(model.Role));

            var user = new AppUser
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
                Phone = model.Phone,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                TempData["Success"] = $"Staff account created for {model.FullName}";
                return RedirectToAction("Index", "Staff");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // ── LOGOUT ─────────────────────────────────────
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ── SEED ADMIN (first time setup) ──────────────
        [HttpGet]
        public async Task<IActionResult> SeedAdmin()
        {
            // Only runs if no users exist
            if (_userManager.Users.Any())
                return RedirectToAction("Login");

            // Create roles
            string[] roles = { "Admin", "Manager", "Cashier", "Washer" };
            foreach (var role in roles)
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

            // Create default admin
            var admin = new AppUser
            {
                FullName = "System Admin",
                UserName = "admin@splashcity.com",
                Email = "admin@splashcity.com",
                IsActive = true
            };

            var result = await _userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(admin, "Admin");

            TempData["Success"] = "Admin created! Email: admin@splashcity.com | Password: Admin@123";
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}