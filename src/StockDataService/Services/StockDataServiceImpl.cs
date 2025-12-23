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
        private readonly IHistoricalDataService _historicalDataService;
        private readonly ILogger<StockDataServiceImpl> _logger;

        public StockDataServiceImpl(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IMemoryCache cache,
            IIndicatorCalculator indicatorCalculator,
            IHistoricalDataService historicalDataService,
            ILogger<StockDataServiceImpl> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _cache = cache;
            _historicalDataService = historicalDataService;
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

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var content = await response.Content.ReadAsStringAsync();
            List<EodhdResponse>? eodhdResponse = JsonSerializer.Deserialize<List<EodhdResponse>>(content, options);

            if (eodhdResponse == null || eodhdResponse.Count == 0)
            {
                _logger.LogError("Full Eodhd response: {Json}", content);
                throw new InvalidOperationException($"No data returned for symbol: {symbol}");
            }
            _logger.LogInformation($"Fetched {eodhdResponse.Count} lines of data from EODHD");

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

            // --- Save historical data to database ---
            await _historicalDataService.SaveHistoricalDataAsync(symbol, historicalData);
            var currentData = historicalData.First();

            // Calculate indicators
            StockIndicators indicators = _indicatorCalculator.CalculateIndicators(historicalData, currentData);

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

            _logger.LogInformation($"DEBUG: Indicators: ", indicators);
            _logger.LogInformation($"SMA_20 = {indicators.SMA_20}");
            _logger.LogInformation($"SMA_200 = {indicators.SMA_50}");
            _logger.LogInformation($"SMA_50 = {indicators.SMA_200}");
            _logger.LogInformation($"EMA_12 = {indicators.EMA_12}");
            _logger.LogInformation($"EMA_26 = {indicators.EMA_26}");
            _logger.LogInformation($"RSI_14 = {indicators.RSI_14}");
            _logger.LogInformation($"RSITrend = {indicators.RSITrend}");
            _logger.LogInformation($"MACD_Line = {indicators.MACD_Line}");
            _logger.LogInformation($"MACD_Signal = {indicators.MACD_Signal}");
            _logger.LogInformation($"MACD_Histogram = {indicators.MACD_Histogram}");
            _logger.LogInformation($"MACDState = {indicators.MACDState}");
            _logger.LogInformation($"BollingerUpper = {indicators.BollingerUpper}");
            _logger.LogInformation($"BollingerMiddle = {indicators.BollingerMiddle}");
            _logger.LogInformation($"BollingerLower = {indicators.BollingerLower}");
            _logger.LogInformation($"AverageVolume_20 = {indicators.AverageVolume_20}");
            _logger.LogInformation($"VolumeChangePercent = {indicators.VolumeChangePercent}");

            _logger.LogInformation($"CurrentPrice = {indicators.CurrentPrice}");
            _logger.LogInformation($"DayChangePercent = {indicators.DayChangePercent}");
            _logger.LogInformation($"Week52High = {indicators.Week52High}");
            _logger.LogInformation($"Week52Low = {indicators.Week52Low}");
            _logger.LogInformation($"PriceVsSMA50 = {indicators.PriceVsSMA50}");
            _logger.LogInformation($"PriceVsSMA200 = {indicators.PriceVsSMA200}");
            _logger.LogInformation($"OBV = {indicators.OBV}");
            _logger.LogInformation($"OBVTrend = {indicators.OBVTrend}");
            //TODO: DCF & VBO (Volume On Balance)

            _logger.LogInformation("Successfully fetched and cached stock data for symbol: {Symbol}", symbol);

            return result;
        }
    }
}
