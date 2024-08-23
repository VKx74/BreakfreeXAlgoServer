using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Services;

namespace Algoserver.Strategies.NLevelStrategy
{
    public class NLevelStrategy_BTCUSD : NLevelStrategyBase
    {
        public NLevelStrategy_BTCUSD(NLevelStrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<NLevelStrategyResponse> Calculate()
        {
            var settings = new NLevelStrategySettings
            {
                UseVolatilityFilter = true,
                VolatilityGranularity = TimeframeHelper.MIN15_GRANULARITY,
                VolatilityMin = -76,
                VolatilityMax = 64,
                UseVolatilityFilter2 = true,
                VolatilityGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                VolatilityMin2 = -53,
                VolatilityMax2 = -8,
                UseOverheatZone1DFilter = true,
                OverheatZone1DThreshold = 5,
                CheckTrends = true,
                TrendFilters = new TrendFiltersSettings {
                    strengthConditionFilter1h = true
                },
                CheckTrendsStrength = true,
                LowGroupStrength = 0,
                HighGroupStrength = 27,
                CheckRSI = true,
                RSIMin = 40,
                RSIMax = 57,
                RSIPeriod = 97,
                CheckStrengthIncreasing = false, // in backtest settings strengthResetDetectionTimeframe=-1 values that is not correct to use this filter
                CheckStrengthReducePeriod = 5, // 21 in MQL settings, but in algo all data compressed and exists just on 5min interval so each value must be divided by 5
                CheckStrengthResetPeriod = 6,
                CheckStrengthReduceGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CheckStrengthResetGranularity = TimeframeHelper.MIN1_GRANULARITY * -1,
                CheckPeaks = false,
                PeakDetectionGranularity = TimeframeHelper.MIN15_GRANULARITY,
                PeakDetectionPeriod = 144,
                PeakDetectionThreshold = 70,
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