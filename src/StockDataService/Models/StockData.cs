 namespace StockDataService.Models
{
    public class StockData
    {
        public string Symbol { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }

    public class StockDataWithIndicators
    {
        public string Symbol { get; set; } = string.Empty;
        public StockData CurrentData { get; set; } = new();
        public List<StockData> HistoricalData { get; set; } = new();
        public StockIndicators Indicators { get; set; } = new();
        public DateTime RetrievedAt { get; set; }
    }
}
