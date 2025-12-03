namespace StockDataService.DTO
{
    public class StockDataDto
    {
        public string Symbol { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }
}
