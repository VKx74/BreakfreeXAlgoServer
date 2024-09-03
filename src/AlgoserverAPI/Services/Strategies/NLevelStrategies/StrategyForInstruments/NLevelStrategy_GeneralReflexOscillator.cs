using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.NLevelStrategy
{
    public class NLevelStrategy_GeneralReflexOscillator : NLevelStrategyBase
    {
        public NLevelStrategy_GeneralReflexOscillator(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<NLevelStrategyResponse> Calculate()
        {
            var settings = new NLevelStrategySettings
            {
                UseVolatilityFilter = false,
                // VolatilityGranularity = TimeframeHelper.MIN15_GRANULARITY,
                // VolatilityMin = -83,
                // VolatilityMax = 70,
                UseVolatilityFilter2 = false,
                // VolatilityGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                // VolatilityMin2 = -83,
                // VolatilityMax2 = 70,
                UseOverheatZone1DFilter = false,
                OverheatZone1DThreshold = 5,
                CheckTrends = false,
                // TrendFilters = new TrendFiltersSettings {
                //     trendfilter1x = true,
                //     trendfilter2x = true,
                //     trendfilter3x = true,
                // },
                CheckTrendsStrength = false,
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
                UseCatReflex = false,
                CatReflexGranularity = TimeframeHelper.HOURLY_GRANULARITY,
                CatReflexPeriodReflex = 30,
                CatReflexPeriodSuperSmoother = 8,
                CatReflexPeriodPostSmooth = 30,
                CatReflexConfirmationPeriod = 5,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 2,
                UseCatReflex2 = true,
                CatReflexGranularity2 = TimeframeHelper.MIN15_GRANULARITY,
                CatReflexPeriodReflex2 = 30,
                CatReflexPeriodSuperSmoother2 = 8,
                CatReflexPeriodPostSmooth2 = 30,
                CatReflexConfirmationPeriod2 = 5,
                CatReflexMinLevel2 = 0,
                CatReflexMaxLevel2 = 2
            };

            var result = await CalculateInternal(settings);
            result.DDClosePositions = true;
            result.DDCloseInitialInterval = 175;
            result.DDCloseIncreasePeriod = 135;
            result.DDCloseIncreaseThreshold = 0.5m;
            
            return result;
        }

    }
}