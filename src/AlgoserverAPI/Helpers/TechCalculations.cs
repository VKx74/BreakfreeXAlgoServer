using System;
using System.Collections.Generic;
using System.Linq;

namespace Algoserver.API.Helpers
{

    public class LookBackResult
    {
        public decimal EightEight { get; set; }
        public decimal FourEight { get; set; }
        public decimal ZeroEight { get; set; }
        public decimal Increment { get; set; }
        public decimal AbsTop { get; set; }

        public static bool IsEquals(LookBackResult obj1, LookBackResult obj2) {
            if (obj1.EightEight != obj2.EightEight) {
                return false;
            }
            if (obj1.FourEight != obj2.FourEight) {
                return false;
            }
            if (obj1.ZeroEight != obj2.ZeroEight) {
                return false;
            }
            return true;
        }
    }
    
    public class Levels
    {
        public LookBackResult Level128 { get; set; }
        public LookBackResult Level32 { get; set; }
        public LookBackResult Level16 { get; set; }
        public LookBackResult Level8 { get; set; }
    }

    public static class TechCalculations
    {
        public static decimal LowestOnRange(IEnumerable<decimal> data, int lookback)
        {
            var lowest = decimal.MaxValue;
            var reversedArray = data.Reverse().ToArray();
            for (var i = 0; i <= lookback && i < reversedArray.Length; i++)
            {
                if (reversedArray[i] < lowest)
                {
                    lowest = reversedArray[i];
                }
            }
            return lowest;
        }

        public static decimal HighestOnRange(IEnumerable<decimal> data, int lookback)
        {
            var highest = decimal.MinValue;
            var reversedArray = data.Reverse().ToArray();
            for (var i = 0; i <= lookback && i < reversedArray.Length; i++)
            {
                if (reversedArray[i] > highest)
                {
                    highest = reversedArray[i];
                }
            }
            return highest;
        }

        public static decimal Sun(IEnumerable<decimal> data, int count)
        {
            var sum = decimal.Zero;
            var reversedArray = data.Reverse().ToArray();
            for (var i = 0; i < count && i < reversedArray.Length; i++)
            {
                sum += reversedArray[i] / count;
            }
            return sum;
        }

        public static decimal[] Sma(decimal[] data, int period)
        {
            var res = new List<decimal>();
            for (var i = 0; i < data.Length; i++)
            {
                var sum = data[i] + (i > 0 ? res[i - 1] : 0) - (i >= period ? data[i - period] : 0);
                res.Add(sum);
            }
            return res.ToArray();
        }

        public static List<decimal> Wma(List<decimal> data, int length)
        {
            var res = new List<decimal>();
            var priorSum = decimal.Zero;
            var priorWsum = decimal.Zero;
            var sum = decimal.Zero;
            var wsum = decimal.Zero;

            for (var i = 0; i < data.Count; i++)
            {
                var myPeriod = Math.Min(i + 1, length);
                wsum = priorWsum - (i >= length ? priorSum : 0) + myPeriod * data[i];
                sum = priorSum + data[i] - (i >= length ? data[i - length] : 0);
                var wma = wsum / (0.5m * myPeriod * (myPeriod + 1));
                res.Add(wma);
                priorWsum = wsum;
                priorSum = sum;
            }

            return res;
        }

        public static List<decimal> Hma(List<decimal> data, int length)
        {
            var res = new List<decimal>();
            var wma1 = TechCalculations.Wma(data, length / 2);
            var wma2 = TechCalculations.Wma(data, length);
            var diffDataSeries = new List<decimal>();
            var diffWmaPeriod = Math.Floor(Math.Sqrt(length));

            for (var i = 0; i < data.Count; i++)
            {
                var wma1Value = wma1[i];
                var wma2Value = wma2[i];
                var diffDataRows = 2 * wma1Value - wma2Value;
                diffDataSeries.Add(diffDataRows);
            }

            var wmaDiffDataRows = TechCalculations.Wma(diffDataSeries, Convert.ToInt32(diffWmaPeriod));
            return wmaDiffDataRows;
        }

        public static decimal[] Cmo(decimal[] data, int period)
        {
            var res = new List<decimal>();
            var up = new List<decimal>();
            var down = new List<decimal>();

            for (var i = 1; i < data.Length; i++)
            {
                var input0 = data[i];
                var input1 = data[i - 1];
                down.Add(Math.Max(input1 - input0, 0));
                up.Add(Math.Max(input0 - input1, 0));
            }

            var sumDown = TechCalculations.Sma(down.ToArray(), period);
            var sumUp = TechCalculations.Sma(up.ToArray(), period);

            for (var i = 1; i < data.Length; i++)
            {
                var cmo = decimal.Zero;
                var currentDown = sumDown[i];
                var currentUp = sumUp[i];

                if (currentUp == currentDown)
                {
                    cmo = 0;
                }
                else
                {
                    cmo = 100 * ((currentUp - currentDown) / (currentUp + currentDown));
                }

                res.Add(cmo);
            }

            return res.ToArray();
        }

        public static decimal[] SubtractArrays(decimal[] array1, decimal[] array2)
        {
            var res = new List<decimal>();
            var length = Math.Min(array1.Length, array2.Length);

            for (var i = 0; i < length; i++)
            {
                res.Add(array1[i] - array2[i]);
            }

            return res.ToArray();
        }

