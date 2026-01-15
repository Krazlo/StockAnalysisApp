using StockDataService.Models;

namespace StockDataService.Services
{
    public interface IIndicatorCalculator
    {
        StockIndicators CalculateIndicators(List<StockData> historicalData, StockData currentData);
        public decimal CalculateSMA(List<decimal> prices, int period);

        public decimal CalculateEMA(List<decimal> prices, int period);

        public decimal CalculateRSI(List<decimal> prices, int period);

        public (decimal Line, decimal Signal, decimal Histogram) CalculateMACD(List<decimal> prices);

        public List<decimal?> CalculateEMASeries(List<decimal> prices, int period);

        public (decimal Upper, decimal Middle, decimal Lower, decimal PercentB) CalculateBollingerBands(List<decimal> prices, int period, decimal deviation, decimal currentPrice);

        public decimal CalculateDayChangePercent(List<StockData> sortedData, StockData currentData);

        public List<decimal> CalculateOBV(List<StockData> historicalData);

        public string CalculateOBVTrend(List<decimal> obv);

        public string CalculateRSITrend(List<decimal> prices);

        public string CalculateMACDState(decimal macd, decimal signal, decimal histogram);
    }
}
