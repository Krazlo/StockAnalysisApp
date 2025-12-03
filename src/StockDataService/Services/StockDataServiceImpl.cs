using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using StockDataService.Models;

namespace StockDataService.Services
{
    public class StockDataServiceImpl : IStockDataService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly IIndicatorCalculator _indicatorCalculator;
        private readonly ILogger<StockDataServiceImpl> _logger;

        public StockDataServiceImpl(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IMemoryCache cache,
            IIndicatorCalculator indicatorCalculator,
            ILogger<StockDataServiceImpl> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _cache = cache;
            _indicatorCalculator = indicatorCalculator;
            _logger = logger;
        }

        public async Task<StockDataWithIndicators> GetStockDataWithIndicatorsAsync(string symbol, string exchange)
        {
            // Check cache first
            string cacheKey = $"stock_{symbol}";
            if (_cache.TryGetValue(cacheKey, out StockDataWithIndicators? cachedData) && cachedData != null)
            {
                _logger.LogInformation("Returning cached data for symbol: {Symbol}", symbol);
                return cachedData;
            }

            // Fetch from EODHD
            var apiKey = _configuration["EODHD:ApiKey"];
            var baseUrl = _configuration["EODHD:BaseUrl"];
            string from = DateTime.Now.AddYears(-20).ToString("yyyy-MM-dd");
            string to = DateTime.Now.ToString("yyyy-MM-dd");
            string period = "d";

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("EODHD API key is not configured");
            }

            var client = _httpClientFactory.CreateClient();
            var url = $"{baseUrl}/eod/{symbol}.{exchange}?from={from}&to={to}&period={period}&fmt=json&api_token={apiKey}";

            _logger.LogInformation("Fetching stock data for symbol: {Symbol}", symbol);

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException($"There was an error while executing the Stock Data Fetch HTTP query. Reason: {response.StatusCode}, {response.ReasonPhrase}");
            }

            var content = await response.Content.ReadAsStringAsync();
            List<EodhdResponse>? eodhdResponse = JsonSerializer.Deserialize<List<EodhdResponse>>(content);

            if (eodhdResponse == null || eodhdResponse.Count == 0)
            {
                _logger.LogError("Full Eodhd response: {Json}", content);
                throw new InvalidOperationException($"No data returned for symbol: {symbol}");
            }

            // Convert to StockData list
            var historicalData = new List<StockData>();
            foreach (var item in eodhdResponse)
            {
                historicalData.Add(new StockData
                {
                    Symbol = symbol,
                    Date = item.Date.GetValueOrDefault(),
                    Open = item.Open.GetValueOrDefault(),
                    High = item.High.GetValueOrDefault(),
                    Low = item.Low.GetValueOrDefault(),
                    Close = item.Close.GetValueOrDefault(),
                    Volume = item.Volume.GetValueOrDefault()
                });
            }

            // Sort by date descending and get current data
            historicalData = historicalData.OrderByDescending(d => d.Date).ToList();
            var currentData = historicalData.First();

            // Calculate indicators
            var indicators = _indicatorCalculator.CalculateIndicators(historicalData, currentData);

            var result = new StockDataWithIndicators
            {
                Symbol = symbol,
                CurrentData = currentData,
                HistoricalData = historicalData.Take(100).ToList(), // Keep last 100 days
                Indicators = indicators,
                RetrievedAt = DateTime.UtcNow
            };

            // Cache the result
            var cacheExpiration = TimeSpan.FromMinutes(
                _configuration.GetValue<int>("Caching:StockDataCacheDurationMinutes", 5));
            _cache.Set(cacheKey, result, cacheExpiration);

            _logger.LogInformation("Successfully fetched and cached stock data for symbol: {Symbol}", symbol);

            return result;
        }
    }
}
