namespace AIAnalysisService.Models
{
    public class AnalysisRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public StockDataInfo? StockData { get; set; }
    }

    public class StockDataInfo
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public DateTime Date { get; set; }
        public StockIndicatorsInfo Indicators { get; set; } = new();
    }

    public class StockIndicatorsInfo
    {
        public decimal SMA_20 { get; set; }
        public decimal SMA_50 { get; set; }
        public decimal SMA_200 { get; set; }
        public decimal EMA_12 { get; set; }
        public decimal EMA_26 { get; set; }
        public decimal RSI_14 { get; set; }
        public decimal MACD_Line { get; set; }
        public decimal MACD_Signal { get; set; }
        public decimal MACD_Histogram { get; set; }
        public decimal BollingerUpper { get; set; }
        public decimal BollingerMiddle { get; set; }
        public decimal BollingerLower { get; set; }
        public decimal BollingerPercentB { get; set; }
        public long AverageVolume_20 { get; set; }
        public decimal VolumeChangePercent { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal DayChangePercent { get; set; }
        public decimal Week52High { get; set; }
        public decimal Week52Low { get; set; }
        public string PriceVsSMA50 { get; set; } = string.Empty;
        public string PriceVsSMA200 { get; set; } = string.Empty;
        public List<decimal> OBV { get; set; }
        // Trends
        public string OBVTrend { get; set; } = string.Empty;
        public string RSITrend { get; set; } = string.Empty;
        public string MACDState { get; set; } = string.Empty;
    }
}
