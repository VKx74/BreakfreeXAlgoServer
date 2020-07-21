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

        public async Task<InputDataContainer> InitAsync(CalculationRequest req) {
            var container = InputDataContainer.MapCalculationRequestToInputDataContainer(req);
            if (container.Datafeed != "twelvedata" && container.Datafeed != "oanda")
            {
                throw new ApiException(HttpStatusCode.BadRequest,
                    $"Unsupported '{container.Datafeed}' datafeed. Available 'twelvedata' or 'oanda' only.");
            }

            if (container.Type == "forex") {
                var usdRatio = await _priceRatioCalculationService.GetUSDRatio(container.Symbol, container.Datafeed, container.Exchange);
                container.setUsdRatio(usdRatio);
            } else {
                container.setUsdRatio(1);
            }

            var granularity = AlgoHelper.ConvertTimeframeToCranularity(container.TimeframeInterval, container.TimeframePeriod);
            var currentPriceData = await _historyService.GetHistory(container.Symbol, granularity, container.Datafeed, container.Exchange, container.ReplayBack);
            var dailyPriceData = await _historyService.GetHistory(container.Symbol, DAILYG_RANULARITY, container.Datafeed, container.Exchange, container.ReplayBack);
            container.InsertHistory(currentPriceData.Bars, dailyPriceData.Bars, container.ReplayBack);
            return container;
        }
        public async Task<CalculationResponse> CalculateAsync(CalculationRequest req)
        {
            var container = await InitAsync(req);
            var levels = TechCalculations.CalculateLevels(container.High, container.Low);
            var sar = SupportAndResistance.Calculate(levels, container.Mintick);
            var trade = TradeEntry.Calculate(container, levels, sar);

            var algoInfo = new AlgoInfo {
                Macrotrend = trade.algo_Info.macrotrend,
                NCurrencySymbol = trade.algo_Info.n_currencySymbol,
                Objective = trade.algo_Info.objective,
                Pas = trade.algo_Info.pas,
                Positionsize = trade.algo_Info.positionsize,
                Status = trade.algo_Info.status,
                Suggestedrisk = trade.algo_Info.suggestedrisk
            };

            var returnData = new CalculationResponse {
                 // xmode
                EE = levels.Level128.EightEight,
                EE1 = levels.Level32.EightEight,
                EE2 = levels.Level16.EightEight,
                EE3 = levels.Level8.EightEight,
                FE = levels.Level128.FourEight,
                FE1 = levels.Level32.FourEight,
                FE2 = levels.Level16.FourEight,
                FE3 = levels.Level8.FourEight,
                ZE = levels.Level128.ZeroEight,
                ZE1 = levels.Level32.ZeroEight,
                ZE2 = levels.Level16.ZeroEight,
                ZE3 = levels.Level8.ZeroEight,
                VR100 = sar.ValidRes100,
                VR75A = sar.ValidRes75a,
                VR75B = sar.ValidRes75b,
                VN100 = sar.ValidNeu100,
                VN75A = sar.ValidNeu75a,
                VN75B = sar.ValidNeu75b,
                VS100 = sar.ValidSup100,
                VS75A = sar.ValidSup75a,
                VS75B = sar.ValidSup75b,
                VSCS = sar.Validscs,
                VSCS2 = sar.Validscs2,
                VEXTTP = sar.Validexttp,
                VEXTTP2 = sar.Validexttp2,
                M18 = sar.Minus18,
                M28 = sar.Minus28,
                P18 = sar.Plus18,
                P28 = sar.Plus28,
                // algo
                AlgoTP2 = trade.algo_TP2,
                AlgoTP1High = trade.algo_TP1_high,
                AlgoTP1Low = trade.algo_TP1_low,
                AlgoEntryHigh = trade.algo_Entry_high,
                AlgoEntryLow = trade.algo_Entry_low,
                AlgoEntry = trade.algo_Entry,
                AlgoStop = trade.algo_Stop,
                AlgoRisk = trade.algo_Risk,
                AlgoInfo = algoInfo,
                Id = container.Id,
                Clean = true
            };
            return returnData;
        }
    }
}
