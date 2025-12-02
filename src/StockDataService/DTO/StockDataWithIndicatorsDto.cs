using StockDataService.Models;

namespace StockDataService.DTO
{
    public class StockDataWithIndicatorsDto
    {
        public string Symbol { get; set; } = string.Empty;
        public StockDataDto CurrentData { get; set; } = new();
        public List<StockDataDto> HistoricalData { get; set; } = new();
        public StockIndicators Indicators { get; set; } = new();
        public DateTime RetrievedAt { get; set; }
    }
}
