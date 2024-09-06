using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.Services
{

    public class RTDService
    {

        private readonly ILogger<AlgoService> _logger;
        private readonly HistoryService _historyService;

        public RTDService(ILogger<AlgoService> logger, HistoryService historyService)
        {
            _logger = logger;
            _historyService = historyService;
        }

        internal Task<RTDCalculationResponse> CalculateMESARTD(RTDCalculationRequest req, CancellationToken token)
        {
            return Task.Run(() =>
            {
                return calculateMESARTD(req);
            }, token);
        }

        private async Task<RTDCalculationResponse> calculateMESARTD(RTDCalculationRequest req)
        {

            var Exchange = req.Instrument.Exchange.ToLowerInvariant();
            var Datafeed = req.Instrument.Datafeed.ToLowerInvariant();
            var Type = req.Instrument.Type.ToLowerInvariant();
            var Symbol = req.Instrument.Id;

            var granularity = AlgoHelper.ConvertTimeframeToGranularity(req.Timeframe.Interval, req.Timeframe.Periodicity);
            var highTFGranularity = TimeframeHelper.DAILY_GRANULARITY;

            if (granularity <= TimeframeHelper.MIN1_GRANULARITY)
            {
                highTFGranularity = TimeframeHelper.HOURLY_GRANULARITY;
            }
            else if (granularity <= TimeframeHelper.MIN5_GRANULARITY)
            {
                highTFGranularity = TimeframeHelper.HOUR4_GRANULARITY;
            }

            var dailyPriceData = await _historyService.GetHistory(Symbol, highTFGranularity, Datafeed, Exchange, Type, req.BarsCount);

            if (dailyPriceData == null)
                return null;

            var calculation_input = dailyPriceData.Bars.Select(_ => _.Close).ToList();

            var rtd1 = TechCalculations.MESA(calculation_input, req.FastLimit, req.SlowLimit);
            var rtd2 = TechCalculations.MESA(calculation_input, req.FastLimit2, req.SlowLimit2);

            var trendsStrength = TrendDetector.MeasureTrendsStrength(rtd2, rtd1);

            if (rtd1.Length > req.BarsCount + 1)
            {
                rtd1 = rtd1.TakeLast(req.BarsCount + 1).ToArray();
            }
            if (rtd2.Length > req.BarsCount + 1)
            {
                rtd2 = rtd2.TakeLast(req.BarsCount + 1).ToArray();
            }

            var dates = dailyPriceData.Bars.Select(_ => _.Timestamp).ToArray();
            if (dates.Length > req.BarsCount + 1)
            {
                dates = dates.TakeLast(req.BarsCount + 1).ToArray();
            }

            return new RTDCalculationResponse
            {
                dates = dates,
                fast = rtd1.Select(_ => _.Fast),
                slow = rtd1.Select(_ => _.Slow),
                fast_2 = rtd2.Select(_ => _.Fast),
                slow_2 = rtd2.Select(_ => _.Slow),
                global_trend_spread = trendsStrength.GlobalTrendSpread,
                local_trend_spread = trendsStrength.LocalTrendSpread,
                global_avg = trendsStrength.GlobalAvg,
                local_avg = trendsStrength.LocalAvg,
                id = req.Id
            };
        }
    }
}
