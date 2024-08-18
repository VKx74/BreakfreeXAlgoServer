using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Services;

namespace Algoserver.Strategies.NLevelStrategy
{
    public class NLevelStrategy_AUDCAD : NLevelStrategyBase
    {
        public NLevelStrategy_AUDCAD(NLevelStrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<NLevelStrategyResponse> Calculate()
        {
            var settings = new NLevelStrategySettings 
            {
                UseVolatilityFilter = true,
                VolatilityGranularity = TimeframeHelper.MIN15_GRANULARITY,
                VolatilityMin = -65,
                VolatilityMax = 50,
                UseVolatilityFilter2 = true,
                VolatilityGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                VolatilityMin2 = -85,
                VolatilityMax2 = 40,
                CheckTrends = false,
                CheckTrendsStrength = true,
                LowGroupStrength = 0, 
                HighGroupStrength = 1,
                CheckRSI = true,
                RSIMin = 39,
                RSIMax = 65,
                RSIPeriod = 30,
                CheckStrengthIncreasing = true,
                CheckStrengthReducePeriod = 3,
                CheckStrengthResetPeriod = 22,
                CheckStrengthReduceGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CheckStrengthResetGranularity = TimeframeHelper.MIN1_GRANULARITY * -1,
                CheckPeaks = true,
                PeakDetectionGranularity = TimeframeHelper.DAILY_GRANULARITY,
                PeakDetectionPeriod = 5,
                PeakDetectionThreshold = 40,
            };

            var result = await CalculateInternal(settings);
            result.DDClosePositions = true;
            result.DDCloseInitialInterval = 120;
            result.DDCloseIncreasePeriod = 330;
            result.DDCloseIncreaseThreshold = 0.1m;
            return result;
        }
    }
}