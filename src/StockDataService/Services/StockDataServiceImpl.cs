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

        public async Task<StockDataWithIndicators> GetStockDataWithIndicatorsAsync(string symbol)
        {
            // Check cache first
            string cacheKey = $"stock_{symbol}";
            if (_cache.TryGetValue(cacheKey, out StockDataWithIndicators? cachedData) && cachedData != null)
            {
                _logger.LogInformation("Returning cached data for symbol: {Symbol}", symbol);
                return cachedData;
            }

            // Fetch from Alpha Vantage
            var apiKey = _configuration["AlphaVantage:ApiKey"];
            var baseUrl = _configuration["AlphaVantage:BaseUrl"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Alpha Vantage API key is not configured");
            }

            var client = _httpClientFactory.CreateClient();
            var url = $"{baseUrl}?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize=full&apikey={apiKey}";

            _logger.LogInformation("Fetching stock data for symbol: {Symbol}", symbol);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var alphaVantageResponse = JsonSerializer.Deserialize<AlphaVantageTimeSeriesResponse>(content);

            if (alphaVantageResponse?.TimeSeriesDaily == null || alphaVantageResponse.TimeSeriesDaily.Count == 0)
            {
                throw new InvalidOperationException($"No data returned for symbol: {symbol}");
            }

            // Convert to StockData list
            var historicalData = new List<StockData>();
            foreach (var kvp in alphaVantageResponse.TimeSeriesDaily)
            {
                if (DateTime.TryParse(kvp.Key, out DateTime date))
                {
                    historicalData.Add(new StockData
                    {
                        Symbol = symbol,
                        Date = date,
                        Open = decimal.Parse(kvp.Value.Open, CultureInfo.InvariantCulture),
                        High = decimal.Parse(kvp.Value.High, CultureInfo.InvariantCulture),
                        Low = decimal.Parse(kvp.Value.Low, CultureInfo.InvariantCulture),
                        Close = decimal.Parse(kvp.Value.Close, CultureInfo.InvariantCulture),
                        Volume = long.Parse(kvp.Value.Volume, CultureInfo.InvariantCulture)
                    });
                }
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
