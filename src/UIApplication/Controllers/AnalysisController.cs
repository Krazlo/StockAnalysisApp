using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UIApplication.Data;
using UIApplication.DTO;
using UIApplication.Models;
using UIApplication.Services;

namespace UIApplication.Controllers
{
    [Authorize]
    public class AnalysisController : Controller
    {
        private readonly IAnalysisService _analysisService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileStorageService _fileStorage;
        private readonly IApiService _apiService;

        public AnalysisController(
            IAnalysisService analysisService,
            UserManager<ApplicationUser> userManager,
            IFileStorageService fileStorage,
            IApiService apiService)
        {
            _analysisService = analysisService;
            _userManager = userManager;
            _fileStorage = fileStorage;
            _apiService = apiService;
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

        // ------------ IMAGE ------------
        [HttpGet]
        [Authorize]
        public IActionResult Image()
        {
            return View(new ImageAnalysisViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnalyzeImage(ImageAnalysisViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Prompt) ||
                model.Images == null ||
                model.Images.Count == 0)
            {
                model.ErrorMessage = "Prompt og mindst ét billede er påkrævet.";
                return View("Image", model);
            }

            if (model.Images.Count > 5)
            {
                model.ErrorMessage = "Du kan maks uploade 5 billeder.";
                return View("Image", model);
            }

            try
            {
                var imageInputs = new List<ImageInputDto>();

                foreach (var image in model.Images)
                {
                    using var ms = new MemoryStream();
                    await image.CopyToAsync(ms);

                    imageInputs.Add(new ImageInputDto
                    {
                        Base64 = Convert.ToBase64String(ms.ToArray()),
                        MimeType = image.ContentType,
                        Description = image.FileName
                    });
                }

                var analysis = await _apiService.AnalyzeStockImagesAsync(
                    model.Prompt,
                    imageInputs
                );

                model.AnalysisResult = analysis;
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
            }

            return View("Image", model);
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
