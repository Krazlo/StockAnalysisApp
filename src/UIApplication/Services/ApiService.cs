using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Text.Json;
using UIApplication.DTO;

namespace UIApplication.Services
{
    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiService> _logger;

        public ApiService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> AnalyzeStockAsync(string prompt, string symbol, string exchange)
        {
            try
            {
                var gatewayUrl = _configuration["ApiGateway:BaseUrl"];
                var client = _httpClientFactory.CreateClient();

                // Step 1: Get stock data with indicators
                _logger.LogInformation("Fetching stock data for symbol: {Symbol}", symbol);
                var stockDataUrl = $"{gatewayUrl}/stock/{symbol}?exchange={exchange}";
                var stockDataResponse = await client.GetAsync(stockDataUrl);

                if (!stockDataResponse.IsSuccessStatusCode)
                {
                    var errorContent = await stockDataResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Error fetching stock data: {StatusCode} - {Content}", 
                        stockDataResponse.StatusCode, errorContent);
                    return $"Error fetching stock data: {stockDataResponse.StatusCode}";
                }

                var stockDataJson = await stockDataResponse.Content.ReadAsStringAsync();
                var stockData = JsonSerializer.Deserialize<JsonElement>(stockDataJson);

                // Step 2: Prepare analysis request
                var analysisRequest = new
                {
                    prompt = prompt,
                    symbol = symbol,
                    stockData = new
                    {
                        symbol = stockData.GetProperty("symbol").GetString(),
                        currentPrice = stockData.GetProperty("currentData").GetProperty("close").GetDecimal(),
                        date = stockData.GetProperty("currentData").GetProperty("date").GetDateTime(),
                        indicators = stockData.GetProperty("indicators")
                    }
                };

                // Step 3: Send to AI Analysis Service
                _logger.LogInformation("Sending analysis request for symbol: {Symbol}", symbol);
                var analysisUrl = $"{gatewayUrl}/analysis/analyze";
                var jsonContent = JsonSerializer.Serialize(analysisRequest);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var analysisResponse = await client.PostAsync(analysisUrl, httpContent);

                if (!analysisResponse.IsSuccessStatusCode)
                {
                    var errorContent = await analysisResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Error from analysis service: {StatusCode} - {Content}", 
                        analysisResponse.StatusCode, errorContent);
                    return $"Error from analysis service: {analysisResponse.StatusCode}";
                }

                var analysisJson = await analysisResponse.Content.ReadAsStringAsync();
                var analysisResult = JsonSerializer.Deserialize<JsonElement>(analysisJson);

                var analysis = analysisResult.GetProperty("analysis").GetString();
                _logger.LogInformation("Successfully received analysis for symbol: {Symbol}", symbol);

                return analysis ?? "No analysis available";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing stock for symbol: {Symbol}", symbol);
                return $"Error: {ex.Message}";
            }
        }

        // single image
        public async Task<string> AnalyzeStockImageAsync(string prompt, string base64Image)
        {
            try
            {
                var gatewayUrl = _configuration["ApiGateway:BaseUrl"];
                var client = _httpClientFactory.CreateClient();

                var payload = new
                {
                    prompt = prompt,
                    image = new
                    {
                        mimeType = "image/png",
                        data = base64Image
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{gatewayUrl}/analysis/image-analyze";

                var response = await client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Vision analysis failed: {StatusCode} - {Error}",
                        response.StatusCode, error);

                    return $"Error from analysis service: {response.StatusCode}";
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var doc = JsonSerializer.Deserialize<JsonElement>(responseJson);

                return doc.GetProperty("analysis").GetString()
                    ?? "No analysis returned.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vision analyze failed");
                return $"Error: {ex.Message}";
            }
        }
        // multi image
        public async Task<string> AnalyzeStockImagesAsync(string prompt, List<ImageInputDto> images)
        {
            var gatewayUrl = _configuration["ApiGateway:BaseUrl"];
            var client = _httpClientFactory.CreateClient();

            var payload = new
            {
                prompt = prompt,
                images = images.Select(i => new
                {
                    base64 = i.Base64,
                    mimeType = i.MimeType,
                    description = i.Description
                })
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{gatewayUrl}/analysis/image-analyze";

            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Multi-image analysis failed: {Status} - {Error}",
                    response.StatusCode, error);

                return $"Error from analysis service: {response.StatusCode}";
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(responseJson);

            return doc
                .RootElement
                .GetProperty("analysis")
                .GetString()
                ?? "No analysis returned.";
        }


    }
}
