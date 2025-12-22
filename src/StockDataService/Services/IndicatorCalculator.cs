using StockDataService.Models;

namespace StockDataService.Services
{
    public class IndicatorCalculator : IIndicatorCalculator
    {
        public StockIndicators CalculateIndicators(List<StockData> historicalData, StockData currentData)
        {
            if (historicalData == null || historicalData.Count == 0)
            {
                throw new ArgumentException("Historical data cannot be null or empty");
            }

            // Sort by date ascending
            var sortedData = historicalData.OrderBy(d => d.Date).ToList();
            var closePrices = sortedData.Select(d => d.Close).ToList();
            var volumes = sortedData.Select(d => d.Volume).ToList();

            var indicators = new StockIndicators
            {
                CurrentPrice = currentData.Close,
                DayChangePercent = CalculateDayChangePercent(sortedData, currentData)
            };

            // Calculate SMAs
            if (closePrices.Count >= 20)
                indicators.SMA_20 = CalculateSMA(closePrices, 20);
            if (closePrices.Count >= 50)
                indicators.SMA_50 = CalculateSMA(closePrices, 50);
            if (closePrices.Count >= 200)
                indicators.SMA_200 = CalculateSMA(closePrices, 200);

            // Calculate EMAs
            if (closePrices.Count >= 12)
                indicators.EMA_12 = CalculateEMA(closePrices, 12);
            if (closePrices.Count >= 26)
                indicators.EMA_26 = CalculateEMA(closePrices, 26);

            // Calculate RSI
            if (closePrices.Count >= 14)
            {
                indicators.RSI_14 = CalculateRSI(closePrices, 14);
                indicators.RSITrend = CalculateRSITrend(closePrices);
            }

            // Calculate MACD
            if (closePrices.Count >= 26)
            {
                var macd = CalculateMACD(closePrices);
                indicators.MACD_Line = macd.Line;
                indicators.MACD_Signal = macd.Signal;
                indicators.MACD_Histogram = macd.Histogram;
                indicators.MACDState = CalculateMACDState(indicators.MACD_Line, indicators.MACD_Signal, indicators.MACD_Histogram);
            }

            // Calculate Bollinger Bands
            if (closePrices.Count >= 20)
            {
                var bollinger = CalculateBollingerBands(closePrices, 20, 2);
                indicators.BollingerUpper = bollinger.Upper;
                indicators.BollingerMiddle = bollinger.Middle;
                indicators.BollingerLower = bollinger.Lower;
            }

            // Volume Analysis
            if (volumes.Count >= 20)
            {
                indicators.AverageVolume_20 = (long)volumes.TakeLast(20).Average();
                if (indicators.AverageVolume_20 > 0)
                {
                    indicators.VolumeChangePercent = ((decimal)currentData.Volume - indicators.AverageVolume_20) / indicators.AverageVolume_20 * 100;
                }
            }

            // 52-week high/low
            if (sortedData.Count >= 252) // Approximately 1 year of trading days
            {
                var last252Days = sortedData.TakeLast(252).ToList();
                indicators.Week52High = last252Days.Max(d => d.High);
                indicators.Week52Low = last252Days.Min(d => d.Low);
            }

            // Price vs SMA positions
            if (indicators.SMA_50 > 0)
                indicators.PriceVsSMA50 = currentData.Close > indicators.SMA_50 ? "Above" : "Below";
            if (indicators.SMA_200 > 0)
                indicators.PriceVsSMA200 = currentData.Close > indicators.SMA_200 ? "Above" : "Below";

            // OBV / VBO
            if (historicalData.Count >= 2)
            {
                indicators.OBV = CalculateOBV(historicalData);
                indicators.OBVTrend = CalculateOBVTrend(indicators.OBV);
            }

            return indicators;
        }

        private decimal CalculateSMA(List<decimal> prices, int period)
        {
            if (prices.Count < period) return 0;
            return prices.TakeLast(period).Average();
        }

        private decimal CalculateEMA(List<decimal> prices, int period)
        {
            if (prices.Count < period) return 0;

            decimal multiplier = 2m / (period + 1);
            decimal ema = prices.Take(period).Average(); // Start with SMA

            foreach (var price in prices.Skip(period))
            {
                ema = (price - ema) * multiplier + ema;
            }

            return ema;
        }

