using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;
using Algoserver.API.Services;

namespace Algoserver.Strategies.NLevelStrategy.V2
{
    public class NLevelStrategy_USDJPY_v2 : NLevelStrategyBase
    {
        public NLevelStrategy_USDJPY_v2(NLevelStrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<NLevelStrategyResponse> Calculate()
        {
            var settings = new NLevelStrategySettings
            {
                UseVolatilityFilter = true,
                VolatilityGranularity = TimeframeHelper.MIN15_GRANULARITY,
                VolatilityMin = -47,
                VolatilityMax = 103,
                UseVolatilityFilter2 = true,
                VolatilityGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                VolatilityMin2 = -50,
                VolatilityMax2 = 94,
                UseOverheatZone1DFilter = true,
                OverheatZone1DThreshold = 5,
                CheckTrends = false,
                TrendFilters = new TrendFiltersSettings {
                    strengthConditionFilter15m = true,
                    strengthConditionFilter1h = true,
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
                CatReflexPeriodReflex = 41,
                CatReflexPeriodSuperSmoother = 29,
                CatReflexPeriodPostSmooth = 60,
                CatReflexConfirmationPeriod = 1,
                CatReflexMinLevel = 0.09,
                CatReflexMaxLevel = 2.2,

                UseCatReflex2 = true,
                CatReflexGranularity2 = TimeframeHelper.HOUR4_GRANULARITY,
                CatReflexPeriodReflex2 = 222,
                CatReflexPeriodSuperSmoother2 = 75,
                CatReflexPeriodPostSmooth2 = 126,
                CatReflexConfirmationPeriod2 = 2,
                CatReflexMinLevel2 = 0.04,
                CatReflexMaxLevel2 = 1.9
            };

            var result = await CalculateInternal(settings);
            result.DDClosePositions = true;
            result.DDCloseInitialInterval = 454;
            result.DDCloseIncreasePeriod = 67;
            result.DDCloseIncreaseThreshold = 9.3m;
            
            return result;
        }

    }
}