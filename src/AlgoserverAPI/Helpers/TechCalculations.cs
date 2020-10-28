using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.Algo;

namespace Algoserver.API.Helpers
{
    public class DataAggregator
    {
        private List<double> _data = new List<double>();

        public double get(int index)
        {
            var i = _data.Count - index - 1;
            if (i < 0)
            {
                return 0;
            }

            return _data[i];
        }

        public void add(double value)
        {
            _data.Add(value);
        }

        public void update(double value)
        {
            var count = _data.Count;
            if (count == 0)
            {
                return;
            }

            _data[count - 1] = value;
        }
    }

    public class MESAData
    {
        public decimal Fast { get; set; }
        public decimal Slow { get; set; }
    }

    public class LookBackResult
    {
        public decimal EightEight { get; set; }
        public decimal FourEight { get; set; }
        public decimal ZeroEight { get; set; }
        public decimal Increment { get; set; }
        public decimal AbsTop { get; set; }
        public decimal Minus18 { get; set; }
        public decimal Minus28 { get; set; }
        public decimal Plus28 { get; set; }
        public decimal Plus18 { get; set; }

        public static bool IsEquals(LookBackResult obj1, LookBackResult obj2)
        {
            if (obj1.EightEight != obj2.EightEight)
            {
                return false;
            }
            if (obj1.FourEight != obj2.FourEight)
            {
                return false;
            }
            if (obj1.ZeroEight != obj2.ZeroEight)
            {
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

        public static bool IsEquals(Levels obj1, Levels obj2)
        {
            if (obj1.Level128.EightEight != obj2.Level128.EightEight) return false;
            if (obj1.Level128.FourEight != obj2.Level128.FourEight) return false;
            if (obj1.Level128.ZeroEight != obj2.Level128.ZeroEight) return false;

            return true;
        }
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

        public static List<decimal> StdDev(List<decimal> data, int length)
        {
            var res = new List<decimal>();
            var sumDataRow = new List<decimal>();

            for (var i = 0; i < data.Count; i++)
            {
                if (i == 0)
                {
                    sumDataRow.Add(data[i]);
                }
                else
                {
                    sumDataRow.Add(data[i] + sumDataRow[i - 1] -
                        (i >= length ? data[i - length] : 0));
                }

                var avg = sumDataRow.LastOrDefault() / Math.Min(i + 1, length);
                var sum = 0m;
                for (var barsBack = Math.Min(i, length - 1); barsBack >= 0; barsBack--)
                {
                    var val = data[i - barsBack] - avg;
                    sum += val * val;
                }
                var stdDev = Math.Sqrt((double)sum / Math.Min(i + 1, length));
                res.Add((decimal)stdDev);
            }
            return res;
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

        public static MESAData[] MESA(List<decimal> data, double fast, double slow)
        {
            var PI = 2 * Math.Asin(1);
            var prevMesaPeriod = 0.0;
            var detrender = new DataAggregator();
            var smoothed = new DataAggregator();
            var prevQ2 = 0.0;
            var prevI2 = 0.0;
            var prevRe = 0.0;
            var prevIm = 0.0;
            var prevPhase = 0.0;
            var prevMema = 1.0;
            var prevFema = 1.0;
            // var prevOscillatorPoint = 0.0;

            var res = new List<MESAData>();
            var i = 0;
            // if (data.Count > 100) {
            //     i = data.Count - 100;
            // }
            for (; i < data.Count; i++)
            {
                var src0 = (double)data[i];
                var src1 = (double)(i > 0 ? data[i - 1] : 0);
                var src2 = (double)(i > 1 ? data[i - 2] : 0);
                var src3 = (double)(i > 2 ? data[i - 3] : 0);
                var mesaPeriodMult = 0.075 * prevMesaPeriod + 0.54;
                smoothed.add(4 * src0 + 3 * src1 + 2 * src2 + src3 / 10);
                detrender.add(_computeComponent(smoothed, mesaPeriodMult));
                var I1Value = detrender.get(3);
                var Q1Value = _computeComponent(detrender, mesaPeriodMult);
                var jI = 0.0962 * I1Value * mesaPeriodMult;
                var jQ = 0.0962 * Q1Value * mesaPeriodMult;
                var I2Value = I1Value - jQ;
                var Q2Value = Q1Value + jI;

                I2Value = 0.2 * I2Value + 0.8 * prevI2;
                Q2Value = 0.2 * Q2Value + 0.8 * prevQ2;

                var ReValue = I2Value * prevI2 + Q2Value * prevQ2;
                var ImValue = I2Value * prevQ2 - Q2Value * prevI2;
                ReValue = 0.2 * ReValue + 0.8 * prevRe;
                ImValue = 0.2 * ImValue + 0.8 * prevIm;

                prevRe = ReValue;
                prevIm = ImValue;
                prevI2 = I2Value;
                prevQ2 = Q2Value;

                var mesaPeriod = 0.0;

                if (ReValue != 0 && ImValue != 0)
                {
                    mesaPeriod = 2 * PI / Math.Atan(ImValue / ReValue);
                }

                if (mesaPeriod > 1.5 * prevMesaPeriod)
                {
                    mesaPeriod = 1.5 * prevMesaPeriod;
                }

                if (mesaPeriod > 50)
                {
                    mesaPeriod = 50;
                }

                if (mesaPeriod < 0.67 * prevMesaPeriod)
                {
                    mesaPeriod = 0.67 * prevMesaPeriod;
                }

                if (mesaPeriod < 6)
                {
                    mesaPeriod = 6;
                }

                prevMesaPeriod = 0.2 * mesaPeriod + 0.8 * prevMesaPeriod;

                var phase = 0.0;
                if (I1Value != 0)
                    phase = (180 / PI) * Math.Atan(Q1Value / I1Value);

                var deltaPhase = prevPhase - phase;
                prevPhase = phase;

                if (deltaPhase < 1)
                {
                    deltaPhase = 1;
                }

                var alpha = fast / deltaPhase;

                if (alpha < slow)
                {
                    alpha = slow;
                }

                var alpha2 = alpha / 2;

                var memaValue = alpha * (double)data[i] + (1 - alpha) * prevMema;
                var famaValue = alpha2 * memaValue + (1 - alpha2) * prevFema;
                // prevOscillatorPoint = _getOscillatorPoint(data, i, prevOscillatorPoint);
                res.Add(new MESAData
                {
                    Fast = (decimal)(memaValue),
                    Slow = (decimal)(famaValue)
                });

                prevMema = memaValue;
                prevFema = famaValue;

            }

            return res.ToArray();
        }

        private static double _computeComponent(DataAggregator src, double mesaPeriodMultiplier)
        {
            var hilbertTransform = 0.0962 * src.get(0) + 0.5769 * src.get(2) - 0.5769 * src.get(4) - 0.0962 * src.get(6);
            return hilbertTransform * mesaPeriodMultiplier;
        }

        private static double _getOscillatorPoint(List<decimal> data, int currentBar, double prevOscillatorPoint)
        {
            var count = currentBar <= 32 ? currentBar : 32;
            if (count == 0)
            {
                return (double)data.FirstOrDefault();
            }

            var sum = 0m;
            for (var i = 0; i < count; i++)
            {
                sum += data[currentBar - i];
            }

            var res = (double)(sum / count);
            if (prevOscillatorPoint > 0)
            {
                res = ((prevOscillatorPoint * 9) + res) / 10;
            }

            return res;
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

        public static LookBackResult LookBack(int lookback, IEnumerable<decimal> uPrice, IEnumerable<decimal> lPrice)
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
            result.Minus18 = AbsTop - (12 * Increment);
            result.Minus28 = AbsTop - (13 * Increment);
            result.Plus28 = AbsTop - Increment;
            result.Plus18 = AbsTop - (2 * Increment);

            return result;
        }

        public static Levels CalculateLevels(IEnumerable<decimal> uPrice, IEnumerable<decimal> lPrice)
        {
            var lookback128 = 128;
            var lookback32 = 32;
            var lookback16 = 16;
            var lookback8 = 8;

            var lookBackResult128 = TechCalculations.LookBack(lookback128, uPrice, lPrice);
            var lookBackResult32 = TechCalculations.LookBack(lookback32, uPrice, lPrice);
            var lookBackResult16 = TechCalculations.LookBack(lookback16, uPrice, lPrice);
            var lookBackResult8 = TechCalculations.LookBack(lookback8, uPrice, lPrice);

            return new Levels()
            {
                Level128 = lookBackResult128,
                Level32 = lookBackResult32,
                Level16 = lookBackResult16,
                Level8 = lookBackResult8
            };
        }

        public static LookBackResult CalculateLevel128(IEnumerable<decimal> uPrice, IEnumerable<decimal> lPrice)
        {
            var lookback128 = 128;
            return TechCalculations.LookBack(lookback128, uPrice, lPrice);
        }

        public static int BRCOverLevelCount(IEnumerable<decimal> cPrice, Trend trend, decimal level)
        {
            var close = cPrice.ToArray().Reverse().ToArray();

            for (var i = 0; i < close.Length; i++)
            {
                if (trend == Trend.Up && close[i] >= level)
                {
                    continue;
                }
                if (trend == Trend.Down && close[i] <= level)
                {
                    continue;
                }
                return i;
            }
            return 0;
        }

        public static int BRCBelowLevelCount(IEnumerable<decimal> cPrice, Trend trend, decimal level, int count)
        {
            var close = cPrice.ToArray().Reverse().ToArray();
            var i = 0;

            for (; i < close.Length; i++)
            {
                if (trend == Trend.Up && close[i] >= level)
                {
                    continue;
                }
                if (trend == Trend.Down && close[i] <= level)
                {
                    continue;
                }
                break;
            }

            var c = 0;
            var j = 0;

            for (; i < close.Length && j < count; i++, j++)
            {
                if (trend == Trend.Up && close[i] <= level)
                {
                    c++;
                    continue;
                }
                if (trend == Trend.Down && close[i] >= level)
                {
                    c++;
                    continue;
                }
            }

            return c;
        }

        public static bool ApproveDirection(List<decimal> cPrice, Trend trend, TradeType type)
        {
            var period = 14;
            List<decimal> prices = null;

            if (cPrice.Count > 100) {
                prices = cPrice.TakeLast(100).ToList();
            } else {
                prices = cPrice;
            }

            var hmaData = TechCalculations.Hma(prices, period);
            var lookback = 4;
            var lastClose = cPrice.TakeLast(lookback + 1).ToArray();
            var lastHma = hmaData.TakeLast(lookback + 1).ToArray();
            for (var i = 0; i < lookback; i++)
            {
                if (trend == Trend.Up)
                {
                    if (lastClose[i] >= lastHma[i])
                    {
                        return false;
                    }
                }
                else
                {
                    if (lastClose[i] <= lastHma[i])
                    {
                        return false;
                    }
                }
            }

            var l = lastHma.Length;
            if (trend == Trend.Up)
            {
                if (lastHma[l - 1] >= lastHma[l - 2])
                {
                    return false;
                }
            }
            else
            {
                if (lastHma[l - 1] <= lastHma[l - 2])
                {
                    return false;
                }
            }

            return true;
        }

        public static decimal CalculateAvdCandleDifference(IEnumerable<decimal> open, IEnumerable<decimal> close)
        {
            var lookback = 5;
            var opens = open.ToArray();
            var closes = close.ToArray();
            var closesLength = closes.Length;
            var total = 0m;
            for (var i = 1; i <= lookback; i++)
            {
                total += Math.Abs(closes[closesLength - i] - opens[closesLength - i]);
            }
            return total / lookback;
        }

        public static decimal CalculatePriceMoveDirection(IEnumerable<decimal> cPrice)
        {
            var lookback = 5;
            var closes = cPrice.ToArray();
            var closesLength = closes.Length;

            return closes[closesLength - 2] - closes[closesLength - lookback - 1];
        }
    }
}
