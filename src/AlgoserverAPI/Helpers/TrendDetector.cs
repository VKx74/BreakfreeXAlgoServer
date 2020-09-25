using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.Algo;

namespace Algoserver.API.Helpers
{
    public static class TrendDetector {      
        public static Trend CalculateByHma(List<decimal> data, int period = 200)
        {
            var hmaData = TechCalculations.Hma(data, period);
            var last = hmaData.LastOrDefault();

            return data.LastOrDefault() >  last ? Trend.Up : Trend.Down;
        }
        
        public static Trend CalculateByMesa(List<decimal> data, decimal diff = 0.1m, decimal global_fast = 0.25m, decimal global_slow = 0.05m, decimal local_fast = 1.2m, decimal local_slow = 0.6m)
        {
            var mesa_global = TechCalculations.MESA(data, (double)global_fast, (double)global_slow);
            var mesa_local = TechCalculations.MESA(data, (double)local_fast, (double)local_slow);

            var mesa_global_value = mesa_global.LastOrDefault();
            var mesa_local_value = mesa_local.LastOrDefault();

            if (mesa_global_value.Fast == Decimal.Zero || mesa_global_value.Slow == Decimal.Zero) {
                return Trend.Undefined;
            }
            
            if (mesa_local_value.Fast == Decimal.Zero || mesa_local_value.Slow == Decimal.Zero) {
                return Trend.Undefined;
            }
            
            var lastPrice = data.LastOrDefault();
            var currentDiff = Math.Abs(mesa_global_value.Fast - mesa_global_value.Slow) / lastPrice * 100;
            if (currentDiff < diff) {
                return Trend.Undefined;
            }

            if (mesa_global_value.Fast > mesa_global_value.Slow) {
                if (mesa_global_value.Slow > mesa_local_value.Slow || mesa_global_value.Slow > mesa_local_value.Fast) {
                    return Trend.Undefined;
                }
                return Trend.Up;    
            } else if (mesa_global_value.Fast < mesa_global_value.Slow) {
                if (mesa_global_value.Slow < mesa_local_value.Slow || mesa_global_value.Slow < mesa_local_value.Fast) {
                    return Trend.Undefined;
                }
                return Trend.Down;
            }

            return Trend.Undefined;
        }
    }

}