        public static decimal PercentRank(decimal[] data, int count)
        {
            var numberEqualOrBellow = 0;
            var reversedArray = data.Reverse().ToArray();
            for (var i = 1; i <= count && i < reversedArray.Length; i++)
            {
                if (reversedArray[i] <= reversedArray[0])
                {
                    numberEqualOrBellow++;
                }
            }
            return (numberEqualOrBellow / count) * 100;
        }

        public static decimal[] THL(decimal[] data, int len, decimal weightMultiplier)
        {
            var res = new List<decimal>();
            var weight = weightMultiplier / (len + 1);
            var output = data[0];

            for (var i = len; i >= 0; i--)
            {
                output = (data[0] - output) * weight + output;
                res.Add(output);
            }

            return res.ToArray();
        }

        public static LookBackResult LookBack(int lookback,IEnumerable<decimal> uPrice, IEnumerable<decimal> lPrice)
        {
            var result = new LookBackResult();
            var logTen = Math.Log(10);
            var log8 = Math.Log(8);
            var log2 = Math.Log(2);

            //-- find highest/lowest price over specified lookback
            var vLow = TechCalculations.LowestOnRange(lPrice, lookback);
            var vHigh = TechCalculations.HighestOnRange(uPrice, lookback);
            var vDist = vHigh - vLow;
            var tmpHigh = (double)vHigh;
            var tmpLow = (double)vLow;

            //-- calculate scale frame
            var sfVar = Math.Log(0.4 * tmpHigh) / logTen - Math.Floor(Math.Log(0.4 * tmpHigh) / logTen);
            var SR = tmpHigh > 25 ? (sfVar > 0 ? Math.Exp(logTen * (Math.Floor(Math.Log(0.4 * tmpHigh) / logTen) + 1)) : Math.Exp(logTen * (Math.Floor(Math.Log(0.4 * tmpHigh) / logTen)))) :
                100 * Math.Exp(log8 * (Math.Floor(Math.Log(0.005 * tmpHigh) / log8)));
            var nVar1 = (Math.Log(SR / (tmpHigh - tmpLow)) / log8);
            var nVar2 = nVar1 - Math.Floor(nVar1);
            var N = nVar1 <= 0 ? 0 : (nVar2 == 0 ? Math.Floor(nVar1) : Math.Floor(nVar1) + 1);

            //-- calculate scale interval and temporary frame top and bottom
            var SI = SR * Math.Exp(-(N) * log8);
            var M = Math.Floor(((1.0 / log2) * Math.Log((tmpHigh - tmpLow) / SI)) + 0.0000001);
            var I = Math.Round(((tmpHigh + tmpLow) * 0.5) / (SI * Math.Exp((M - 1) * log2)));

            var Bot = (I - 1) * SI * Math.Exp((M - 1) * log2);
            var Top = (I + 1) * SI * Math.Exp((M - 1) * log2);

            //-- determine if frame shift is required
            var doShift = ((tmpHigh - Top) > 0.25 * (Top - Bot)) || ((Bot - tmpLow) > 0.25 * (Top - Bot));

            var ER = doShift;

            var MM = !ER ? M : (ER && M < 2) ? M + 1 : 0;
            var NN = !ER ? N : (ER && M < 2) ? N : N - 1;

            //-- recalculate scale interval and top and bottom of frame, if necessary
            var finalSI = ER ? (SR * Math.Exp(-(NN) * log8)) : SI;
            var finalI = ER ? Math.Round(((tmpHigh + tmpLow) * 0.5) / (finalSI * Math.Exp((MM - 1) * log2))) : I;
            var finalBot = (decimal)(ER ? (finalI - 1) * finalSI * Math.Exp((MM - 1) * log2) : Bot);
            var finalTop = (decimal)(ER ? (finalI + 1) * finalSI * Math.Exp((MM - 1) * log2) : Top);

            //-- determine the increment
            var Increment = (finalTop - finalBot) / 8;
            var AbsTop = finalTop + (3 * Increment);    //-- determine the absolute top
            var EightEight = AbsTop - (3 * Increment);  //-- create our Murrey line variables based on absolute top and the increment
            var FourEight = AbsTop - (7 * Increment); 
            var ZeroEight = AbsTop - (11 * Increment);

            result.AbsTop = AbsTop;
            result.Increment = Increment;
            result.EightEight = EightEight;
            result.FourEight = FourEight;
            result.ZeroEight = ZeroEight;

            return result;
        }

        public static Levels CalculateLevels(IEnumerable<decimal> uPrice, IEnumerable<decimal> lPrice) {
            var lookback128 = 128;
            var lookback32 = 32;
            var lookback16 = 16;
            var lookback8 = 8;

            var lookBackResult128 = TechCalculations.LookBack(lookback128, uPrice, lPrice);
            var lookBackResult32 = TechCalculations.LookBack(lookback32, uPrice, lPrice);
            var lookBackResult16 = TechCalculations.LookBack(lookback16, uPrice, lPrice);
            var lookBackResult8 = TechCalculations.LookBack(lookback8, uPrice, lPrice);

            return new Levels() {
                Level128 = lookBackResult128,
                Level32 = lookBackResult32,
                Level16 = lookBackResult16,
                Level8 = lookBackResult8
            };
        }
    }
}
