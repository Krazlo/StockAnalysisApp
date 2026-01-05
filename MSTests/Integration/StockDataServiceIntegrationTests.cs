using AIAnalysisService.Models;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace MSTests.StockDataServiceIntegrationTests
{
    [TestClass]
    public class StockDataServiceIntegrationTests
    {
        private static StockDataServiceFactory _stockFactory;
        private static AIAnalysisServiceFactory _analysisFactory;

        [ClassInitialize]
        public static async Task Setup(TestContext context)
        {
            _stockFactory = new StockDataServiceFactory();
            _analysisFactory = new AIAnalysisServiceFactory();
        }

        #region Controller Tests
        //Test af controllers, DI, serialization. 

        [TestMethod]
        public async Task GetStockIndicators_ReturnsCalculatedIndicators()
        {
            var client = _stockFactory.CreateClient();

            var response = await client.GetAsync(
              "/api/stock/AAPL?exchange=US"
            );

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var indicators = doc.RootElement.GetProperty("indicators");

            Assert.IsTrue(indicators.TryGetProperty("rsI_14", out _));
            Assert.IsTrue(indicators.TryGetProperty("smA_50", out _));
        }

        [TestMethod]
        public async Task AnalyzeStocks_ReturnsAnalysis()
        {
            var client = _analysisFactory.CreateClient();

            var request = new AnalysisRequest
            {
                Prompt = "Analyze this stock",
                Symbol = "AAPL",
                StockData = new StockDataInfo
                {
                    Symbol = "AAPL",
                    CurrentPrice = 150,
                    Date = DateTime.Now,
                    Indicators = TestHelpers.GenerateDummyIndicators()
                }
            };

            var response = await client.PostAsJsonAsync(
                "/api/analysis/analyze",
                request
            );

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            Assert.IsTrue(doc.RootElement.TryGetProperty("success", out var success));
            Assert.IsTrue(success.GetBoolean());

            Assert.IsTrue(doc.RootElement.TryGetProperty("analysis", out var analysis));
            Assert.AreEqual("Mega god analyse", analysis.GetString());

            Assert.AreEqual("AAPL", doc.RootElement.GetProperty("symbol").GetString());
        }
        #endregion

        #region Cleanup
        [ClassCleanup]
        public static async Task Cleanup()
        {
            _stockFactory.Dispose();
            _analysisFactory.Dispose();
        }
        #endregion

    }
}
