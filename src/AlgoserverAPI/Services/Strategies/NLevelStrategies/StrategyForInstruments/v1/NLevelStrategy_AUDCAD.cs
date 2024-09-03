using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.NLevelStrategy.V1
{
    public class NLevelStrategy_AUDCAD : NLevelStrategyBase
    {
        public NLevelStrategy_AUDCAD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<NLevelStrategyResponse> Calculate()
        {
            var settings = new NLevelStrategySettings 
            {
                UseVolatilityFilter = true,
                VolatilityGranularity = TimeframeHelper.MIN15_GRANULARITY,
                VolatilityMin = -90,
                VolatilityMax = 100,
                UseVolatilityFilter2 = true,
                VolatilityGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                VolatilityMin2 = -60,
                VolatilityMax2 = 100,
                UseOverheatZone1DFilter = true,
                OverheatZone1DThreshold = 5,
                CheckTrends = true,
                TrendFilters = new TrendFiltersSettings {
                    strengthConditionFilter1m = true,
                    strengthConditionFilter5m = true,
                    strengthConditionFilter15m = true,
                    strengthConditionFilter1h = true,
                },
                CheckTrendsStrength = true,
                LowGroupStrength = 0, 
                HighGroupStrength = 1,
                CheckRSI = true,
                RSIMin = 15,
                RSIMax = 100,
                RSIPeriod = 30,
                CheckStrengthIncreasing = true,
                CheckStrengthReducePeriod = 4,
                CheckStrengthResetPeriod = 6,
                CheckStrengthReduceGranularity = TimeframeHelper.MIN1_GRANULARITY * -1,
                CheckStrengthResetGranularity = TimeframeHelper.MIN1_GRANULARITY * -1,
                CheckPeaks = true,
                PeakDetectionGranularity = TimeframeHelper.MIN5_GRANULARITY,
                PeakDetectionPeriod = 90,
                PeakDetectionThreshold = 30,
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