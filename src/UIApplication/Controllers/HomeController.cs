using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UIApplication.Data;
using UIApplication.Models;
using UIApplication.Services;
using Ganss.Xss;
using System.Security.Authentication;


namespace UIApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiService _apiService;
        private readonly IAnalysisService _analysisService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<HomeController> _logger;
        private HtmlSanitizer sanitizer;
        private readonly IFileStorageService _fileStorage;

        public HomeController(
            IApiService apiService,
            IAnalysisService analysisService,
            UserManager<ApplicationUser> userManager,
            ILogger<HomeController> logger,
            IFileStorageService fileStorage)
        {
            _apiService = apiService;
            _analysisService = analysisService;
            _userManager = userManager;
            _logger = logger;
            sanitizer = new HtmlSanitizer();
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AnalysisViewModel();
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                model.SavedPrompts = await _analysisService.GetUserPromptsAsync(userId);
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analyze(string prompt, string symbol, int? usedPromptId, string exchange)
        {
            var userId = _userManager.GetUserId(User);
            var savedPrompts = await _analysisService.GetUserPromptsAsync(userId);
            var model = new AnalysisViewModel
            {
                Prompt = prompt,
                Symbol = symbol,
                Exchange = exchange,
                SavedPrompts = savedPrompts,
                UsedPromptId = usedPromptId
            };

            if (string.IsNullOrWhiteSpace(prompt) || string.IsNullOrWhiteSpace(symbol))
            {
                model.ErrorMessage = "Both prompt and symbol are required";
                return View("Index", model);
            }

            try
            {
                _logger.LogInformation($"Analyzing stock: symbol = {symbol}, prompt = {prompt}");
                model.IsLoading = true;
                var analysis = await _apiService.AnalyzeStockAsync(prompt, symbol.ToUpper(), exchange.ToUpper());   
                model.Analysis = sanitizer.Sanitize(analysis);
                model.IsLoading = false;

                // 1. Save Analysis
                // NOTE: We need to extract KeyIndicators from the analysis string, which is not possible here.
                // For now, we will save the first 100 characters as a placeholder for KeyIndicators.
                var keyIndicators = analysis.Length > 100 ? analysis.Substring(0, 100) + "..." : analysis;
                await _analysisService.SaveStockAnalysisAsync(userId, symbol.ToUpper(), analysis, keyIndicators, usedPromptId);

                // 2. Update User Stats
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    user.PromptsUsed++;
                    user.StocksAnalyzed++;
                    await _userManager.UpdateAsync(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing stock");
                model.ErrorMessage = $"Error: {ex.Message}";
                model.IsLoading = false;
            }

            return View("Index", model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePrompt(string name, string prompt, string symbol, string exchange)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(prompt) || string.IsNullOrWhiteSpace(symbol) || string.IsNullOrWhiteSpace(exchange))
            {
                return BadRequest("Titel, prompt, symbol og exchange skal udfyldes.");
            }

            var userId = _userManager.GetUserId(User);
            await _analysisService.SaveUserPromptAsync(userId, name, symbol, exchange, prompt); //TODO AXEL
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePrompt(int id)
        {
            var userId = _userManager.GetUserId(User);
            await _analysisService.DeleteUserPromptAsync(userId, id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> LoadPrompt(int id)
        {
            var userId = _userManager.GetUserId(User);
            var savedPrompts = await _analysisService.GetUserPromptsAsync(userId);
            var prompt = savedPrompts.FirstOrDefault(p => p.Id == id);

            if (prompt == null || prompt.UserId != userId)
            {
                return NotFound();
            }

            var model = new AnalysisViewModel
            {
                Prompt = prompt.Prompt,
                Symbol = prompt.Symbol,
                Exchange = prompt.Exchange,
                SavedPrompts = savedPrompts,
                UsedPromptId = id
            };

            return View("Index", model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
