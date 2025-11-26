using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UIApplication.Models;
using UIApplication.Services;
using Ganss.Xss;


namespace UIApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILocalStorageService _localStorageService;
        private readonly ILogger<HomeController> _logger;
        private HtmlSanitizer sanitizer;

        public HomeController(
            IApiService apiService,
            ILocalStorageService localStorageService,
            ILogger<HomeController> logger)
        {
            _apiService = apiService;
            _localStorageService = localStorageService;
            _logger = logger;
            sanitizer = new HtmlSanitizer();
        }

        public async Task<IActionResult> Index()
        {
            var savedPrompts = await _localStorageService.GetSavedPromptsAsync();
            var model = new AnalysisViewModel
            {
                SavedPrompts = savedPrompts
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analyze(string prompt, string symbol)
        {
            var savedPrompts = await _localStorageService.GetSavedPromptsAsync();
            var model = new AnalysisViewModel
            {
                Prompt = prompt,
                Symbol = symbol,
                SavedPrompts = savedPrompts
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
                var analysis = await _apiService.AnalyzeStockAsync(prompt, symbol.ToUpper());   
                model.Analysis = sanitizer.Sanitize(analysis);
                model.IsLoading = false;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePrompt(string name, string prompt, string symbol)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(prompt) || string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Name, prompt, and symbol are required");
            }

            var savedPrompt = new SavedPrompt
            {
                Name = name,
                Prompt = prompt,
                Symbol = symbol,
                CreatedDate = DateTime.Now
            };

            await _localStorageService.SavePromptAsync(savedPrompt);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePrompt(string id)
        {
            await _localStorageService.DeletePromptAsync(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> LoadPrompt(string id)
        {
            var savedPrompts = await _localStorageService.GetSavedPromptsAsync();
            var prompt = savedPrompts.FirstOrDefault(p => p.Id == id);

            if (prompt == null)
            {
                return NotFound();
            }

            var model = new AnalysisViewModel
            {
                Prompt = prompt.Prompt,
                Symbol = prompt.Symbol,
                SavedPrompts = savedPrompts
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
