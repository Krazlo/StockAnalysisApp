namespace StockDataService.Models
{
    public class StockIndicators
    {
        // Simple Moving Averages
        public decimal SMA_20 { get; set; }
        public decimal SMA_50 { get; set; }
        public decimal SMA_200 { get; set; }

        // Exponential Moving Averages
        public decimal EMA_12 { get; set; }
        public decimal EMA_26 { get; set; }

        // RSI (Relative Strength Index)
        public decimal RSI_14 { get; set; }

        // MACD (Moving Average Convergence Divergence)
        public decimal MACD_Line { get; set; }
        public decimal MACD_Signal { get; set; }
        public decimal MACD_Histogram { get; set; }

        // Bollinger Bands
        public decimal BollingerUpper { get; set; }
        public decimal BollingerMiddle { get; set; }
        public decimal BollingerLower { get; set; }

        // Volume Analysis
        public long AverageVolume_20 { get; set; }
        public decimal VolumeChangePercent { get; set; }

        // Price Metrics
        public decimal CurrentPrice { get; set; }
        public decimal DayChangePercent { get; set; }
        public decimal Week52High { get; set; }
        public decimal Week52Low { get; set; }
        public string PriceVsSMA50 { get; set; } = string.Empty; // "Above" or "Below"
        public string PriceVsSMA200 { get; set; } = string.Empty; // "Above" or "Below"
    }
}
