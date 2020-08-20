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
        
        public static Trend CalculateByMesa(List<decimal> data, decimal diff = 0.00001m, decimal fast = 0.5m, decimal slow = 0.05m)
        {
            var mesaData = TechCalculations.MESA(data, (double)fast, (double)slow);
            var last = mesaData.LastOrDefault();
            var currentDiff = Math.Abs(last.Fast - last.Slow);

            if (last.Fast > last.Slow && currentDiff >= diff) {
                return Trend.Up;
            }

            if (last.Fast < last.Slow && currentDiff >= diff) {
                return Trend.Down;
            }

            return Trend.Undefined;
        }
    }

}
