using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Algoserver.API.Exceptions;
using Algoserver.API.Helpers;
using Algoserver.API.Models.Algo;
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
        
        internal Task<RTDCalculationResponse> CalculateMESARTD(RTDCalculationRequest req)
        {
            return Task.Run(() =>
            {
                return calculateMESARTD(req);
            });
        }

        private async Task<RTDCalculationResponse> calculateMESARTD(RTDCalculationRequest req) {

            var Exchange = req.Instrument.Exchange.ToLowerInvariant();
            var Datafeed = req.Instrument.Datafeed.ToLowerInvariant();
            var Type = req.Instrument.Type.ToLowerInvariant();
            var Symbol = req.Instrument.Id;

            var dailyPriceData = await _historyService.GetHistory(Symbol, TimeframeHelper.DAILY_GRANULARITY, Datafeed, Exchange, Type, req.BarsCount);

            var calculation_input = dailyPriceData.Bars.Select(_ => _.Close).ToList();

            var rtd1 = TechCalculations.MESA(calculation_input, req.FastLimit, req.SlowLimit);
            var rtd2 = TechCalculations.MESA(calculation_input, req.FastLimit2, req.SlowLimit2);

            if (rtd1.Length > req.BarsCount + 1) {
                rtd1 = rtd1.TakeLast(req.BarsCount + 1).ToArray();
            }
            if (rtd2.Length > req.BarsCount + 1) {
                rtd2 = rtd2.TakeLast(req.BarsCount + 1).ToArray();
            }

            var mesa_local_value = rtd1.LastOrDefault();
            var mesa_global_value = rtd2.LastOrDefault();

            var globalTrendDiff = 0m;
            var localTrendDiff = 0m;
            if (mesa_global_value != null && mesa_local_value != null) {
                globalTrendDiff = Math.Abs(mesa_global_value.Fast - mesa_global_value.Slow) / Math.Min(mesa_global_value.Fast, mesa_global_value.Slow) * 100;
                localTrendDiff = Math.Abs(mesa_local_value.Fast - mesa_local_value.Slow) / Math.Min(mesa_local_value.Fast, mesa_local_value.Slow) * 100;
            }

            var dates = dailyPriceData.Bars.Select(_ => _.Timestamp).ToArray();
            if (dates.Length > req.BarsCount + 1) {
                dates = dates.TakeLast(req.BarsCount + 1).ToArray();
            }

            return new RTDCalculationResponse {
                dates = dates,
                fast = rtd1.Select(_ => _.Fast).ToArray(),
                slow = rtd1.Select(_ => _.Slow).ToArray(),
                fast_2 = rtd2.Select(_ => _.Fast).ToArray(),
                slow_2 = rtd2.Select(_ => _.Slow).ToArray(),
                global_trend_spread = globalTrendDiff,
                local_trend_spread = localTrendDiff
            };
        }
    }
}
