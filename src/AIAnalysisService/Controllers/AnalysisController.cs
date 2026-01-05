using Microsoft.AspNetCore.Mvc;
using AIAnalysisService.Models;
using AIAnalysisService.Services;
using AIAnalysisService.DTO;
using System.Text.Json;

namespace AIAnalysisService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IGeminiService _geminiService;
        private readonly ILogger<AnalysisController> _logger;

        public AnalysisController(IGeminiService geminiService, ILogger<AnalysisController> logger)
        {
            _geminiService = geminiService;
            _logger = logger;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeStock([FromBody] AnalysisRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Prompt))
                {
                    return BadRequest(new { error = "Prompt is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Symbol))
                {
                    return BadRequest(new { error = "Symbol is required" });
                }

                if (request.StockData == null)
                {
                    return BadRequest(new { error = "Stock data is required" });
                }

                var result = await _geminiService.AnalyzeStockAsync(request);

                if (!result.Success)
                {
                    return StatusCode(500, new { error = result.ErrorMessage });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing stock for symbol: {Symbol}", request.Symbol);
                return StatusCode(500, new { error = "An error occurred while analyzing the stock" });
            }
        }

        [HttpPost("image-analyze")]
        public async Task<IActionResult> AnalyzeImage([FromBody] ImageAnalysisRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Prompt) ||
                req.Images == null ||
                req.Images.Count == 0)
            {
                return BadRequest("Prompt and at least one image are required");
            }

            if (req.Images.Count > 5)
            {
                return BadRequest("Maximum 5 images are allowed");
            }

            var result = await _geminiService.AnalyzeImage(
                req.Prompt,
                req.Images
            );

            _logger.LogInformation(
    "Received {Count} images. First Base64 length: {Len}",
    req.Images.Count,
    req.Images[0].Base64?.Length
);

            return Ok(new
            {
                analysis = result
            });
        }


        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", service = "AI Analysis Service" });
        }
    }
}
