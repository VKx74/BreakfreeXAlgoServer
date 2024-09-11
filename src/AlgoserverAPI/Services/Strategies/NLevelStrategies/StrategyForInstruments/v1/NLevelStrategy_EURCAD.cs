using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.NLevelStrategy.V1
{
    public class NLevelStrategy_EURCAD : NLevelStrategyBase
    {
        public NLevelStrategy_EURCAD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<NLevelStrategyResponse> Calculate()
        {
            var settings = new NLevelStrategySettings
            {
                UseVolatilityFilter = true,
                VolatilityGranularity = TimeframeHelper.MIN15_GRANULARITY,
                VolatilityMin = -30,
                VolatilityMax = 10,
                UseVolatilityFilter2 = true,
                VolatilityGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                VolatilityMin2 = -30,
                VolatilityMax2 = 90,
                UseOverheatZone1DFilter = true,
                OverheatZone1DThreshold = 5,
                CheckTrends = true,
                TrendFilters = new TrendFiltersSettings {
                    strengthConditionFilter1h = true
                },
                CheckTrendsStrength = true,
                LowGroupStrength = 0,
                HighGroupStrength = 1,
                CheckRSI = true,
                RSIMin = 27,
                RSIMax = 71,
                RSIPeriod = 104,
                CheckStrengthIncreasing = true,
                CheckStrengthReducePeriod = 4,
                CheckStrengthResetPeriod = 6,
                CheckStrengthReduceGranularity = TimeframeHelper.MIN1_GRANULARITY * -1,
                CheckStrengthResetGranularity = TimeframeHelper.MIN1_GRANULARITY * -1,
                CheckPeaks = true,
                PeakDetectionGranularity = TimeframeHelper.MIN5_GRANULARITY,
                PeakDetectionPeriod = 96,
                PeakDetectionThreshold = 65,
            };

            var result = await CalculateInternal(settings);
            result.DDClosePositions = true;
            result.DDCloseInitialInterval = 9;
            result.DDCloseIncreasePeriod = 30;
            result.DDCloseIncreaseThreshold = 0.1m;

            return result;
        }

    }
}