using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockDataService.Data;
using System.Net.Http.Json;
using System.Text.Json;


namespace MSTests.Integration
{
    [TestClass]
    public class WireMockTests
    {
        private static IContainer _wireMock;
        private static WebApplicationFactory<StockDataService.Program> _stockFactory;
        private static WebApplicationFactory<AIAnalysisService.Program> _analysisFactory;

        [ClassInitialize]
        public static async Task Setup(TestContext _)
        {
            _wireMock = new ContainerBuilder()
                .WithImage("wiremock/wiremock:latest")
                .WithPortBinding(8080, true)
                .WithWaitStrategy(
                    Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(8080))
                .Build();

            await _wireMock.StartAsync();

            var wireMockUrl =
                $"http://localhost:{_wireMock.GetMappedPublicPort(8080)}";

            _stockFactory = new WebApplicationFactory<StockDataService.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((_, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["EODHD:BaseUrl"] = wireMockUrl,
                            ["EODHD:ApiKey"] = "fake-key"
                        });
                    });
                    builder.ConfigureServices(services =>
                    {
                        // Remove existing DbContext registration
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<StockDataDbContext>)
                        );

                        if (descriptor != null)
                            services.Remove(descriptor);

                        // Add In-Memory DB
                        services.AddDbContext<StockDataDbContext>(options =>
                        {
                            options.UseInMemoryDatabase("StockDataTestDb");
                        });

                        var sp = services.BuildServiceProvider();
                        using var scope = sp.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<StockDataDbContext>();
                        db.Database.EnsureCreated();
                    });
                });

            _analysisFactory = new WebApplicationFactory<AIAnalysisService.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((_, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Gemini:BaseUrl"] = wireMockUrl,
                            ["Gemini:ApiKey"] = "fake-key",
                            ["Gemini:Model"] = "gemini-2.5-flash"
                        });
                    });
                });
        }

        [TestMethod]
        public async Task GetStockData_Uses_Eodhd_Response()
        {
            //Arrange
            var client = new HttpClient
            {
                BaseAddress = new Uri($"http://localhost:{_wireMock.GetMappedPublicPort(8080)}")
            };

            await client.PostAsync("/__admin/mappings", JsonContent.Create(new
            {
                request = new
                {
                    method = "GET",
                    urlPathPattern = "/eod/.*"
                },
                response = new
                {
                    status = 200,
                    headers = new { ContentType = "application/json" },
                    jsonBody = new[]
                    {
                        new {
                            date = "2024-01-01",
                            open = 100,
                            high = 102,
                            low = 99,
                            close = 101,
                            volume = 1_000_000
                        },
                        new {
                            date = "2024-01-02",
                            open = 101,
                            high = 103,
                            low = 100,
                            close = 102,
                            volume = 1_100_000
                        }
                    }
                }
            }));

            var apiClient = _stockFactory.CreateClient();

            //Act
            var response = await apiClient.GetAsync(
                "/api/stock/AAPL?exchange=US");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            //Assert
            Assert.AreEqual("AAPL", doc.RootElement.GetProperty("symbol").GetString());
            Assert.IsTrue(doc.RootElement.GetProperty("indicators")
                .TryGetProperty("rsI_14", out _));
        }

        [TestMethod]
        public async Task AnalyzeStock_Uses_Gemini_Response()
        {
            //Arrange
            var wireMockClient = new HttpClient
            {
                BaseAddress = new Uri(
                    $"http://localhost:{_wireMock.GetMappedPublicPort(8080)}")
            };

            await wireMockClient.PostAsync("/__admin/mappings", JsonContent.Create(new
            {
                request = new
                {
                    method = "POST",
                    urlPathPattern = "/gemini-2.5-flash:generateContent"
                },
                response = new
                {
                    status = 200,
                    headers = new { ContentType = "application/json" },
                    jsonBody = new
                    {
                        candidates = new[]
                        {
                    new
                    {
                        content = new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = "Mega god analyse"
                                }
                            }
                        }
                    }
                }
                    }
                }
            }));

            var apiClient = _analysisFactory.CreateClient();

            var request = new
            {
                Prompt = "Analyze this stock",
                Symbol = "AAPL",
                StockData = new
                {
                    Symbol = "AAPL",
                    CurrentPrice = 150,
                    Indicators = TestHelpers.GenerateDummyIndicators()
                }
            };

            // Act
            var response = await apiClient.PostAsJsonAsync(
                "/api/analysis/analyze",
                request);

            // Assert
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            Assert.IsTrue(doc.RootElement.GetProperty("success").GetBoolean());
            Assert.AreEqual(
                "Mega god analyse",
                doc.RootElement.GetProperty("analysis").GetString());
            Assert.AreEqual(
                "AAPL",
                doc.RootElement.GetProperty("symbol").GetString());
        }

        [ClassCleanup]
        public static async Task Cleanup()
        {
            _stockFactory.Dispose();
            await _wireMock.DisposeAsync();
        }
    }
}
