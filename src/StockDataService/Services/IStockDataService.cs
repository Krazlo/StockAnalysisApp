using StockDataService.Models;

namespace StockDataService.Services
{
    public interface IStockDataService
    {
        Task<StockDataWithIndicators> GetStockDataWithIndicatorsAsync(string symbol, string exchange);
    }
}
