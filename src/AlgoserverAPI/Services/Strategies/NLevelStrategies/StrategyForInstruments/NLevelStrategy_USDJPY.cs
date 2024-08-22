using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Services;

namespace Algoserver.Strategies.NLevelStrategy
{
    public class NLevelStrategy_USDJPY : NLevelStrategyBase
    {
        public NLevelStrategy_USDJPY(NLevelStrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<NLevelStrategyResponse> Calculate()
        {
            var settings = new NLevelStrategySettings
            {
                UseVolatilityFilter = true,
                VolatilityGranularity = TimeframeHelper.MIN15_GRANULARITY,
                VolatilityMin = -45,
                VolatilityMax = 60,
                UseVolatilityFilter2 = true,
                VolatilityGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                VolatilityMin2 = -95,
                VolatilityMax2 = 60,
                UseOverheatZone1DFilter = true,
                OverheatZone1DThreshold = 5,
                CheckTrends = true,
                CheckTrendsStrength = true,
                LowGroupStrength = 0,
                HighGroupStrength = 2,
                CheckRSI = true,
                RSIMin = 55,
                RSIMax = 95,
                RSIPeriod = 70,
                CheckStrengthIncreasing = true,
                CheckStrengthReducePeriod = 3, // 11 in MQL settings, but in algo all data compressed and exists just on 5min interval so each value must be divided by 5
                CheckStrengthResetPeriod = 6, // 28 in MQL settings, but in algo all data compressed and exists just on 5min interval so each value must be divided by 5
                CheckStrengthReduceGranularity = TimeframeHelper.HOURLY_GRANULARITY,
                CheckStrengthResetGranularity = TimeframeHelper.MIN5_GRANULARITY,
                CheckPeaks = true,
                PeakDetectionGranularity = TimeframeHelper.DAILY_GRANULARITY,
                PeakDetectionPeriod = 20,
                PeakDetectionThreshold = 5,
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

            var isTrendValid = true;

            if (!(dir == DOWNTREND && si.Strength5M <= si.Strength1M) &&
                     !(dir == UPTREND && si.Strength5M >= si.Strength1M))
            {
                isTrendValid = false;
            }

            return isTrendValid;
        }
    }
}