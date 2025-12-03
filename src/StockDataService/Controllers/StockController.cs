using Microsoft.AspNetCore.Mvc;
using StockDataService.Services;

namespace StockDataService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IStockDataService _stockDataService;
        private readonly ILogger<StockController> _logger;

        public StockController(IStockDataService stockDataService, ILogger<StockController> logger)
        {
            _stockDataService = stockDataService;
            _logger = logger;
        }

        [HttpGet("{symbol}")]
        public async Task<IActionResult> GetStockData(string symbol, [FromQuery] string exchange)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(symbol))
                {
                    return BadRequest(new { error = "Symbol is required" });
                }

                if (string.IsNullOrWhiteSpace(exchange))
                {
                    return BadRequest(new { error = "Exchange is required" });
                }

                var result = await _stockDataService.GetStockDataWithIndicatorsAsync(symbol.ToUpper(), exchange.ToUpper());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error fetching stock data for symbol: {Symbol}", symbol);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching stock data for symbol: {Symbol}", symbol);
                return StatusCode(500, new { error = "An error occurred while fetching stock data" });
            }
        }

        [HttpGet("{symbol}/indicators")]
        public async Task<IActionResult> GetStockIndicators(string symbol, [FromQuery] string exchange)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(symbol))
                {
                    return BadRequest(new { error = "Symbol is required" });
                }

                var result = await _stockDataService.GetStockDataWithIndicatorsAsync(symbol.ToUpper(), exchange.ToUpper());
                return Ok(new
                {
                    symbol = result.Symbol,
                    currentPrice = result.CurrentData.Close,
                    indicators = result.Indicators,
                    retrievedAt = result.RetrievedAt
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error fetching stock indicators for symbol: {Symbol}", symbol);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching stock indicators for symbol: {Symbol}", symbol);
                return StatusCode(500, new { error = "An error occurred while fetching stock indicators" });
            }
        }
    }
}
