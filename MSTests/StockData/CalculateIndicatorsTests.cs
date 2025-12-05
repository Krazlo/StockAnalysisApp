using Microsoft.CodeAnalysis;
using StockDataService.Models;
using StockDataService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MSTests.StockData
{
    [TestClass]
    public class CalculateIndicatorsTests
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var content = await File.ReadAllTextAsync("StockData\\json\\eodhd-nkt-sample-response.json");
            List<EodhdResponse>? eodhdResponse = JsonSerializer.Deserialize<List<EodhdResponse>>(content, options);

            var historicalData = new List<StockDataService.Models.StockData>();
            foreach (var item in eodhdResponse)
            {
                historicalData.Add(new StockDataService.Models.StockData
                {
                    Symbol = "NKT",
                    Date = item.Date.GetValueOrDefault(),
                    Open = item.Open.GetValueOrDefault(),
                    High = item.High.GetValueOrDefault(),
                    Low = item.Low.GetValueOrDefault(),
                    Close = item.Close.GetValueOrDefault(),
                    Volume = item.Volume.GetValueOrDefault()
                });
            }

            historicalData = historicalData.OrderByDescending(d => d.Date).ToList();
            var currentData = historicalData.First();

            // Calculate indicators
            IndicatorCalculator calculator = new IndicatorCalculator();
            StockIndicators indicators = calculator.CalculateIndicators(historicalData, currentData);

            var a = 2;
        }
    }
}
