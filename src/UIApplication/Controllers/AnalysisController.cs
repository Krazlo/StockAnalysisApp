using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UIApplication.Data;
using UIApplication.Models;
using UIApplication.Services;

namespace UIApplication.Controllers
{
    [Authorize]
    public class AnalysisController : Controller
    {
        private readonly IAnalysisService _analysisService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnalysisController(
            IAnalysisService analysisService,
            UserManager<ApplicationUser> userManager)
        {
            _analysisService = analysisService;
            _userManager = userManager;
        }

        // ---------------- INDEX (Main Analysis Page) ----------------
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var analyses = await _analysisService.GetUserAnalysesAsync(userId);

            var viewModel = analyses.Select(a => new AnalysisOverviewViewModel
            {
                Id = a.Id,
                Symbol = a.Symbol,
                AnalysisDate = a.AnalysisDate,
                KeyIndicators = a.KeyIndicators,
                UsedPromptTitle = a.UsedPrompt?.Title ?? "N/A"
            }).ToList();

            return View(viewModel);
        }

        // ---------------- OVERVIEW (List of previous analyses) ----------------
        public async Task<IActionResult> Overview()
        {
            var userId = _userManager.GetUserId(User);
            var analyses = await _analysisService.GetUserAnalysesAsync(userId);

            var viewModel = analyses
                .OrderByDescending(a => a.AnalysisDate)
                .Select(a => new AnalysisOverviewViewModel
                {
                    Id = a.Id,
                    Symbol = a.Symbol,
                    AnalysisDate = a.AnalysisDate,
                    KeyIndicators = a.KeyIndicators,
                    UsedPromptTitle = a.UsedPrompt?.Title ?? "N/A"
                })
                .ToList();

            return View("Overview", viewModel);
        }

        // ---------------- DETAILS (Single analysis view) ----------------
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var analyses = await _analysisService.GetUserAnalysesAsync(userId);
            var analysis = analyses.FirstOrDefault(a => a.Id == id);

            if (analysis == null)
                return NotFound();

            var viewModel = new AnalysisDetailsViewModel
            {
                Id = analysis.Id,
                Symbol = analysis.Symbol,
                AnalysisDate = analysis.AnalysisDate,
                AnalysisResult = analysis.AnalysisResult,
                KeyIndicators = analysis.KeyIndicators,
                UsedPromptTitle = analysis.UsedPrompt?.Title ?? "N/A"
            };

            return View("Details", viewModel);
        }
    }

    public class AnalysisOverviewViewModel
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public DateTime AnalysisDate { get; set; }
        public string KeyIndicators { get; set; }
        public string UsedPromptTitle { get; set; }
    }

    public class AnalysisDetailsViewModel : AnalysisOverviewViewModel
    {
        public string AnalysisResult { get; set; }
    }
}
