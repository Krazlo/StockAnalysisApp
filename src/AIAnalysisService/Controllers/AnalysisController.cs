using Microsoft.AspNetCore.Mvc;
using AIAnalysisService.Models;
using AIAnalysisService.Services;
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

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", service = "AI Analysis Service" });
        }
    }
}
