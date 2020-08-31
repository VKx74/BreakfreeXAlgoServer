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
        private const int HOURLY_GRANULARITY = 3600;
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
            // if (container.Datafeed != "twelvedata" && container.Datafeed != "oanda")
            // {
            //     throw new ApiException(HttpStatusCode.BadRequest,
            //         $"Unsupported '{container.Datafeed}' datafeed. Available 'twelvedata' or 'oanda' only.");
            // }

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
            HistoryData dailyPriceData = null;

            if (granularity == DAILY_GRANULARITY)
            {
                dailyPriceData = new HistoryData
                {
                    Datafeed = currentPriceData.Datafeed,
                    Exchange = currentPriceData.Exchange,
                    Granularity = currentPriceData.Granularity,
                    Symbol = currentPriceData.Symbol,
                    Bars = currentPriceData.Bars.ToList(),
                };
            }
            else
            {
                dailyPriceData = await _historyService.GetHistory(container.Symbol, DAILY_GRANULARITY, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            }

            container.InsertHistory(currentPriceData.Bars, null, dailyPriceData.Bars, container.ReplayBack);

            return container;
        }

        internal Task<BacktestResponse> BacktestAsync(BacktestRequest req)
        {
            return Task.Run(async () =>
            {
                return await backtestAsync(req);
            });
        }
        internal Task<BacktestV2Response> Strategy2BacktestAsync(Strategy2BacktestRequest req)
        {
            return Task.Run(async () =>
            {
                return await strategy2BacktestAsync(req);
            });
        }

        internal Task<ExtHitTestResponse> HitTestExtensionsAsync(HittestRequest req)
        {
            return Task.Run(async () =>
            {
                return await hitTestExtensionsAsync(req);
            });
        }

        public async Task<CalculationResponse> CalculateAsync(CalculationRequest req)
        {
            var container = await InitAsync(req);
            var levels = TechCalculations.CalculateLevels(container.High, container.Low);
            var sar = SupportAndResistance.Calculate(levels, container.Mintick);
            var trend = TrendDetector.CalculateByHma(container.CloseD);
            var calculationData = new TradeEntryCalculationData
            {
                container = container,
                hma_period = 200,
                levels = levels,
                randomize = true,
                sar = sar,
                trend = trend
            };

            var trade = TradeEntry.Calculate(calculationData);
            var result = this.toResponse(levels, sar, trade);
            return result;
        }

        private async Task<BacktestResponse> backtestAsync(BacktestRequest req)
        {
            var container = InputDataContainer.MapCalculationRequestToInputDataContainer(req);
            // if (container.Datafeed != "twelvedata" && container.Datafeed != "oanda")
            // {
            //     throw new ApiException(HttpStatusCode.BadRequest,
            //         $"Unsupported '{container.Datafeed}' datafeed. Available 'twelvedata' or 'oanda' only.");
            // }

            // no need USD rate for this test
            container.setUsdRatio(1);

            var granularity = AlgoHelper.ConvertTimeframeToCranularity(container.TimeframeInterval, container.TimeframePeriod);
            var currentPriceData = await _historyService.GetHistory(container.Symbol, granularity, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            HistoryData dailyPriceData = null;

            if (granularity == DAILY_GRANULARITY)
            {
                dailyPriceData = new HistoryData
                {
                    Datafeed = currentPriceData.Datafeed,
                    Exchange = currentPriceData.Exchange,
                    Granularity = currentPriceData.Granularity,
                    Symbol = currentPriceData.Symbol,
                    Bars = currentPriceData.Bars.ToList(),
                };
            }
            else
            {
                dailyPriceData = await _historyService.GetHistory(container.Symbol, DAILY_GRANULARITY, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            }

            var replayBack = container.ReplayBack;
            var response = new BacktestResponse();
            response.signals = new List<BacktestSignal>();
            TradeEntryResult lastSignal = null;

            var availableBarsCount = currentPriceData.Bars.Count();
            if (replayBack > availableBarsCount - InputDataContainer.MIN_BARS_COUNT)
            {
                replayBack = availableBarsCount - InputDataContainer.MIN_BARS_COUNT;
            }

            while (replayBack >= 0)
            {
                container.AddNext(currentPriceData.Bars, null, dailyPriceData.Bars, replayBack);
                replayBack--;

                var levels = TechCalculations.CalculateLevels(container.High, container.Low);
                var sar = SupportAndResistance.Calculate(levels, container.Mintick);
                var trend = Trend.Undefined;
                var hma_period = req.hma_period.GetValueOrDefault(200);

                if (req.trend_detector == TrendDetectorType.hma)
                {
                    trend = TrendDetector.CalculateByHma(container.CloseD, hma_period);
                }
                else
                {
                    var mesa_fast = req.mesa_fast.GetValueOrDefault(0.5m);
                    var mesa_slow = req.mesa_slow.GetValueOrDefault(0.05m);
                    var mesa_diff = req.mesa_diff.GetValueOrDefault(0.00001m);
                    trend = TrendDetector.CalculateByMesa(container.CloseD, mesa_diff, mesa_fast, mesa_slow);
                }

                var calculationData = new TradeEntryCalculationData
                {
                    container = container,
                    hma_period = hma_period,
                    levels = levels,
                    randomize = false,
                    sar = sar,
                    trend = trend
                };
                var trade = TradeEntry.Calculate(calculationData);
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
                    if (lastBacktestSignal != null && lastBacktestSignal.end_timestamp == 0)
                    {
                        lastBacktestSignal.end_timestamp = container.Time.LastOrDefault();
                    }

                    var result = this.toResponse(levels, sar, trade);
                    response.signals.Add(new BacktestSignal
                    {
                        data = result,
                        timestamp = container.Time.LastOrDefault()
                    });
                }
                else
                {
                    lastSignal = null;
                    var lastBacktestSignal = response.signals.LastOrDefault();
                    if (lastBacktestSignal != null && lastBacktestSignal.end_timestamp == 0)
                    {
                        lastBacktestSignal.end_timestamp = container.Time.LastOrDefault();
                    }
                }
            }

            var signalProcessor = new SignalsProcessor(currentPriceData.Bars, response.signals, req.breakeven_candles);
            var orders = signalProcessor.Backtest(container.InputSplitPositions);
            response.orders = orders;

            return response;
        }

        private async Task<ExtHitTestResponse> hitTestExtensionsAsync(HittestRequest req)
        {
            var container = InputDataContainer.MapCalculationRequestToInputDataContainer(req);
            // if (container.Datafeed != "twelvedata" && container.Datafeed != "oanda")
            // {
            //     throw new ApiException(HttpStatusCode.BadRequest,
            //         $"Unsupported '{container.Datafeed}' datafeed. Available 'twelvedata' or 'oanda' only.");
            // }

            // no need USD rate for this test
            container.setUsdRatio(1);

            var granularity = AlgoHelper.ConvertTimeframeToCranularity(container.TimeframeInterval, container.TimeframePeriod);
            var currentPriceData = await _historyService.GetHistory(container.Symbol, granularity, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            HistoryData dailyPriceData = null;

            if (granularity == DAILY_GRANULARITY)
            {
                dailyPriceData = new HistoryData
                {
                    Datafeed = currentPriceData.Datafeed,
                    Exchange = currentPriceData.Exchange,
                    Granularity = currentPriceData.Granularity,
                    Symbol = currentPriceData.Symbol,
                    Bars = currentPriceData.Bars.ToList(),
                };
            }
            else
            {
                dailyPriceData = await _historyService.GetHistory(container.Symbol, DAILY_GRANULARITY, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            }

            var replayBack = container.ReplayBack;
            var response = new ExtHitTestResponse();
            response.signals = new List<ExtHitTestSignal>();
            Levels lastLevels = null;
            Trend? trendData = null;
            decimal prevDailyClose = 0;
            var hma_period = req.hma_period.GetValueOrDefault(200);
            var mesa_fast = req.mesa_fast.GetValueOrDefault(0.5m);
            var mesa_slow = req.mesa_slow.GetValueOrDefault(0.05m);
            var mesa_diff = req.mesa_diff.GetValueOrDefault(0.00001m);

            var availableBarsCount = currentPriceData.Bars.Count();
            if (replayBack > availableBarsCount - InputDataContainer.MIN_BARS_COUNT)
            {
                replayBack = availableBarsCount - InputDataContainer.MIN_BARS_COUNT;
            }

            while (replayBack >= 0)
            {
                // no need daily bars for this test
                container.AddNext(currentPriceData.Bars, null, dailyPriceData.Bars, replayBack);
                replayBack--;

                var levels = TechCalculations.CalculateLevels(container.High, container.Low);
                var sar = SupportAndResistance.Calculate(levels, container.Mintick);
                var dailyClose = container.CloseD.LastOrDefault();

                if (prevDailyClose != dailyClose || trendData == null)
                {
                    if (req.trend_detector == TrendDetectorType.hma)
                    {
                        trendData = TrendDetector.CalculateByHma(container.CloseD, hma_period);
                    }
                    else
                    {
                        trendData = TrendDetector.CalculateByMesa(container.CloseD, mesa_diff, mesa_fast, mesa_slow);
                    }
                }

                prevDailyClose = dailyClose;

                ExtHitTestSignal lastSignal;

                if (lastLevels != null)
                {
                    if (LookBackResult.IsEquals(levels.Level128, lastLevels.Level128))
                    {
                        lastSignal = response.signals.LastOrDefault();
                        if (lastSignal != null && lastSignal.trend == trendData.Value)
                        {
                            continue;
                        }
                    }
                }

                lastSignal = response.signals.LastOrDefault();
                if (lastSignal != null && lastSignal.end_timestamp == 0)
                {
                    lastSignal.end_timestamp = container.Time.LastOrDefault();
                }

                lastLevels = levels;
                var result = this.toExtHitTestResponse(levels, sar);
                response.signals.Add(new ExtHitTestSignal
                {
                    trend = trendData.Value,
                    data = result,
                    timestamp = container.Time.LastOrDefault()
                });
            }

            var history = currentPriceData.Bars.Count() > container.ReplayBack + 1 ? currentPriceData.Bars.TakeLast(container.ReplayBack + 1) : currentPriceData.Bars;
            var extHitTestProcessor = new ExtHitTestProcessor(history, response.signals, req.breakeven_candles, req.entry_target_box, req.stoploss_rr);
            // processing passsed in constructor signals
            extHitTestProcessor.HitTest();

            return response;
        }

        private async Task<BacktestV2Response> strategy2BacktestAsync(Strategy2BacktestRequest req)
        {
            var container = InputDataContainer.MapCalculationRequestToInputDataContainer(req);
            // no need USD rate for this test
            container.setUsdRatio(1);

            var granularity = AlgoHelper.ConvertTimeframeToCranularity(container.TimeframeInterval, container.TimeframePeriod);
            var currentPriceData = await _historyService.GetHistory(container.Symbol, granularity, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            HistoryData dailyPriceData = null;

            if (granularity >= DAILY_GRANULARITY)
            {
                dailyPriceData = new HistoryData
                {
                    Datafeed = currentPriceData.Datafeed,
                    Exchange = currentPriceData.Exchange,
                    Granularity = currentPriceData.Granularity,
                    Symbol = currentPriceData.Symbol,
                    Bars = currentPriceData.Bars.ToList(),
                };
            }
            else
            {
                dailyPriceData = await _historyService.GetHistory(container.Symbol, DAILY_GRANULARITY, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            }
             
             HistoryData hourlyPriceData = null;

            if (granularity >= HOURLY_GRANULARITY)
            {
                hourlyPriceData = new HistoryData
                {
                    Datafeed = currentPriceData.Datafeed,
                    Exchange = currentPriceData.Exchange,
                    Granularity = currentPriceData.Granularity,
                    Symbol = currentPriceData.Symbol,
                    Bars = currentPriceData.Bars.ToList(),
                };
            }
            else
            {
                hourlyPriceData = await _historyService.GetHistory(container.Symbol, HOURLY_GRANULARITY, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            }

            var replayBack = container.ReplayBack;
            var response = new BacktestV2Response();
            response.signals = new List<Strategy2BacktestSignal>();
            Trend? hourlyTrend = null;
            Trend? dailyTrend = null;
            decimal prevHourlyClose = 0;
            decimal prevDailyClose = 0;
            var hma_period = req.hma_period.GetValueOrDefault(200);
            var mesa_fast = req.mesa_fast.GetValueOrDefault(0.25m);
            var mesa_slow = req.mesa_slow.GetValueOrDefault(0.05m);
            var mesa_diff = req.mesa_diff.GetValueOrDefault(0.1m);

            var availableBarsCount = currentPriceData.Bars.Count();
            if (replayBack > availableBarsCount - InputDataContainer.MIN_BARS_COUNT)
            {
                replayBack = availableBarsCount - InputDataContainer.MIN_BARS_COUNT;
            }

            Levels lastLevels = null;
            while (replayBack >= 0)
            {
                // no need daily bars for this test
                container.AddNext(currentPriceData.Bars, hourlyPriceData.Bars, dailyPriceData.Bars, replayBack);
                replayBack--;

                var levels = TechCalculations.CalculateLevels(container.High, container.Low);
                var sar = SupportAndResistance.Calculate(levels, container.Mintick);
                var dailyClose = container.CloseD.LastOrDefault();
                var hourlyClose = container.CloseH.LastOrDefault();

                if (prevDailyClose != dailyClose || dailyTrend == null)
                {
                    if (req.trend_detector == TrendDetectorType.hma)
                    {
                        dailyTrend = TrendDetector.CalculateByHma(container.CloseD, hma_period);
                    }
                    else
                    {
                        dailyTrend = TrendDetector.CalculateByMesa(container.CloseD, mesa_diff, mesa_fast, mesa_slow);
                    }
                }

                prevDailyClose = dailyClose; 
                
                if (prevHourlyClose != hourlyClose || hourlyTrend == null)
                {
                    if (req.trend_detector == TrendDetectorType.hma)
                    {
                        hourlyTrend = TrendDetector.CalculateByHma(container.CloseH, hma_period);
                    }
                    else
                    {
                        hourlyTrend = TrendDetector.CalculateByMesa(container.CloseH, mesa_diff, mesa_fast, mesa_slow);
                    }
                }

                prevHourlyClose = hourlyClose;

                var calculationData = new TradeEntryV2CalculationData
                {
                    container = container,
                    levels = levels,
                    randomize = false,
                    sar = sar,
                    hourlyTrend = hourlyTrend.GetValueOrDefault(Trend.Undefined),
                    dailyTrend = dailyTrend.GetValueOrDefault(Trend.Undefined),
                    riskRewords = req.risk_rewards
                };

                if (calculationData.isTrendValid())
                {
                    if (lastLevels != null && Levels.IsEquals(lastLevels, levels))
                    {
                        lastLevels = levels;
                        continue;
                    }
                    lastLevels = levels;

                    var lastBacktestSignal = response.signals.LastOrDefault();
                    if (lastBacktestSignal != null && lastBacktestSignal.end_timestamp == 0)
                    {
                        lastBacktestSignal.end_timestamp = container.Time.LastOrDefault();
                    }

                    var tradeSR = req.place_on_sr ? TradeEntryV2.CalculateSREntry(calculationData, req.stoploss_rr) : null;
                    var tradeEx1 = req.place_on_ex1 ? TradeEntryV2.CalculateEx1Entry(calculationData, req.stoploss_rr) : null;
                    var result = this.toStrategyV2Response(calculationData, tradeSR, tradeEx1);
                    response.signals.Add(new Strategy2BacktestSignal
                    {
                        data = result,
                        timestamp = container.Time.LastOrDefault()
                    });
                }
                else
                {
                    lastLevels = null;
                    var lastBacktestSignal = response.signals.LastOrDefault();
                    if (lastBacktestSignal != null && lastBacktestSignal.end_timestamp == 0)
                    {
                        lastBacktestSignal.end_timestamp = container.Time.LastOrDefault();
                    }
                }
            }

            var signalProcessor = new SignalsV2Processor(currentPriceData.Bars, response.signals, req.breakeven_candles);
            var orders = signalProcessor.Backtest();
            response.orders = orders;

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

        private Strategy2CalculationResponse toStrategyV2Response(TradeEntryV2CalculationData calculationData, TradeEntryV2Result tradeSR, TradeEntryV2Result tradeEx1)
        {
            var top_ext2 = calculationData.sar.Plus28;
            var top_ext1 = calculationData.sar.Plus18;
            var resistance = calculationData.levels.Level128.EightEight;
            var natural = calculationData.levels.Level128.FourEight;
            var support = calculationData.levels.Level128.ZeroEight;
            var bottom_ext1 = calculationData.sar.Minus18;
            var bottom_ext2 = calculationData.sar.Minus28;

            return new Strategy2CalculationResponse {
                top_ex2 = top_ext2,
                top_ex1 = top_ext1,
                r = resistance,
                s = support,
                n = natural,
                bottom_ex1 = bottom_ext1,
                bottom_ex2 = bottom_ext2,
                daily_trend = calculationData.dailyTrend,
                hourly_trend = calculationData.hourlyTrend,
                trade_sr = tradeSR,
                trade_ex1 = tradeEx1
            };
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
