using System.Net;
using PactNet;
using System.Net.Http.Json;
using System.Text.Json;

namespace MSTests.Contract
{
    [TestClass]
    public class ContractTests
    {
        private static PactConfig _config;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            var pactDir = Path.Combine(context.TestRunDirectory, "pacts");

            Directory.CreateDirectory(pactDir);

            _config = new PactConfig
            {
                PactDir = pactDir,
                DefaultJsonSettings = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
            };
        }

        [TestMethod]
        public async Task UIApplication_Contract_With_AIAnalysisService()
        {
            IPactBuilderV4 pact = Pact.V4("UIApplication", "AIAnalysisService", _config).WithHttpInteractions();

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

        [TestMethod]
        public async Task UIApplication_Contract_With_StockDataService()
        {
            IPactBuilderV4 pact =
                Pact.V4("UIApplication", "StockDataService", _config)
                    .WithHttpInteractions();

            pact
                .UponReceiving("Valid stock data request with indicators")
                .WithRequest(HttpMethod.Get, "/api/stock/AAPL")
                .WithQuery("exchange", "US")
                .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithJsonBody(new
                {
                    symbol = "AAPL",
                    currentData = new
                    {
                        close = 150.25
                    },
                    indicators = new
                    {
                        RSI_14 = 55.0,
                        SMA_50 = 145.0
                    }
                });

            await pact.VerifyAsync(async ctx =>
            {
                var client = new HttpClient
                {
                    BaseAddress = ctx.MockServerUri
                };

                var response = await client.GetAsync(
                    "/api/stock/AAPL?exchange=US"
                );

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            });
        }
    }
}
