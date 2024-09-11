using System;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;

namespace Algoserver.Strategies.LevelStrategy
{
    public abstract class LevelStrategyBase
    {
        protected const int UPTREND = 1;
        protected const int DOWNTREND = -1;
        protected const int SIDEWAYS = 0;
        protected bool ShowLogs = true;
        protected string StrategyName = "LevelsStrategy";
        protected readonly StrategyInputContext context;

        protected LevelStrategyBase(StrategyInputContext _context)
        {
            context = _context;
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

        protected void WriteLog(string str)
        {
            if (!ShowLogs)
            {
                return;
            }

            Console.WriteLine($"{StrategyName} >>> {str}");
        }

    }
}