using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.NLevelStrategy.V2
{
    public class NLevelStrategy_AUDNZD_v2 : NLevelStrategyBase
    {
        public NLevelStrategy_AUDNZD_v2(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<NLevelStrategyResponse> Calculate()
        {
            var settings = new NLevelStrategySettings
            {
                UseVolatilityFilter = true,
                VolatilityGranularity = TimeframeHelper.MIN15_GRANULARITY,
                VolatilityMin = -80,
                VolatilityMax = 97,
                UseVolatilityFilter2 = true,
                VolatilityGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                VolatilityMin2 = -59,
                VolatilityMax2 = 79,
                UseOverheatZone1DFilter = true,
                OverheatZone1DThreshold = 5,
                CheckTrends = false,
                TrendFilters = new TrendFiltersSettings {
                    strengthConditionFilter5m = true
                },
                CheckTrendsStrength = true,
                LowGroupStrength = 0,
                HighGroupStrength = 1,
                CheckRSI = false,
                // RSIMin = 0,
                // RSIMax = 75,
                // RSIPeriod = 57,
                CheckStrengthIncreasing = false,
                // CheckStrengthReducePeriod = 4,
                // CheckStrengthResetPeriod = 6,
                // CheckStrengthReduceGranularity = TimeframeHelper.MIN1_GRANULARITY * -1,
                // CheckStrengthResetGranularity = TimeframeHelper.MIN1_GRANULARITY * -1,
                CheckPeaks = false,
                // PeakDetectionGranularity = TimeframeHelper.MIN15_GRANULARITY,
                // PeakDetectionPeriod = 93,
                // PeakDetectionThreshold = 80,
                CheckStochastic = false,
                // StochasticGranularity = TimeframeHelper.HOUR4_GRANULARITY, // 8H in settings, we dont have this TF
                // StochasticPeriodK = 120, // 60 in settings for 8H TF, 120 for 4H
                // StochasticPeriodD = 60, // 30 in settings for 8H TF, 60 for 4H
                // StochasticSmooth = 78, // 39 in settings for 8H TF, 78 for 4H
                // StochasticThreshold = 39,

                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.MIN1_GRANULARITY,
                CatReflexPeriodReflex = 40,
                CatReflexPeriodSuperSmoother = 86,
                CatReflexPeriodPostSmooth = 29,
                CatReflexConfirmationPeriod = 5,
                CatReflexMinLevel = 0.1,
                CatReflexMaxLevel = 2.1,

                UseCatReflex2 = true,
                CatReflexGranularity2 = TimeframeHelper.HOUR4_GRANULARITY,
                CatReflexPeriodReflex2 = 69,
                CatReflexPeriodSuperSmoother2 = 162,
                CatReflexPeriodPostSmooth2 = 1,
                CatReflexConfirmationPeriod2 = 4,
                CatReflexMinLevel2 = 0.01,
                CatReflexMaxLevel2 = 2.1
            };

            var result = await CalculateInternal(settings);
            result.DDClosePositions = true;
            result.DDCloseInitialInterval = 176;
            result.DDCloseIncreasePeriod = 135;
            result.DDCloseIncreaseThreshold = 0.5m;
            
            return result;
        }

    }
}