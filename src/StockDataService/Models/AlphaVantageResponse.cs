using System.Text.Json.Serialization;

namespace StockDataService.Models
{
    public class AlphaVantageTimeSeriesResponse
    {
        [JsonPropertyName("Meta Data")]
        public MetaData? MetaData { get; set; }

        [JsonPropertyName("Time Series (Daily)")]
        public Dictionary<string, DailyData>? TimeSeriesDaily { get; set; }
    }

    public class MetaData
    {
        [JsonPropertyName("1. Information")]
        public string Information { get; set; } = string.Empty;

        [JsonPropertyName("2. Symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("3. Last Refreshed")]
        public string LastRefreshed { get; set; } = string.Empty;

        [JsonPropertyName("4. Output Size")]
        public string OutputSize { get; set; } = string.Empty;

        [JsonPropertyName("5. Time Zone")]
        public string TimeZone { get; set; } = string.Empty;
    }

    public class DailyData
    {
        [JsonPropertyName("1. open")]
        public string Open { get; set; } = string.Empty;

        [JsonPropertyName("2. high")]
        public string High { get; set; } = string.Empty;

        [JsonPropertyName("3. low")]
        public string Low { get; set; } = string.Empty;

        [JsonPropertyName("4. close")]
        public string Close { get; set; } = string.Empty;

        [JsonPropertyName("5. volume")]
        public string Volume { get; set; } = string.Empty;
    }
}
