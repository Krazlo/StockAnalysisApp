using StockDataService.Models;
using StockDataService.Services;

public class FakeStockDataServiceImpl : IStockDataService
{
    public Task<StockDataWithIndicators> GetStockDataWithIndicatorsAsync(string symbol, string exchange)
    {
        var data = new List<StockData>();
        decimal price = 100;

        for (int i = 0; i < 100; i++)
        {
            data.Add(new StockData
            {
                Date = DateTime.Today.AddDays(-300 + i),
                Open = price,
                High = price + 1,
                Low = price - 1,
                Close = price,
                Volume = 1_000_000
            });

            price += 0.5m;
        }
        var current = data.First();

        var calculator = new IndicatorCalculator();
        // Calculate indicators
        StockIndicators indicators = calculator.CalculateIndicators(data, current);

        var result = new StockDataWithIndicators
        {
            Symbol = symbol,
            CurrentData = current,
            HistoricalData = data.Take(100).ToList(), // Keep last 100 days
            Indicators = indicators,
            RetrievedAt = DateTime.UtcNow
        };


        return Task.FromResult(result);
    }
}