using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UIApplication.Models;
using Microsoft.EntityFrameworkCore;
using UIApplication.Data;

namespace UIApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DevController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DevController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // 1. User Stats
            var users = await _context.Users
                .Select(u => new UserStatsViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    PromptsUsed = u.PromptsUsed,
                    StocksAnalyzed = u.StocksAnalyzed
                })
                .ToListAsync();

            // Check for Admin role status
            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id);
                if (appUser != null)
                {
                    user.IsAdmin = await _userManager.IsInRoleAsync(appUser, "Admin");
                }
            }

            // 2. Total Stock Data Collected (Requires an API call to StockDataService)
            // Since we cannot make a direct API call here without a service, we will mock the total count for now
            // and assume a service will be implemented later to fetch this from StockDataService's database.
            // For now, we will use a placeholder.
            long totalHistoricalDataCount = 0; // Placeholder

            var viewModel = new DevDashboardViewModel
            {
                UserStats = users,
                TotalHistoricalDataCount = totalHistoricalDataCount
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign role
                    if (model.IsAdmin)
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    // Handle error if deletion fails
                    // For simplicity, we just redirect back to index
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                    await _userManager.AddToRoleAsync(user, "User");
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, "User");
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }

    public class UserStatsViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public int PromptsUsed { get; set; }
        public int StocksAnalyzed { get; set; }
    }

    public class DevDashboardViewModel
    {
        public List<UserStatsViewModel> UserStats { get; set; }
        public long TotalHistoricalDataCount { get; set; }
    }
}
