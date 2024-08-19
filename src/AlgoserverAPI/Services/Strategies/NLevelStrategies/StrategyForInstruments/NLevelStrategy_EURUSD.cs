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
                VolatilityMin = -95,
                VolatilityMax = -20,
                UseVolatilityFilter2 = true,
                VolatilityGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                VolatilityMin2 = -75,
                VolatilityMax2 = 90,
                UseOverheatZone1DFilter = true,
                OverheatZone1DThreshold = 5,
                CheckTrends = true,
                CheckTrendsStrength = true,
                LowGroupStrength = 0,
                HighGroupStrength = 2,
                CheckRSI = true,
                RSIMin = 40,
                RSIMax = 50,
                RSIPeriod = 65,
                CheckStrengthIncreasing = true,
                CheckStrengthReducePeriod = 5, // 24 in MQL settings, but in algo all data compressed and exists just on 5min interval so each value must be divided by 5
                CheckStrengthResetPeriod = 31,
                CheckStrengthReduceGranularity = TimeframeHelper.HOURLY_GRANULARITY,
                CheckStrengthResetGranularity = TimeframeHelper.MIN1_GRANULARITY * -1,
                CheckPeaks = true,
                PeakDetectionGranularity = TimeframeHelper.HOUR4_GRANULARITY,
                PeakDetectionPeriod = 55,
                PeakDetectionThreshold = 10,
            };

            var result = await CalculateInternal(settings);
            result.DDClosePositions = true;
            result.DDCloseInitialInterval = 150;
            result.DDCloseIncreasePeriod = 270;
            result.DDCloseIncreaseThreshold = 0.8m;

            return result;
        }

        protected override bool IsTrendCorrect()
        {
            var si = context.symbolInfo;
            var dir = si.TrendDirection;
            var UPTREND = 1;
            var DOWNTREND = -1;

            var condition1 = (si.Strength15M > 0 && si.Strength1H < 0) ||  // Prevent trading if 15m is positive and 1h is negative
                             (si.Strength15M < 0 && si.Strength1H > 0);     // Prevent trading if 15m is negative and 1h is positive

            var condition2 = (si.Strength1H > 0 && si.Strength4H < 0) ||  // Prevent trading if 1h is positive and 4h is negative
                             (si.Strength1H < 0 && si.Strength4H > 0);     // Prevent trading if 1h is negative and 4h is positive

            if (condition1 || condition2)
            {
                return false;
            }

            return true;
        }
    }
}