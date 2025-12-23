using StockDataService.Models;

namespace StockDataService.Services
{
    public interface IHistoricalDataService
    {
        Task SaveHistoricalDataAsync(string symbol, IEnumerable<StockData> data);
        Task<List<StockData>> GetHistoricalDataAsync(string symbol);
    }
}
