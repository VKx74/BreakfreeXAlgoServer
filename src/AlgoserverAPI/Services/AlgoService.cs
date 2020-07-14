using System;
using System.Collections.Generic;
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

        private readonly ILogger<AlgoService> _logger;
        private readonly HistoryService _historyService;
        private readonly PriceRatioCalculationService _priceRatioCalculationService;

        public AlgoService(ILogger<AlgoService> logger, HistoryService historyService, PriceRatioCalculationService priceRatioCalculationService)
        {
            _logger = logger;
            _historyService = historyService;
            _priceRatioCalculationService = priceRatioCalculationService;
        }

        public async Task<CalculationResponse> CalculateAsync(CalculationRequest req)
        {
            var container = InputDataContainer.MapCalculationRequestToInputDataContainer(req);
            if (container.Datafeed != "twelvedata" && container.Datafeed != "oanda")
            {
                throw new ApiException(HttpStatusCode.BadRequest,
                    $"Unsupported '{container.Datafeed}' datafeed. Available 'twelvedata' or 'oanda' only.");
            }

            var usdRatio = 1m;
            if (container.Type != "forex") {
                usdRatio = await _priceRatioCalculationService.GetUSDRatio(container.Symbol, container.Datafeed, container.Exchange);
            }

            var granularity = AlgoHelper.ConvertTimeframeToCranularity(container.TimeframeInterval, container.TimeframePeriod);
            var currentPriceData = await _historyService.GetHistory(container.Symbol, granularity, container.Datafeed, container.Exchange);
            var dailyPriceData = await _historyService.GetHistory(container.Symbol, DAILYG_RANULARITY, container.Datafeed, container.Exchange);
            throw new NotImplementedException();
        }
    }
}
