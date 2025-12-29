using Microsoft.CodeAnalysis.CSharp.Syntax;
using StockDataService.Models;
using StockDataService.Services;
using System.Text.Json;

namespace MSTests.StockDataTests
{
    [TestClass]
    public class CalculateIndicatorsTests
    {
        private IndicatorCalculator _calculator;

        [TestInitialize]
        public void Setup()
        {
            _calculator = new IndicatorCalculator();
        }

        [TestMethod]
        public async Task TestCalculationDoesSomething()
        {
            List<StockData> historicalData = await GetHistoricalData();
            var currentData = historicalData.First();

            // Calculate indicators
            IndicatorCalculator calculator = new IndicatorCalculator();
            StockIndicators indicators = calculator.CalculateIndicators(historicalData, currentData);

            //Test is successful if it goes all the way through without any errors.
        }

        #region Validation

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CalculateIndicators_Throws_WhenHistoricalDataIsEmpty()
        {
            _calculator.CalculateIndicators(
                new List<StockData>(),
                new StockData());
        }

        #endregion

        #region SMA / EMA

        [TestMethod]
        public void CalculateIndicators_Computes_SMA20_WhenEnoughData()
        {
            var history = GenerateHistoricalData(30);
            var current = history.Last();

            var result = _calculator.CalculateIndicators(history, current);

            Assert.IsTrue(result.SMA_20 > 0);
            Assert.IsTrue(result.SMA_20 < result.CurrentPrice);
        }

        [TestMethod]
        public void CalculateIndicators_Computes_EMA12_WhenEnoughData()
        {
            var history = GenerateHistoricalData(20);
            var current = history.Last();

            var result = _calculator.CalculateIndicators(history, current);

            Assert.IsTrue(result.EMA_12 > 0);
        }

        #endregion

        #region RSI

        [TestMethod]
        public async Task CalculateIndicators_RSI_InRange_0_To_100()
        {
            var history = await GetHistoricalData();
            var current = history.Last();

            var result = _calculator.CalculateIndicators(history, current);

            Assert.IsTrue(result.RSI_14 >= 0);
            Assert.IsTrue(result.RSI_14 <= 100);
        }

        [TestMethod]
        public void CalculateIndicators_RSITrend_Bullish_WhenPricesRising()
        {
            List<decimal>? priceList = new List<decimal>
            {
                100, 102, 101, 103, 102, 104, 103, 105, 104, 106, 
                105, 107, 106, 108, 107, 106, 108, 109, 110, 109,
                108, 109, 110, 111, 113, 115, 117, 118, 117, 120
            };
            var history = GenerateHistoricalData(priceList.Count, prices: priceList);
            var current = history.Last();

            var result = _calculator.CalculateIndicators(history, current);

            Assert.AreEqual("Bullish", result.RSITrend);
        }

        [TestMethod]
        public void CalculateIndicators_RSI_Equals100_WhenNoLosses()
        {
            var history = GenerateHistoricalData(30, dailyChange: 2);
            var current = history.Last();

            var result = _calculator.CalculateIndicators(history, current);

            Assert.AreEqual("Overbought", result.RSITrend);
            Assert.AreEqual(100, result.RSI_14);
        }

        #endregion

        #region MACD

        [TestMethod]
        public void CalculateIndicators_Computes_MACD_WhenEnoughData()
        {
            var history = GenerateHistoricalData(50);
            var current = history.Last();

            var result = _calculator.CalculateIndicators(history, current);

            Assert.AreNotEqual(0, result.MACD_Line);
            Assert.AreNotEqual(0, result.MACD_Signal);
        }

        [TestMethod]
        public void CalculateIndicators_MACDState_Bullish_ForUptrend()
        {
            List<decimal>? priceList = new List<decimal>
            {
                100, 102, 101, 103, 102, 104, 103, 105, 104, 106,
                105, 107, 106, 108, 107, 106, 108, 109, 110, 109,
                108, 109, 110, 111, 113, 115, 117, 118, 117, 120,
                119, 121, 123, 124, 126, 125, 128, 129, 130, 129,
                129, 131, 133, 134, 136, 135, 138, 139, 140, 139,
                139, 141, 143, 144, 146, 145, 148, 149, 150, 149,
            };
            var history = GenerateHistoricalData(priceList.Count, prices: priceList);
            var current = history.Last();

            var result = _calculator.CalculateIndicators(history, current);

            Assert.AreEqual("Bullish", result.MACDState);
        }

        #endregion

        #region Bollinger Bands

        [TestMethod]
        public void CalculateIndicators_Computes_BollingerBands()
        {
            var history = GenerateHistoricalData(25);
            var current = history.Last();

            var result = _calculator.CalculateIndicators(history, current);

            Assert.IsTrue(result.BollingerUpper > result.BollingerMiddle);
            Assert.IsTrue(result.BollingerLower < result.BollingerMiddle);
        }

        #endregion

        #region OBV

        [TestMethod]
        public void CalculateIndicators_OBVTrend_Bullish_WhenVolumeConfirmsUptrend()
        {
            var history = GenerateHistoricalData(30, dailyChange: 1);
            var current = history.Last();

            var result = _calculator.CalculateIndicators(history, current);

            Assert.AreEqual("Bullish", result.OBVTrend);
        }

        #endregion

        #region Helpers

        private static List<StockData> GenerateHistoricalData(
            int days,
            decimal startPrice = 100,
            decimal dailyChange = 1,
            long volume = 1_000_000,
            List<decimal>? prices = null)
        {
            var data = new List<StockData>();

            if (prices == null)
            {
                var price = startPrice;

                for (int i = 0; i < days; i++)
                {
                    data.Add(new StockData
                    {
                        Date = DateTime.Today.AddDays(-days + i),
                        Open = price,
                        High = price + 1,
                        Low = price - 1,
                        Close = price,
                        Volume = volume
                    });

                    price += dailyChange;
                }
            }
            else
            {
                for (int i = 0; i < days; i++)
                {
                    data.Add(new StockData
                    {
                        Date = DateTime.Today.AddDays(-days + i),
                        Open = prices[i],
                        High = prices[i] + 1,
                        Low = prices[i] - 1,
                        Close = prices[i],
                        Volume = volume
                    });
                }
            }


            return data;
        }

        private static async Task<List<StockData>> GetHistoricalData()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "StockDataTests",
                "json",
                "eodhd-nkt-sample-response.json"
            );

            var content = await File.ReadAllTextAsync(filePath);
            List<EodhdResponse>? eodhdResponse = JsonSerializer.Deserialize<List<EodhdResponse>>(content, options);

            var historicalData = new List<StockData>();
            foreach (var item in eodhdResponse)
            {
                historicalData.Add(new StockData
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

            historicalData.OrderByDescending(d => d.Date).ToList();

            return historicalData;
        }

        #endregion
    }
}
