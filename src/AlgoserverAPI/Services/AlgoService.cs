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
        private const int DAILY_GRANULARITY = 86400;

        private readonly ILogger<AlgoService> _logger;
        private readonly HistoryService _historyService;
        private readonly PriceRatioCalculationService _priceRatioCalculationService;
        
        public AlgoService(ILogger<AlgoService> logger, HistoryService historyService, PriceRatioCalculationService priceRatioCalculationService)
        {
            _logger = logger;
            _historyService = historyService;
            _priceRatioCalculationService = priceRatioCalculationService;
        }

        public async Task<InputDataContainer> InitAsync(CalculationRequest req)
        {
            var container = InputDataContainer.MapCalculationRequestToInputDataContainer(req);
            if (container.Datafeed != "twelvedata" && container.Datafeed != "oanda")
            {
                throw new ApiException(HttpStatusCode.BadRequest,
                    $"Unsupported '{container.Datafeed}' datafeed. Available 'twelvedata' or 'oanda' only.");
            }

            if (container.Type == "forex")
            {
                var usdRatio = await _priceRatioCalculationService.GetUSDRatio(container.Symbol, container.Datafeed, container.Type, container.Exchange);
                container.setUsdRatio(usdRatio);
            }
            else
            {
                container.setUsdRatio(1);
            }

            var granularity = AlgoHelper.ConvertTimeframeToCranularity(container.TimeframeInterval, container.TimeframePeriod);
            var currentPriceData = await _historyService.GetHistory(container.Symbol, granularity, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            var dailyPriceData = await _historyService.GetHistory(container.Symbol, DAILY_GRANULARITY, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            container.InsertHistory(currentPriceData.Bars, dailyPriceData.Bars, container.ReplayBack);
            
            return container;
        }

        internal Task<BacktestResponse> BacktestAsync(BacktestRequest req) {
            return Task.Run(async () => {
                return await backtestAsync(req);
            });
        }
        
        internal Task<ExtHitTestResponse> HitTestExtensionsAsync(BacktestRequest req) {
            return Task.Run(async () => {
                return await hitTestExtensionsAsync(req);
            });
        }

        public async Task<CalculationResponse> CalculateAsync(CalculationRequest req)
        {
            var container = await InitAsync(req);
            var levels = TechCalculations.CalculateLevels(container.High, container.Low);
            var sar = SupportAndResistance.Calculate(levels, container.Mintick);
            var trade = TradeEntry.Calculate(container, levels, sar, 200);
            var result = this.toResponse(levels, sar, trade);
            return result;
        }

        private async Task<BacktestResponse> backtestAsync(BacktestRequest req)
        {
            var container = InputDataContainer.MapCalculationRequestToInputDataContainer(req);
            if (container.Datafeed != "twelvedata" && container.Datafeed != "oanda")
            {
                throw new ApiException(HttpStatusCode.BadRequest,
                    $"Unsupported '{container.Datafeed}' datafeed. Available 'twelvedata' or 'oanda' only.");
            }

            // no need USD rate for this test
            container.setUsdRatio(1);

            var granularity = AlgoHelper.ConvertTimeframeToCranularity(container.TimeframeInterval, container.TimeframePeriod);
            var currentPriceData = await _historyService.GetHistory(container.Symbol, granularity, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            var dailyPriceData = await _historyService.GetHistory(container.Symbol, DAILY_GRANULARITY, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);

            var replayBack = container.ReplayBack;
            var response = new BacktestResponse();
            response.signals = new List<BacktestSignal>();
            TradeEntryResult lastSignal = null;

            while (replayBack >= 0)
            {
                container.InsertHistory(currentPriceData.Bars, dailyPriceData.Bars, replayBack);
                replayBack--;

                var levels = TechCalculations.CalculateLevels(container.High, container.Low);
                var sar = SupportAndResistance.Calculate(levels, container.Mintick);
                var trade = TradeEntry.Calculate(container, levels, sar, req.hma_period, false);
                if (trade.algo_Entry != Decimal.Zero)
                {
                    if (lastSignal != null)
                    {
                        if (TradeEntryResult.IsEquals(lastSignal, trade))
                        {
                            continue;
                        }
                    }

                    lastSignal = trade;

                    var lastBacktestSignal = response.signals.LastOrDefault();
                    if (lastBacktestSignal != null && lastBacktestSignal.end_timestamp == 0) {
                        lastBacktestSignal.end_timestamp = container.Time.LastOrDefault();
                    }

                    var result = this.toResponse(levels, sar, trade);
                    response.signals.Add(new BacktestSignal
                    {
                        data = result,
                        timestamp = container.Time.LastOrDefault()
                    });
                } else {
                    lastSignal = null;
                    var lastBacktestSignal = response.signals.LastOrDefault();
                    if (lastBacktestSignal != null && lastBacktestSignal.end_timestamp == 0) {
                        lastBacktestSignal.end_timestamp = container.Time.LastOrDefault();
                    }
                }
            }

            var signalProcessor = new SignalsProcessor(currentPriceData.Bars, response.signals);
            var orders = signalProcessor.Backtest(container.InputSplitPositions);
            response.orders = orders;

            return response;
        }
        
        private async Task<ExtHitTestResponse> hitTestExtensionsAsync(BacktestRequest req)
        {
            var container = InputDataContainer.MapCalculationRequestToInputDataContainer(req);
            if (container.Datafeed != "twelvedata" && container.Datafeed != "oanda")
            {
                throw new ApiException(HttpStatusCode.BadRequest,
                    $"Unsupported '{container.Datafeed}' datafeed. Available 'twelvedata' or 'oanda' only.");
            }

            // no need USD rate for this test
            container.setUsdRatio(1);

            var granularity = AlgoHelper.ConvertTimeframeToCranularity(container.TimeframeInterval, container.TimeframePeriod);
            var currentPriceData = await _historyService.GetHistory(container.Symbol, granularity, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            var dailyPriceData = await _historyService.GetHistory(container.Symbol, DAILY_GRANULARITY, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);

            var replayBack = container.ReplayBack;
            var response = new ExtHitTestResponse();
            response.signals = new List<ExtHitTestSignal>();
            Levels lastLevels = null;
            TrendResponse trendData = null;
            decimal prevDailyClose = 0;

            while (replayBack >= 0)
            {
                // no need daily bars for this test
                container.InsertHistory(currentPriceData.Bars, dailyPriceData.Bars, replayBack);
                replayBack--;

                var levels = TechCalculations.CalculateLevels(container.High, container.Low);
                var sar = SupportAndResistance.Calculate(levels, container.Mintick);
                var dailyClose = container.CloseD.LastOrDefault();
                
                if (prevDailyClose != dailyClose || trendData == null) {
                    trendData = TrendDetector.Calculate(container.CloseD, req.hma_period);
                }

                prevDailyClose = dailyClose;

                ExtHitTestSignal lastSignal;

                if (lastLevels != null)
                {
                    if (LookBackResult.IsEquals(levels.Level128, lastLevels.Level128))
                    {
                        lastSignal = response.signals.LastOrDefault();
                        if (lastSignal != null && lastSignal.end_timestamp == 0) {
                            if (lastSignal.is_up_tending != trendData.isUpTrending) {
                                lastSignal.end_timestamp = container.Time.LastOrDefault();
                            }
                        }
                        continue;
                    }
                }

                lastSignal = response.signals.LastOrDefault();
                if (lastSignal != null && lastSignal.end_timestamp == 0) {
                    lastSignal.end_timestamp = container.Time.LastOrDefault();
                }

                lastLevels = levels;
                var result = this.toExtHitTestResponse(levels, sar);
                response.signals.Add(new ExtHitTestSignal
                {
                    is_up_tending = trendData.isUpTrending,
                    data = result,
                    timestamp = container.Time.LastOrDefault()
                });
            }

            var extHitTestProcessor = new ExtHitTestProcessor(currentPriceData.Bars.Count() > container.ReplayBack + 1 ? currentPriceData.Bars.TakeLast(container.ReplayBack + 1) : currentPriceData.Bars, response.signals);
            // processing passsed in constructor signals
            extHitTestProcessor.HitTest();

            return response;
        }

        private CalculationResponse toExtHitTestResponse(Levels levels, SupportAndResistanceResult sar)
        {
            var returnData = new CalculationResponse
            {
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
                Clean = true
            };

            return returnData;
        }

        private CalculationResponse toResponse(Levels levels, SupportAndResistanceResult sar, TradeEntryResult trade)
        {
            var algoInfo = new AlgoInfo
            {
                Macrotrend = trade.algo_Info.macrotrend,
                NCurrencySymbol = trade.algo_Info.n_currencySymbol,
                Objective = trade.algo_Info.objective,
                Pas = trade.algo_Info.pas,
                Positionsize = trade.algo_Info.positionsize,
                Status = trade.algo_Info.status,
                Suggestedrisk = trade.algo_Info.suggestedrisk
            };

            var returnData = new CalculationResponse
            {
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
                Clean = true
            };

            return returnData;
        }
    }
}
