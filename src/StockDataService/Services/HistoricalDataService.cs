using Microsoft.EntityFrameworkCore;
using StockDataService.Data;
using StockDataService.Models;

namespace StockDataService.Services
{
    public class HistoricalDataService : IHistoricalDataService
    {
        private readonly StockDataDbContext _context;
        private readonly ILogger<HistoricalDataService> _logger;

        public HistoricalDataService(StockDataDbContext context, ILogger<HistoricalDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SaveHistoricalDataAsync(string symbol, IEnumerable<StockData> data)
        {
            if (data == null || !data.Any())
            {
                _logger.LogWarning("Attempted to save empty or null historical data for symbol {Symbol}", symbol);
                return;
            }

            var historicalData = data.Select(d => new StockData
            {
                Symbol = symbol,
                Date = d.Date.Date, // Ensure only date part is used for deduplication
                Open = d.Open,
                High = d.High,
                Low = d.Low,
                Close = d.Close,
                Volume = d.Volume
            }).ToList();

            // Deduplication logic: Check for existing records before adding
            var existingRecords = await _context.StockData
                .Where(h => h.Symbol == symbol && historicalData.Select(d => d.Date).Contains(h.Date))
                .Select(h => h.Date)
                .ToListAsync();

            var newRecords = historicalData
                .Where(d => !existingRecords.Contains(d.Date))
                .ToList();

            if (newRecords.Any())
            {
                _context.StockData.AddRange(newRecords);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Saved {Count} new historical data records for symbol {Symbol}", newRecords.Count, symbol);
            }
            else
            {
                _logger.LogInformation("No new historical data records to save for symbol {Symbol}", symbol);
            }
        }

        public async Task<List<StockData>> GetHistoricalDataAsync(string symbol)
        {
            return await _context.StockData
                .Where(h => h.Symbol == symbol)
                .OrderBy(h => h.Date)
                .ToListAsync();
        }
    }
}
