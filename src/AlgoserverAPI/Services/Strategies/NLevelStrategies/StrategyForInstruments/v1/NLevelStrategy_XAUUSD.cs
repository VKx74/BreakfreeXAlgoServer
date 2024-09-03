using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.NLevelStrategy.V1
{
    public class NLevelStrategy_XAUUSD : NLevelStrategyBase
    {
        public NLevelStrategy_XAUUSD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<NLevelStrategyResponse> Calculate()
        {
            var settings = new NLevelStrategySettings 
            {
                UseVolatilityFilter = true,
                VolatilityGranularity = TimeframeHelper.MIN15_GRANULARITY,
                VolatilityMin = -60,
                VolatilityMax = 90,
                UseVolatilityFilter2 = true,
                VolatilityGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                VolatilityMin2 = -55,
                VolatilityMax2 = 60,
                UseOverheatZone1DFilter = true,
                OverheatZone1DThreshold = 5,
                CheckTrends = true,
                CheckTrendsStrength = true,
                LowGroupStrength = 0, 
                HighGroupStrength = 2,
                CheckRSI = true,
                RSIMin = 30,
                RSIMax = 100,
                RSIPeriod = 90,
                CheckStrengthIncreasing = true,
                CheckStrengthReducePeriod = 3, // 11 in MQL settings, but in algo all data compressed and exists just on 5min interval so each value must be divided by 5
                CheckStrengthResetPeriod = 6, // 28 in MQL settings, but in algo all data compressed and exists just on 5min interval so each value must be divided by 5
                CheckStrengthReduceGranularity = TimeframeHelper.HOURLY_GRANULARITY,
                CheckStrengthResetGranularity = TimeframeHelper.MIN5_GRANULARITY,
                CheckPeaks = true,
                PeakDetectionGranularity = TimeframeHelper.MIN5_GRANULARITY,
                PeakDetectionPeriod = 25,
                PeakDetectionThreshold = 60,
            };

            var result = await CalculateInternal(settings);
            result.DDClosePositions = true;
            result.DDCloseInitialInterval = 150;
            result.DDCloseIncreasePeriod = 270;
            result.DDCloseIncreaseThreshold = 0.8m;
            return result;
        }

        protected override bool IsTrendCorrect(TrendFiltersSettings settings)
        {
            var si = context.symbolInfo;
            var dir = si.TrendDirection;
            var UPTREND = 1;
            var DOWNTREND = -1;

            // var condition1 = (dir == DOWNTREND && si.Strength5M <= si.Strength1M && si.Strength15M <= si.Strength5M) ||
            //     (dir == UPTREND && si.Strength5M >= si.Strength1M && si.Strength15M >= si.Strength5M);

            var condition2 = !(si.Strength5M > 0 && si.Strength15M < 0); // Prevent trading if 5m is positive and 15m is negative in uptrend
            var condition3 = !(si.Strength5M < 0 && si.Strength15M > 0); // Prevent trading if 5m is negative and 15m is positive in downtrend
            var condition4 = !(si.Strength15M > 0 && si.Strength1H < 0); // Prevent trading if 15m is positive and 1h is negative in uptrend
            var condition5 = !(si.Strength15M < 0 && si.Strength1H > 0); // Prevent trading if 15m is negative and 1h is positive in downtrend
            var condition6 = !(si.Strength1H > 0 && si.Strength4H < 0); // Prevent trading if 1h is positive and 4h is negative in uptrend
            var condition7 = !(si.Strength1H < 0 && si.Strength4H > 0); // Prevent trading if 1h is negative and 4h is positive in downtrend


            if (condition2 && condition3 && condition4 && condition5 && condition6 && condition7)
            {
                return true;
            }

            return false;
        }
    }
}