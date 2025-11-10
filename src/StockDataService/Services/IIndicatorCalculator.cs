using StockDataService.Models;

namespace StockDataService.Services
{
    public interface IIndicatorCalculator
    {
        StockIndicators CalculateIndicators(List<StockData> historicalData, StockData currentData);
    }
}