        private decimal CalculateRSI(List<decimal> prices, int period)
        {
            if (prices.Count < period + 1) return 0;

            var changes = new List<decimal>();
            for (int i = 1; i < prices.Count; i++)
            {
                changes.Add(prices[i] - prices[i - 1]);
            }

            var recentChanges = changes.TakeLast(period).ToList();
            var gains = recentChanges.Where(c => c > 0).DefaultIfEmpty(0).Average();
            var losses = Math.Abs(recentChanges.Where(c => c < 0).DefaultIfEmpty(0).Average());

            if (losses == 0) return 100;

            decimal rs = gains / losses;
            decimal rsi = 100 - (100 / (1 + rs));

            return rsi;
        }

        private (decimal Line, decimal Signal, decimal Histogram) CalculateMACD(List<decimal> prices)
        {
            var ema12Series = CalculateEMASeries(prices, 12);
            var ema26Series = CalculateEMASeries(prices, 26);

            var macdSeries = new List<decimal>();

            for (int i = 0; i < prices.Count; i++)
            {
                if (ema12Series[i] != null && ema26Series[i] != null)
                    macdSeries.Add(ema12Series[i]!.Value - ema26Series[i]!.Value);
            }

            if (macdSeries.Count < 9)
                return (0, 0, 0);

            var signalSeries = CalculateEMASeries(macdSeries, 9);

            decimal macdLine = macdSeries.Last();
            decimal signalLine = signalSeries.Last() ?? 0;
            decimal histogram = macdLine - signalLine;

            return (macdLine, signalLine, histogram);
        }

        private List<decimal?> CalculateEMASeries(List<decimal> prices, int period)
        {
            var result = new List<decimal?>();
            decimal multiplier = 2m / (period + 1);

            for (int i = 0; i < prices.Count; i++)
            {
                if (i < period - 1)
                {
                    result.Add(null);
                }
                else if (i == period - 1)
                {
                    result.Add(prices.Take(period).Average());
                }
                else
                {
                    var prevEma = result[i - 1]!.Value;
                    var ema = (prices[i] - prevEma) * multiplier + prevEma;
                    result.Add(ema);
                }
            }

            return result;
        }

        private (decimal Upper, decimal Middle, decimal Lower) CalculateBollingerBands(List<decimal> prices, int period, decimal standardDeviations)
        {
            if (prices.Count < period) return (0, 0, 0);

            decimal sma = CalculateSMA(prices, period);
            var recentPrices = prices.TakeLast(period).ToList();

            // Calculate standard deviation
            decimal variance = recentPrices.Sum(p => (p - sma) * (p - sma)) / period;
            decimal stdDev = (decimal)Math.Sqrt((double)variance);

            decimal upper = sma + (standardDeviations * stdDev);
            decimal lower = sma - (standardDeviations * stdDev);

            return (upper, sma, lower);
        }

        private decimal CalculateDayChangePercent(List<StockData> sortedData, StockData currentData)
        {
            if (sortedData.Count < 2) return 0;

            var previousDay = sortedData[sortedData.Count - 2];
            if (previousDay.Close == 0) return 0;
            return ((currentData.Close - previousDay.Close) / previousDay.Close) * 100;
        }

        private List<decimal> CalculateOBV(List<StockData> historicalData)
        {
            var data = historicalData
                .OrderBy(d => d.Date)
                .ToList();

            var obv = new List<decimal>();
            decimal currentObv = 0;

            obv.Add(0); // first day baseline

            for (int i = 1; i < data.Count; i++)
            {
                if (data[i].Close > data[i - 1].Close)
                    currentObv += data[i].Volume;
                else if (data[i].Close < data[i - 1].Close)
                    currentObv -= data[i].Volume;

                obv.Add(currentObv);
            }

            return obv;
        }

        private string CalculateOBVTrend(List<decimal> obv)
        {
            if (obv.Count < 10)
                return "Neutral";

            var recent = obv.TakeLast(10).ToList();

            if (recent.Last() > recent.First())
                return "Bullish";
            if (recent.Last() < recent.First())
                return "Bearish";

            return "Neutral";
        }

        private string CalculateRSITrend(List<decimal> prices)
        {
            if (prices.Count < 15)
                return "Neutral";

            var rsiValues = new List<decimal>();

            for (int i = 14; i < prices.Count; i++)
            {
                rsiValues.Add(CalculateRSI(prices.Take(i + 1).ToList(), 14));
            }

            var latest = rsiValues.Last();
            var previous = rsiValues[^2];

            if (latest > 70)
                return "Overbought";
            if (latest < 30)
                return "Oversold";
            if (latest > previous)
                return "Bullish";
            if (latest < previous)
                return "Bearish";

            return "Neutral";
        }

        private string CalculateMACDState(decimal macd, decimal signal, decimal histogram)
        {
            if (macd > signal && histogram > 0)
                return "Bullish";
            if (macd < signal && histogram < 0)
                return "Bearish";

            return "Neutral";
        }

    }
}
