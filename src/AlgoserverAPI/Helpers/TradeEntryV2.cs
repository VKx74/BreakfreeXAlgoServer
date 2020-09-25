using System;
using System.Linq;
using Algoserver.API.Models.Algo;

namespace Algoserver.API.Helpers
{

    public class TradeEntryV2Result
    {
        public decimal entry { get; internal set; }
        public decimal stop { get; internal set; }
        public decimal limit { get; internal set; }
        public bool is_buy { get; internal set; }

        public static bool IsEquals(TradeEntryV2Result obj1, TradeEntryV2Result obj2)
        {
            if (obj1.entry != obj2.entry) return false;
            if (obj1.stop != obj2.stop) return false;
            if (obj1.limit != obj2.limit) return false;
            if (obj1.is_buy != obj2.is_buy) return false;

            return true;
        }

    }

    public class TradeEntryV2CalculationData
    {
        public InputDataContainer container { get; set; }
        public SupportAndResistanceResult sar { get; set; }
        public Levels levels { get; set; }
        public bool randomize { get; set; } = true;
        public Trend trend { get; set; }
        public decimal riskRewords { get; set; }
    }

    public static class TradeEntryV2
    {
        public static TradeEntryV2Result CalculateSREntry(TradeEntryV2CalculationData calculationData, decimal stoploss_rr = 0m)
        {
            var container = calculationData.container;
            var randomize = calculationData.randomize;
            var top_ext2 = calculationData.sar.Plus28;
            var top_ext1 = calculationData.sar.Plus18;
            var resistance = calculationData.levels.Level128.EightEight;
            var natural = calculationData.levels.Level128.FourEight;
            var support = calculationData.levels.Level128.ZeroEight;
            var bottom_ext1 = calculationData.sar.Minus18;
            var bottom_ext2 = calculationData.sar.Minus28;

            var trend = calculationData.trend;

            if (trend == Trend.Up) {
                return new TradeEntryV2Result {
                    entry = support,
                    stop = bottom_ext2 - (stoploss_rr / 100 * (Math.Abs(bottom_ext1 - bottom_ext2))),
                    limit = support + (Math.Abs(support - bottom_ext2) * calculationData.riskRewords),
                    is_buy = true
                };
            } 
            
            if (trend == Trend.Down) {
                return new TradeEntryV2Result {
                    entry = resistance,
                    stop = top_ext2 + (stoploss_rr / 100 * Math.Abs(top_ext2 - top_ext1)),
                    limit = resistance - (Math.Abs(top_ext2 - resistance) * calculationData.riskRewords),
                    is_buy = false
                };
            }

            return null;

        } 
        
        public static TradeEntryV2Result CalculateEx1Entry(TradeEntryV2CalculationData calculationData, decimal stoploss_rr = 0m)
        {
            var container = calculationData.container;
            var randomize = calculationData.randomize;
            var top_ext2 = calculationData.sar.Plus28;
            var top_ext1 = calculationData.sar.Plus18;
            var resistance = calculationData.levels.Level128.EightEight;
            var natural = calculationData.levels.Level128.FourEight;
            var support = calculationData.levels.Level128.ZeroEight;
            var bottom_ext1 = calculationData.sar.Minus18;
            var bottom_ext2 = calculationData.sar.Minus28;

            var trend = calculationData.trend;

            if (trend == Trend.Up) {
                return new TradeEntryV2Result {
                    entry = bottom_ext1,
                    stop = bottom_ext2 - (stoploss_rr / 100 * (Math.Abs(bottom_ext1 - bottom_ext2))),
                    limit = bottom_ext1 + (Math.Abs(bottom_ext1 - bottom_ext2) * calculationData.riskRewords),
                    is_buy = true
                };
            } 
            
            if (trend == Trend.Down) {
                return new TradeEntryV2Result {
                    entry = top_ext1,
                    stop = top_ext2 + (stoploss_rr / 100 * Math.Abs(top_ext2 - top_ext1)),
                    limit = top_ext1 - (Math.Abs(top_ext2 - top_ext1) * calculationData.riskRewords),
                    is_buy = false
                };
            }

            return null;

        }

        private static decimal GetRandomizedShift(decimal price)
        {
            Random random = new Random();
            // from 0 to 10
            int randomShift = random.Next(0, 10);
            // positive or negative shift
            int randomShiftDirection = random.Next(0, 10) >= 5 ? -1 : 1;
            // 0.001% of price
            var minShift = price / 100000;
            // from -0.01% to 0.01% shift from price
            return minShift * randomShift * randomShiftDirection;
        }

    }

}
