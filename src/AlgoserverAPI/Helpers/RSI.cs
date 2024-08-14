using System;
using System.Linq;

namespace Algoserver.API.Helpers
{
    class RSIIndicator
    {
        public static decimal CalculateLastRSI(decimal[] input, int period)
        {
            // make latest data in the start
            var data = input.Reverse().ToArray();
            decimal sumGain = 0, sumLoss = 0;
            for (var i = 1; i <= period; i++)
            {
                var diff = data[i] - data[i + 1];
                if (diff > 0)
                {
                    sumGain += diff;
                }
                else
                {
                    sumLoss -= diff;
                }
            }

            var avgGain = sumGain / period;
            var avgLoss = sumLoss / period;

            if (avgLoss == 0)
            {
                return 100; // Edge case, return max RSI value
            }

            var rs = avgGain / avgLoss;
            var rsi = 100 - (100 / (1 + rs));

            return rsi;
        }
    }
}