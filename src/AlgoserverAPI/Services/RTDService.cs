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
            return Task.Run(async () =>
            {
                return await calculateMESARTD(req);
            });
        }

        private async Task<RTDCalculationResponse> calculateMESARTD(RTDCalculationRequest req) {

            var Exchange = req.Instrument.Exchange.ToLowerInvariant();
            var Datafeed = req.Instrument.Datafeed.ToLowerInvariant();
            var Type = req.Instrument.Type.ToLowerInvariant();
            var Symbol = req.Instrument.Id;

            var dailyPriceData = await _historyService.GetHistory(Symbol, TimeframeHelper.DAILY_GRANULARITY, Datafeed, Exchange, Type, req.BarsCount);

            var calculation_input = dailyPriceData.Bars.Select(_ => _.Close).ToList();
            var dates = dailyPriceData.Bars.Select(_ => _.Timestamp).ToList();

            var rtd1 = TechCalculations.MESA(calculation_input, req.FastLimit, req.SlowLimit);
            var rtd2 = TechCalculations.MESA(calculation_input, req.FastLimit2, req.SlowLimit2);

            return new RTDCalculationResponse {
                dates = dates,
                fast = rtd1.Select(_ => _.Fast),
                slow = rtd1.Select(_ => _.Slow),
                fast_2 = rtd2.Select(_ => _.Fast),
                slow_2 = rtd2.Select(_ => _.Slow)
            };
        }
    }
}
