using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Services;

namespace Algoserver.Strategies.NLevelStrategy
{
    public class NLevelStrategy_EURUSD : NLevelStrategyBase
    {
        public NLevelStrategy_EURUSD(NLevelStrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<NLevelStrategyResponse> Calculate()
        {
            var settings = new NLevelStrategySettings 
            {
                UseVolatilityFilter = true,
                VolatilityGranularity = TimeframeHelper.MIN15_GRANULARITY,
                VolatilityMin = -83,
                VolatilityMax = 70,
                UseOverheatZone1DFilter = true,
                OverheatZone1DThreshold = 5,
                CheckTrends = true,
                CheckTrendsStrength = true,
                LowGroupStrength = 0, 
                HighGroupStrength = 1,
                CheckRSI = true,
                RSIMin = 7,
                RSIMax = 57,
                RSIPeriod = 91,
                CheckStrengthIncreasing = true,
                CheckStrengthReducePeriod = 4,
                CheckStrengthResetPeriod = 6,
                CheckStrengthReduceGranularity = TimeframeHelper.MIN1_GRANULARITY * -1,
                CheckStrengthResetGranularity = TimeframeHelper.MIN1_GRANULARITY * -1,
                CheckPeaks = false,
                PeakDetectionGranularity = TimeframeHelper.MIN1_GRANULARITY,
                PeakDetectionPeriod = 35,
                PeakDetectionThreshold = 90,
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}