using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Services;

namespace Algoserver.Strategies.LevelStrategy
{
    public abstract class LevelStrategyBase
    {
        protected const int UPTREND = 1;
        protected const int DOWNTREND = -1;
        protected const int SIDEWAYS = 0;
        protected bool ShowLogs = true;
        protected string StrategyName = "LevelsStrategy";
        protected StringBuilder Logs = new StringBuilder();
        protected readonly StrategyInputContext context;

        protected LevelStrategyBase(StrategyInputContext _context)
        {
            context = _context;
        }
        
        public string GetLogs() {
            return Logs.ToString();
        }

        protected async Task<bool> CheckReflexOscillator(int granularity, double periodSuperSmoother, int reflexPeriod, double periodPostSmooth, double min, double max, int confirmationPeriod, bool validateZeroCrossover)
        {
            var barsCount = 3000;

            var history = await context.historyService.GetHistory(context.symbol, granularity, context.datafeed, context.exchange, context.type, 0, barsCount);
            var close = history.Bars.Select((_) => _.Close).TakeLast(barsCount).Reverse().ToArray();

            var reflexValues = TechCalculations.ReflexOscillatorTradingView(close, periodSuperSmoother, reflexPeriod, periodPostSmooth);

            var result = true;

            if (!CheckReflexIndicatorConditions(reflexValues, min, max, confirmationPeriod))
            {
                WriteLog($"{context.symbol}_{granularity}({reflexPeriod}, {periodSuperSmoother}, {periodPostSmooth}) => base conditions not correct");
                result = false;
            }

            if (result && validateZeroCrossover)
            {
                if (!CheckReflexIndicatorResetCondition(reflexValues, min, max, confirmationPeriod))
                {
                    WriteLog($"{context.symbol}_{granularity}({reflexPeriod}, {periodSuperSmoother}, {periodPostSmooth}) => reset conditions not correct");
                    result = false;
                }
            }

            double currentReflexValue = reflexValues[0];
            double previouse1ReflexValue = reflexValues[1];
            double previouse2ReflexValue = reflexValues[2];

            WriteLog($"{context.symbol}_{granularity}({reflexPeriod}, {periodSuperSmoother}, {periodPostSmooth}) => {currentReflexValue}, {previouse1ReflexValue}, {previouse2ReflexValue}");

            return result;
        }

        protected bool CheckReflexIndicatorResetCondition(double[] data, double min, double max, int confirmationPeriod)
        {
            double firstValue = data[0];
            int dataCount = data.Length;
            int end = dataCount - confirmationPeriod - 1;
            bool stopTradingConditionDetected = false;
            for (int i = 1; i < end; i++)
            {
                double currentValue = data[i];
                // reverse detected
                if ((firstValue > 0 && currentValue < 0) || (firstValue < 0 && currentValue > 0))
                {
                    return true;
                }

                double[] source = data.Skip(i).ToArray();
                bool canTrade = CheckReflexIndicatorConditions(source, min, max, confirmationPeriod);
                if (!canTrade && !stopTradingConditionDetected)
                {
                    stopTradingConditionDetected = true;
                }

                if (canTrade && stopTradingConditionDetected)
                {
                    return false;
                }
            }
            return true;
        }

        protected bool CheckReflexIndicatorConditions(double[] data, double min, double max, int confirmationPeriod)
        {
            var si = context.symbolInfo;
            var dir = si.TrendDirection;

            if (dir == SIDEWAYS)
            {
                return false;
            }

            double currentReflexValue = data[0];
            if (dir == UPTREND)
            {
                if (!(currentReflexValue > min && currentReflexValue < max))
                {
                    return false;
                }
            }
            if (dir == DOWNTREND)
            {
                if (!(currentReflexValue < min * -1 && currentReflexValue > max * -1))
                {
                    return false;
                }
            }

            if (!ConfirmReflexIndicatorDirection(data, confirmationPeriod, dir))
            {
                return false;
            }

            return true;
        }

        protected bool ConfirmReflexIndicatorDirection(double[] data, int period, int dir)
        {
            int arraySize = data.Length;
            int end = Math.Min(arraySize - 1, period);

            for (int i = 0; i < end; i++)
            {
                double current = data[i];
                double previouse = data[i + 1];

                if (dir == UPTREND && current < previouse)
                {
                    return false;
                }
                if (dir == DOWNTREND && current > previouse)
                {
                    return false;
                }
            }

            return true;
        }
        
        protected bool IsEnoughStrength(decimal lowStrength, decimal highStrength)
        {
            var symbolInfo = context.symbolInfo;
            var shortGroupStrength = symbolInfo.ShortGroupStrength * 100;
            var midGroupStrength = symbolInfo.MidGroupStrength * 100;
            var longGroupStrength = symbolInfo.LongGroupStrength * 100;

            if (symbolInfo.TrendDirection == 1)
            {
                // Uptrend
                if (longGroupStrength < highStrength || midGroupStrength < highStrength || shortGroupStrength < lowStrength)
                {
                    return false;
                }
            }
            else if (symbolInfo.TrendDirection == -1)
            {
                // Downtrend
                if (longGroupStrength > highStrength * -1 || midGroupStrength > highStrength * -1 || shortGroupStrength > lowStrength * -1)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }


        protected decimal GetDefaultSL()
        {
            var hourSL = GetSL(TimeframeHelper.HOURLY_GRANULARITY, context.symbolInfo.TrendDirection);
            return hourSL;
        }

        protected decimal GetDefaultOppositeSL()
        {
            var oppositeHourSL = GetSL(TimeframeHelper.HOURLY_GRANULARITY, context.symbolInfo.TrendDirection > 0 ? -1 : 1);
            return oppositeHourSL;
        }

        protected decimal GetSL(int granularity, int trendDirection)
        {
            var data = context.levelsResponse;
            if (data.TryGetValue(granularity, out var item))
            {
                var lastSar = item.sar.LastOrDefault();
                if (lastSar == null)
                {
                    return 0;
                }

                var halsBand = GetHalfBand(granularity, trendDirection);
                if (trendDirection > 0)
                {
                    return lastSar.s - halsBand;
                }
                if (trendDirection < 0)
                {
                    return lastSar.r + halsBand;
                }
            }
            return 0;
        }
        
        protected decimal GetHalfBand(int granularity, int trendDirection)
        {
            var data = context.levelsResponse;
            if (data.TryGetValue(granularity, out var item))
            {
                var lastSar = item.sar.LastOrDefault();
                if (lastSar == null)
                {
                    return 0;
                }

                var rH = (lastSar.r_p28 - lastSar.r) / 6;
                var sH = (lastSar.s - lastSar.s_m28) / 6;

                if (trendDirection > 0)
                {
                    return sH;
                }
                if (trendDirection < 0)
                {
                    return rH;
                }
            }
            return 0;
        }

        protected void WriteLog(string str)
        {
            Logs.AppendLine(str);

            if (!ShowLogs)
            {
                return;
            }

            Console.WriteLine($"{StrategyName} >>> {str}");
        }
    }
}