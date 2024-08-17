using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Services;

namespace Algoserver.Strategies.NLevelStrategy
{
    public class NLevelStrategy_USDCHF : NLevelStrategyBase
    {
        public NLevelStrategy_USDCHF(NLevelStrategyInputContext _context) : base(_context)
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
                CheckTrendsStrength = true,
                LowGroupStrength = 0, 
                HighGroupStrength = 1,
                CheckRSI = true,
                RSIMin = 45,
                RSIMax = 75,
                RSIPeriod = 57,
                CheckStrengthIncreasing = true,
                CheckStrengthReducePeriod = 9,
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
    }
}