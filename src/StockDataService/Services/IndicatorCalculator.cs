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
                var bollinger = CalculateBollingerBands(closePrices, 20, 2, indicators.CurrentPrice); //2 standard deviations cover approximately 95% of all data points
                indicators.BollingerUpper = bollinger.Upper;
                indicators.BollingerMiddle = bollinger.Middle;
                indicators.BollingerLower = bollinger.Lower;
                indicators.BollingerPercentB = bollinger.PercentB;
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

        public decimal CalculateSMA(List<decimal> prices, int period)
        {
            if (prices.Count < period) return 0;
            return prices.TakeLast(period).Average();
        }

        public decimal CalculateEMA(List<decimal> prices, int period)
        {
            if (prices.Count < period) return 0;

            decimal multiplier = 2m / (period + 1);

            decimal ema = 0;
            for (int i = 0; i < period; i++) ema += prices[i];
            ema /= period;

            for (int i = period; i < prices.Count; i++)
            {
                ema = (prices[i] - ema) * multiplier + ema;
            }

            return ema;
        }

        public decimal CalculateRSI(List<decimal> prices, int period)
        {
            //50 is the midpoint of the RSI scale
            if (prices.Count <= period) return 50;

            decimal maU = 0;
            decimal maD = 0;

            for (int i = 1; i <= period; i++)
            {
                decimal change = prices[i] - prices[i - 1];
                maU += change > 0 ? change : 0;
                maD += change < 0 ? Math.Abs(change) : 0;
            }

            maU /= period;
            maD /= period;

            for (int i = period + 1; i < prices.Count; i++)
            {
                decimal change = prices[i] - prices[i - 1];
                decimal u = change > 0 ? change : 0;
                decimal d = change < 0 ? Math.Abs(change) : 0;

                maU = (u + maU * (period - 1)) / period;
                maD = (d + maD * (period - 1)) / period;
            }

            if (maD == 0) return 100;

            decimal rs = maU / maD;
            decimal rsi = 100 - (100 / (1 + rs));
            return rsi;
        }

        public (decimal Line, decimal Signal, decimal Histogram) CalculateMACD(List<decimal> prices)
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

        public List<decimal?> CalculateEMASeries(List<decimal> prices, int period)
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
                    var ema = prevEma + multiplier * (prices[i] - prevEma);
                    result.Add(ema);
                }
            }

            return result;
        }

        public (decimal Upper, decimal Middle, decimal Lower, decimal PercentB) CalculateBollingerBands(List<decimal> prices, int period, decimal deviation, decimal currentPrice)
        {
            if (prices.Count < period) return (0, 0, 0, 0);

            decimal sma = CalculateSMA(prices, period);
            var recentPrices = prices.TakeLast(period).ToList();

            // Calculate standard deviation
            decimal variance = recentPrices.Sum(p => (p - sma) * (p - sma)) / period;
            decimal sigma = (decimal)Math.Sqrt((double)variance);

            decimal upper = sma + (sigma * deviation);
            decimal lower = sma - (sigma * deviation);

            // %B
            decimal bandwidth = upper - lower;

            decimal percentB = bandwidth == 0 ? 0.5m : (currentPrice - lower) / bandwidth;
            return (upper, sma, lower, percentB);
        }

        public decimal CalculateDayChangePercent(List<StockData> sortedData, StockData currentData)
        {
            if (sortedData.Count < 2) return 0;

            var previousDay = sortedData[sortedData.Count - 2];
            if (previousDay.Close == 0) return 0;
            return ((currentData.Close - previousDay.Close) / previousDay.Close) * 100;
        }

        public List<decimal> CalculateOBV(List<StockData> historicalData)
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

        public string CalculateOBVTrend(List<decimal> obv)
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

        public string CalculateRSITrend(List<decimal> prices)
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

        public string CalculateMACDState(decimal macd, decimal signal, decimal histogram)
        {
            if (macd > signal && histogram > 0)
                return "Bullish";
            if (macd < signal && histogram < 0)
                return "Bearish";

            return "Neutral";
        }

    }
}
