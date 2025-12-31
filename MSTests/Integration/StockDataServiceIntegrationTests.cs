using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.ComponentModel;
using System.Net;
using PactNet;
using System.Net.Http.Json;
using System.Text.Json;

namespace MSTests.StockDataServiceIntegrationTests
{
    [TestClass]
    public class StockDataServiceIntegrationTests
    {
        private static DotNet.Testcontainers.Containers.IContainer _wireMockContainer;
        private static StockDataServiceFactory _factory;
        private static PactConfig _config;

        [ClassInitialize]
        public static async Task Setup(TestContext context)
        {
            _wireMockContainer = new ContainerBuilder()
                .WithImage("wiremock/wiremock:latest")
                .WithPortBinding(8080, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(8080))
                .Build();

            await _wireMockContainer.StartAsync();

            var wireMockUrl = $"http://localhost:{_wireMockContainer.GetMappedPublicPort(8080)}";
            var inMemoryConfig = new Dictionary<string, string?>
            {
                ["Eodhd:BaseUrl"] = wireMockUrl,
            };

            _factory = new StockDataServiceFactory();

            _config = new PactConfig
            {
                PactDir = "/pacts",
                DefaultJsonSettings = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                }
            };
        }

        [TestMethod]
        public async Task GetStockIndicators_ReturnsCalculatedIndicators()
        {
            var client = _factory.CreateClient();

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
        public async Task StockDataService_Contract_With_AIAnalysisService()
        {
            IPactBuilderV4 pact = Pact.V4("StockDataService", "AIAnalysisService", _config).WithHttpInteractions();

            pact
              .UponReceiving("Valid analysis request")
              .WithRequest(HttpMethod.Post, "/analyze")
              .WithJsonBody(new
              {
                  symbol = "AAPL",
                  indicators = new { RSI_14 = 55 }
              })
              .WillRespond()
              .WithStatus(HttpStatusCode.OK)
              .WithJsonBody(new
              {
                  summary = "Bullish outlook"
              });

            await pact.VerifyAsync(async ctx =>
            {
                var client = new HttpClient { BaseAddress = ctx.MockServerUri };

                var response = await client.PostAsJsonAsync("/analyze", new
                {
                    symbol = "AAPL",
                    indicators = new { RSI_14 = 55 }
                });

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [ClassCleanup]
        public static async Task Cleanup()
        {
            await _wireMockContainer.DisposeAsync();
            _factory.Dispose();
        }
    }
}
