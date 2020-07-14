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
    public class AlgoService
    {
        private const int DAILYG_RANULARITY = 86400;

        private readonly ILogger<HistoryService> _logger;
        private readonly HistoryService _historyService;

        public AlgoService(ILogger<HistoryService> logger, HistoryService historyService)
        {
            _logger = logger;
            _historyService = historyService;
        }

        public async Task<CalculationResponse> CalculateAsync(CalculationRequest req)
        {
            var provider = req.Instrument.Datafeed.ToLowerInvariant();
            if (provider != "twelvedata" || provider != "oanda")
            {
                throw new ApiException(HttpStatusCode.BadRequest,
                    $"Unsupported '{provider}' datafeed. Available 'twelvedata' or 'oanda' only.");
            }

            var container = InputDataContainer.MapCalculationRequestToInputDataContainer(req);
            var granularity = AlgoHelper.ConvertTimeframeToCranularity(container.TimeframeMultiplier, container.TimeframePeriod);
            var currentPriceData = await _historyService.GetHistory(container.Symbol, granularity, provider, container.Exchange);
            var dailyPriceData = await _historyService.GetHistory(container.Symbol, DAILYG_RANULARITY, provider, container.Exchange);
            throw new NotImplementedException();
        }
    }
}
