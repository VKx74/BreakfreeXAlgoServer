using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.Algo;

namespace Algoserver.API.Helpers
{
    public class ExtendedTrendResult {
        public Trend GlobalTrend { get; set; }
        public Trend LocalTrend { get; set; }
        public decimal LocalTrendSpread { get; set; }
        public decimal GlobalTrendSpread { get; set; }
    }
    public class TrendsStrengthResult {
        public decimal LocalTrendSpread { get; set; }
        public decimal GlobalTrendSpread { get; set; }
    }

    public static class TrendDetector
    {
        public static Trend CalculateByHma(List<decimal> data, int period = 200)
        {
            var hmaData = TechCalculations.Hma(data, period);
            var last = hmaData.LastOrDefault();

            return data.LastOrDefault() > last ? Trend.Up : Trend.Down;
        }

        public static ExtendedTrendResult CalculateByMesaBy2TrendAdjusted(List<decimal> data, decimal global_fast = 0.25m, decimal global_slow = 0.05m, decimal local_fast = 1.2m, decimal local_slow = 0.6m)
        {
            var mesa_global = TechCalculations.MESA(data, (double)global_fast, (double)global_slow);
            var mesa_local = TechCalculations.MESA(data, (double)local_fast, (double)local_slow);

            var mesa_global_value = mesa_global.LastOrDefault();
            var mesa_local_value = mesa_local.LastOrDefault();

            var trendsStrength = TrendDetector.MeasureTrendsStrength(mesa_global, mesa_local);

            return new ExtendedTrendResult {
                GlobalTrend = mesa_global_value.Fast > mesa_global_value.Slow ? Trend.Up : Trend.Down,
                LocalTrend = mesa_local_value.Fast > mesa_local_value.Slow ? Trend.Up : Trend.Down,
                LocalTrendSpread = trendsStrength.LocalTrendSpread,
                GlobalTrendSpread = trendsStrength.GlobalTrendSpread
            };
        }
        
        public static TrendsStrengthResult MeasureTrendsStrength(MESAData[] mesa_global, MESAData[] mesa_local) {
            var length = mesa_global.Length;
            var count = mesa_global.Length / 2;
            if (count > 100) {
                count = 100;
            }

            var global_spreads = new List<decimal>();
            var local_spreads = new List<decimal>();
            for (var i = 1; i < count; i++) {
                var global = mesa_global[length - i];
                var globalSpread = Math.Abs(global.Fast - global.Slow);
                var local = mesa_local[length - i];
                var localSpread = Math.Abs(local.Fast - local.Slow);

                global_spreads.Add(globalSpread);
                local_spreads.Add(localSpread);
            }
            var global_avg = global_spreads.Sum() / count;
            var local_avg = local_spreads.Sum() / count;
            var global_current = global_spreads.FirstOrDefault();
            var local_current = local_spreads.FirstOrDefault();

            var globalTrendDiff = global_current / global_avg;
            var localTrendDiff = local_current / local_avg;

            return new TrendsStrengthResult {
                LocalTrendSpread = localTrendDiff,
                GlobalTrendSpread = globalTrendDiff
            };
        }

        public static Trend CalculateByMesa(List<decimal> data, decimal diff = 0.1m, decimal global_fast = 0.25m, decimal global_slow = 0.05m, decimal local_fast = 1.2m, decimal local_slow = 0.6m)
        {
            var mesa_global = TechCalculations.MESA(data, (double)global_fast, (double)global_slow);
            var mesa_local = TechCalculations.MESA(data, (double)local_fast, (double)local_slow);

            var mesa_global_value = mesa_global.LastOrDefault();
            var mesa_local_value = mesa_local.LastOrDefault();

            if (mesa_global_value.Fast == Decimal.Zero || mesa_global_value.Slow == Decimal.Zero)
            {
                return Trend.Undefined;
            }

            if (mesa_local_value.Fast == Decimal.Zero || mesa_local_value.Slow == Decimal.Zero)
            {
                return Trend.Undefined;
            }

            var lastPrice = data.LastOrDefault();
            var currentDiff = Math.Abs(mesa_global_value.Fast - mesa_global_value.Slow) / lastPrice * 100;
            if (currentDiff < diff)
            {
                return Trend.Undefined;
            }

            if (mesa_global_value.Fast > mesa_global_value.Slow)
            {
                if (mesa_global_value.Slow > mesa_local_value.Slow || mesa_global_value.Slow > mesa_local_value.Fast)
                {
                    return Trend.Undefined;
                }
                return Trend.Up;
            }
            else if (mesa_global_value.Fast < mesa_global_value.Slow)
            {
                if (mesa_global_value.Slow < mesa_local_value.Slow || mesa_global_value.Slow < mesa_local_value.Fast)
                {
                    return Trend.Undefined;
                }
                return Trend.Down;
            }

            return Trend.Undefined;
        }

        public static Trend MergeTrends(ExtendedTrendResult trends)
        {
            if (trends.GlobalTrend == Trend.Up && trends.LocalTrend == Trend.Up)
            {
                return Trend.Up;
            }
            if (trends.GlobalTrend == Trend.Down && trends.LocalTrend == Trend.Down)
            {
                return Trend.Down;
            }
            return Trend.Undefined;
        }
    }

}
