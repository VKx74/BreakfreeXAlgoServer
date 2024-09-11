using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.NLevelStrategy.V1
{
    public class NLevelStrategy_USDCHF : NLevelStrategyBase
    {
        public NLevelStrategy_USDCHF(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<NLevelStrategyResponse> Calculate()
        {
            var settings = new NLevelStrategySettings
            {
                UseVolatilityFilter = true,
                VolatilityGranularity = TimeframeHelper.MIN15_GRANULARITY,
                VolatilityMin = -94,
                VolatilityMax = 0,
                UseVolatilityFilter2 = true,
                VolatilityGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                VolatilityMin2 = -86,
                VolatilityMax2 = -9,
                UseOverheatZone1DFilter = true,
                OverheatZone1DThreshold = 5,
                CheckTrends = true,
                TrendFilters = new TrendFiltersSettings
                {
                    trendfilter1x = true,
                    trendfilter2x = true,
                    trendfilter3x = true,
                },
                CheckTrendsStrength = true,
                LowGroupStrength = 0,
                HighGroupStrength = 1,
                CheckRSI = true,
                RSIMin = 45,
                RSIMax = 75,
                RSIPeriod = 57,
                CheckStrengthIncreasing = true,
                CheckStrengthReducePeriod = 2, // 9 in MQL settings, but in algo all data compressed and exists just on 5min interval so each value must be divided by 5
                CheckStrengthResetPeriod = 27,
                CheckStrengthReduceGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CheckStrengthResetGranularity = TimeframeHelper.MIN1_GRANULARITY * -1,
                CheckPeaks = true,
                PeakDetectionGranularity = TimeframeHelper.MIN1_GRANULARITY,
                PeakDetectionPeriod = 35,
                PeakDetectionThreshold = 90,
            };

            var result = await CalculateInternal(settings);
            result.DDClosePositions = true;
            result.DDCloseInitialInterval = 175;
            result.DDCloseIncreasePeriod = 135;
            result.DDCloseIncreaseThreshold = 0.5m;
            return result;
        }

        protected override bool IsTrendCorrect(TrendFiltersSettings settings)
        {
            var si = context.symbolInfo;
            var dir = si.TrendDirection;
            var UPTREND = 1;
            var DOWNTREND = -1;

            var condition1 = (dir == DOWNTREND && si.Strength5M <= si.Strength1M && si.Strength15M <= si.Strength5M) ||
    (dir == UPTREND && si.Strength5M >= si.Strength1M && si.Strength15M >= si.Strength5M);

            var condition2 = !(si.Strength5M > 0 && si.Strength15M < 0); // Prevent trading if 5m is positive and 15m is negative in uptrend
            var condition3 = !(si.Strength5M < 0 && si.Strength15M > 0); // Prevent trading if 5m is negative and 15m is positive in downtrend
            var condition4 = !(si.Strength15M > 0 && si.Strength1H < 0); // Prevent trading if 15m is positive and 1h is negative in uptrend
            var condition5 = !(si.Strength15M < 0 && si.Strength1H > 0); // Prevent trading if 15m is negative and 1h is positive in downtrend

            if (condition1 && condition2 && condition3 && condition4 && condition5)
            {
                return true;
            }

            return false;
        }
    }
}