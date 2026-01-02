using AIAnalysisService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSTests
{
    public static class TestHelpers
    {
        public static StockIndicatorsInfo GenerateDummyIndicators()
        {
            StockIndicatorsInfo indicators = new StockIndicatorsInfo()
            {
                SMA_20 = 55,
                SMA_50 = 60,
                SMA_200 = 65,
                EMA_12 = 1,
                EMA_26 = 1,
                RSI_14 = 1,
                MACD_Line = 1,
                MACD_Signal = 1,
                MACD_Histogram = 1,
                BollingerUpper = 1,
                BollingerMiddle = 1,
                BollingerLower = 1,
                AverageVolume_20 = 1,
                VolumeChangePercent = 1,
                CurrentPrice = 1,
                DayChangePercent = 1,
                Week52High = 1,
                Week52Low = 1,
                PriceVsSMA50 = string.Empty,
                PriceVsSMA200 = string.Empty,
                OBV = new List<decimal>(),
                OBVTrend = string.Empty,
                RSITrend = string.Empty,
                MACDState = string.Empty
            };

            return indicators;
        }
    }
}
