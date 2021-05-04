﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.Algo;
using Algoserver.API.Models.REST;
using Algoserver.API.Services.CacheServices;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.Services
{
    public class AlgoService
    {

        private readonly ILogger<AlgoService> _logger;
        private readonly HistoryService _historyService;
        private readonly ScannerService _scanner;
        private readonly ICacheService _cache;
        private string _cachePrefix = "MarketInfo_";
        private string _cachePrefixV2 = "MarketInfoV2_";
        private readonly PriceRatioCalculationService _priceRatioCalculationService;

        public AlgoService(ILogger<AlgoService> logger, HistoryService historyService, PriceRatioCalculationService priceRatioCalculationService, ScannerService scanner, ICacheService cache)
        {
            _logger = logger;
            _historyService = historyService;
            _scanner = scanner;
            _cache = cache;
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
                var usdRatio = await _priceRatioCalculationService.GetSymbolRatio(container.Symbol, req.AccountCurrency, container.Datafeed, container.Type, container.Exchange);
                container.setUsdRatio(usdRatio);
            }
            else
            {
                container.setUsdRatio(1);
            }

            var granularity = AlgoHelper.ConvertTimeframeToCranularity(container.TimeframeInterval, container.TimeframePeriod);
            var currentPriceData = await _historyService.GetHistory(container.Symbol, granularity, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            HistoryData dailyPriceData = null;

            if (granularity == TimeframeHelper.DAILY_GRANULARITY)
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
                dailyPriceData = await _historyService.GetHistory(container.Symbol, TimeframeHelper.DAILY_GRANULARITY, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            }

            container.InsertHistory(currentPriceData.Bars, null, dailyPriceData.Bars, container.ReplayBack);

            return container;
        }

        internal Task<CalculatePositionSizeResponse> CalculatePositionSize(CalculatePositionSizeRequest req)
        {
            return Task.Run(async () =>
            {
                return await calculatePositionSize(req);
            });
        }

        internal Task<CalculatePriceRatioResponse> CalculatePriceRatio(CalculatePriceRatioRequest req)
        {
            return Task.Run(async () =>
            {
                return await calculatePriceRatio(req);
            });
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

        internal Task<List<BacktestExtensionsResponse>> BacktestExtensionsAsync(BacktestExtensions req)
        {
            return Task.Run(async () =>
            {
                return await backtestExtensionsAsync(req);
            });
        }

        public async Task<CalculationResponse> CalculateAsync(CalculationRequest req)
        {
            var container = await InitAsync(req);
            var levels = TechCalculations.CalculateLevels(container.High, container.Low);
            var sar = SupportAndResistance.Calculate(levels, container.Mintick);
            var trend = TrendDetector.CalculateByMesa(container.CloseD);
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

        public Task<CalculationMarketInfoResponse> CalculateMarketInfoAsync(Instrument instrument)
        {
            return Task.Run(() =>
            {
                return calculateMarketInfoAsync(instrument);
            });
        }

        public Task<CalculationMarketInfoResponse> CalculateMarketInfoV2Async(MarketInfoCalculationRequest request)
        {
            return Task.Run(() =>
            {
                return calculateMarketInfoV2Async(request);
            });
        }

        private async Task<CalculationMarketInfoResponse> calculateMarketInfoAsync(Instrument instrument)
        {
            var cachedResponse = tryGetCalculateMarketInfoFromCache(instrument);

            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var data = await _historyService.GetHistory(instrument.Id, TimeframeHelper.DAILY_GRANULARITY, instrument.Datafeed, instrument.Exchange, instrument.Type);
            var high = data.Bars.Select(_ => _.High);
            var low = data.Bars.Select(_ => _.Low);
            var close = data.Bars.Select(_ => _.Close).ToList();

            var levels = TechCalculations.CalculateLevel128(high, low);
            var trend = TrendDetector.CalculateByMesaBy2TrendAdjusted(close);

            var topExt = levels.Plus18;
            var natural = levels.FourEight;
            var bottomExt = levels.Minus18;
            var support = levels.ZeroEight;
            var resistance = levels.EightEight;
            var result = new CalculationMarketInfoResponse
            {
                global_trend = trend.GlobalTrend,
                local_trend = trend.LocalTrend,
                is_overhit = trend.IsOverhit,
                natural = natural,
                resistance = resistance,
                support = support,
                daily_natural = natural,
                daily_resistance = resistance,
                daily_support = support,
                last_price = close.LastOrDefault(),
                global_trend_spread = trend.GlobalTrendSpread,
                local_trend_spread = trend.LocalTrendSpread
            };

            tryAddCalculateMarketInfoInCache(instrument, result);

            return result;
        }

        private async Task<CalculationMarketInfoResponse> calculateMarketInfoV2Async(MarketInfoCalculationRequest request)
        {
            var instrument = request.Instrument;
            var timeframe = request.Granularity.GetValueOrDefault(TimeframeHelper.DAILY_GRANULARITY);
            var cachedResponse = tryGetCalculateMarketInfoV2FromCache(instrument, timeframe);

            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var dataDaily = await _historyService.GetHistory(instrument.Id, TimeframeHelper.DAILY_GRANULARITY, instrument.Datafeed, instrument.Exchange, instrument.Type);
            var highDaily = dataDaily.Bars.Select(_ => _.High);
            var lowDaily = dataDaily.Bars.Select(_ => _.Low);
            var closeDaily = dataDaily.Bars.Select(_ => _.Close).ToList();

            var levelsDaily = TechCalculations.CalculateLevel128(highDaily, lowDaily);
            var exactTFLevels = levelsDaily;

            if (timeframe != TimeframeHelper.DAILY_GRANULARITY)
            {
                var dataTF = await _historyService.GetHistory(instrument.Id, timeframe, instrument.Datafeed, instrument.Exchange, instrument.Type);
                var highFT = dataTF.Bars.Select(_ => _.High);
                var lowFT = dataTF.Bars.Select(_ => _.Low);
                exactTFLevels = TechCalculations.CalculateLevel128(highFT, lowFT);
            }


            var topDailyExt = levelsDaily.Plus18;
            var naturalDaily = levelsDaily.FourEight;
            var bottomDailyExt = levelsDaily.Minus18;
            var supportDaily = levelsDaily.ZeroEight;
            var resistanceDaily = levelsDaily.EightEight;

            var topTFExt = exactTFLevels.Plus18;
            var naturalTF = exactTFLevels.FourEight;
            var bottomTFExt = exactTFLevels.Minus18;
            var supportTF = exactTFLevels.ZeroEight;
            var resistanceTF = exactTFLevels.EightEight;

            var trend = TrendDetector.CalculateByMesaBy2TrendAdjusted(closeDaily);

            var result = new CalculationMarketInfoResponse
            {
                global_trend = trend.GlobalTrend,
                local_trend = trend.LocalTrend,
                is_overhit = trend.IsOverhit,
                daily_natural = naturalDaily,
                daily_resistance = resistanceDaily,
                daily_support = supportDaily,
                natural = naturalTF,
                resistance = resistanceTF,
                support = supportTF,
                last_price = closeDaily.LastOrDefault(),
                global_trend_spread = trend.GlobalTrendSpread,
                local_trend_spread = trend.LocalTrendSpread
            };

            tryAddCalculateMarketInfoV2InCache(instrument, timeframe, result);

            return result;
        }

        internal Task<CalculationResponseV2> CalculateV2Async(CalculationRequest req)
        {
            return Task.Run(() =>
            {
                return calculateV2Async(req);
            });
        }

        private async Task<CalculationResponseV2> calculateV2Async(CalculationRequest req)
        {
            var container = await InitAsync(req);
            var levels = TechCalculations.CalculateLevels(container.High, container.Low);
            var sar = SupportAndResistance.Calculate(levels, container.Mintick);
            var scanningHistory = new ScanningHistory
            {
                Open = container.Open,
                High = container.High,
                Low = container.Low,
                Close = container.Close,
            };

            var dailyScanningHistory = new ScanningHistory
            {
                Open = container.OpenD,
                High = container.HighD,
                Low = container.LowD,
                Close = container.CloseD,
            };

            var isForex = container.Type == "forex";
            var symbol = container.Symbol;
            var accountSize = container.InputAccountSize * container.UsdRatio;
            var suggestedRisk = container.InputRisk;
            var sl_ratio = container.InputStoplossRatio;
            var granularity = AlgoHelper.ConvertTimeframeToCranularity(container.TimeframeInterval, container.TimeframePeriod);

            ScanResponse scanRes = null;

            if (granularity >= TimeframeHelper.MIN15_GRANULARITY)
            {
                var extendedTrendData = TrendDetector.CalculateByMesaBy2TrendAdjusted(container.CloseD);
                var trend = TrendDetector.MergeTrends(extendedTrendData);

                if (container.TimeframePeriod != "d" && container.TimeframePeriod != "w")
                {
                    scanRes = _scanner.ScanExt(scanningHistory, dailyScanningHistory, trend, sl_ratio);
                    if (scanRes == null)
                    {
                        scanRes = _scanner.ScanBRC(scanningHistory, trend, sl_ratio);
                    }
                }

                if (scanRes == null)
                {
                    if (container.TimeframePeriod == "d")
                    {
                        scanRes = _scanner.ScanSwingOldStrategy(scanningHistory, sl_ratio);
                    }
                    if (container.TimeframePeriod == "h" && container.TimeframeInterval == 4)
                    {
                        if (!extendedTrendData.IsOverhit)
                        {
                            scanRes = _scanner.ScanSwing(scanningHistory, dailyScanningHistory, extendedTrendData.GlobalTrend, extendedTrendData.LocalTrend, sl_ratio);
                        }
                    }
                }
            }

            var size = 0m;

            if (scanRes != null)
            {
                scanRes.risk = suggestedRisk;
                size = AlgoHelper.CalculatePositionValue(container.Type, symbol, accountSize, suggestedRisk, scanRes.entry, scanRes.stop, container.ContractSize);
            }

            var result = this.toResponseV2(levels, sar, scanRes, size);
            result.id = container.Id;
            return result;
        }

        private async Task<CalculatePositionSizeResponse> calculatePositionSize(CalculatePositionSizeRequest req)
        {
            var type = req.Instrument.Type.ToLowerInvariant();
            var datafeed = req.Instrument.Datafeed.ToLowerInvariant();
            var exchange = req.Instrument.Exchange.ToLowerInvariant();
            var symbol = req.Instrument.Id;
            var suggestedRisk = req.InputRisk;
            var priceDiff = req.PriceDiff;
            var contractSize = req.ContractSize;
            var usdRatio = 1m;

            if (type == "forex")
            {
                usdRatio = await _priceRatioCalculationService.GetSymbolRatio(symbol, req.AccountCurrency, datafeed, type, exchange);
            }

            var accountSize = req.InputAccountSize * usdRatio;

            var size = AlgoHelper.CalculatePositionValue(type, symbol, accountSize, suggestedRisk, priceDiff, contractSize);

            return new CalculatePositionSizeResponse {
                size = size
            };
        }
        private async Task<CalculatePriceRatioResponse> calculatePriceRatio(CalculatePriceRatioRequest req)
        {
            var type = req.Instrument.Type.ToLowerInvariant();
            var datafeed = req.Instrument.Datafeed.ToLowerInvariant();
            var exchange = req.Instrument.Exchange.ToLowerInvariant();
            var symbol = req.Instrument.Id;
            var usdRatio = 1m;

            if (type == "forex")
            {
                usdRatio = await _priceRatioCalculationService.GetSymbolRatio(symbol, req.AccountCurrency, datafeed, type, exchange);
            }

            return new CalculatePriceRatioResponse {
                ratio = usdRatio
            };
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

            if (granularity == TimeframeHelper.DAILY_GRANULARITY)
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
                dailyPriceData = await _historyService.GetHistory(container.Symbol, TimeframeHelper.DAILY_GRANULARITY, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
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
                    var global_fast = req.global_fast.GetValueOrDefault(0.25m);
                    var global_slow = req.global_slow.GetValueOrDefault(0.05m);
                    var local_fast = req.local_fast.GetValueOrDefault(1.2m);
                    var local_slow = req.local_slow.GetValueOrDefault(0.6m);
                    var mesa_diff = req.mesa_diff.GetValueOrDefault(0.1m);
                    trend = TrendDetector.CalculateByMesa(container.CloseD, mesa_diff, global_fast, global_slow, local_fast, local_slow);
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

            if (granularity == TimeframeHelper.DAILY_GRANULARITY)
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
                dailyPriceData = await _historyService.GetHistory(container.Symbol, TimeframeHelper.DAILY_GRANULARITY, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            }

            var replayBack = container.ReplayBack;
            var response = new ExtHitTestResponse();
            response.signals = new List<ExtHitTestSignal>();
            Levels lastLevels = null;
            Trend? trendData = null;
            decimal prevDailyClose = 0;
            var hma_period = req.hma_period.GetValueOrDefault(200);
            var global_fast = req.global_fast.GetValueOrDefault(0.25m);
            var global_slow = req.global_slow.GetValueOrDefault(0.05m);
            var local_fast = req.local_fast.GetValueOrDefault(1.2m);
            var local_slow = req.local_slow.GetValueOrDefault(0.6m);
            var mesa_diff = req.mesa_diff.GetValueOrDefault(0.1m);

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
                        trendData = TrendDetector.CalculateByMesa(container.CloseD, mesa_diff, global_fast, global_slow, local_fast, local_slow);
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

            if (granularity >= TimeframeHelper.DAILY_GRANULARITY)
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
                dailyPriceData = await _historyService.GetHistory(container.Symbol, TimeframeHelper.DAILY_GRANULARITY, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            }

            HistoryData hourlyPriceData = null;

            if (granularity >= TimeframeHelper.HOURLY_GRANULARITY)
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
                hourlyPriceData = await _historyService.GetHistory(container.Symbol, TimeframeHelper.HOURLY_GRANULARITY, container.Datafeed, container.Exchange, container.Type, container.ReplayBack);
            }

            var replayBack = container.ReplayBack;
            var response = new BacktestV2Response();
            response.signals = new List<Strategy2BacktestSignal>();
            Trend? dailyTrend = null;
            decimal prevDailyClose = 0;
            var hma_period = req.hma_period.GetValueOrDefault(200);

            var global_fast = req.global_fast.GetValueOrDefault(0.25m);
            var global_slow = req.global_slow.GetValueOrDefault(0.05m);
            var local_fast = req.local_fast.GetValueOrDefault(1.2m);
            var local_slow = req.local_slow.GetValueOrDefault(0.6m);
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
                        dailyTrend = TrendDetector.CalculateByMesa(container.CloseD, mesa_diff, global_fast, global_slow, local_fast, local_slow);
                    }
                }

                prevDailyClose = dailyClose;
                var calculationData = new TradeEntryV2CalculationData
                {
                    container = container,
                    levels = levels,
                    randomize = false,
                    sar = sar,
                    trend = dailyTrend.GetValueOrDefault(Trend.Undefined),
                    riskRewords = req.risk_rewards
                };

                if (calculationData.trend != Trend.Undefined)
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

        private async Task<List<BacktestExtensionsResponse>> backtestExtensionsAsync(BacktestExtensions req)
        {
            var instrument = req.Instrument;
            var granularity = req.Granularity.GetValueOrDefault(TimeframeHelper.DAILY_GRANULARITY);
            var currentPriceData = await _historyService.GetHistoryByDates(instrument.Datafeed, instrument.Id, granularity, instrument.Exchange, req.From, req.To);
            var container = new InputDataContainer();
            var bars = currentPriceData.Bars.ToList();

            // need at least 128 bars to calculate SAR
            var replayBack = bars.Count - 128;
            var response = new List<BacktestExtensionsResponse>();

            while (replayBack-- >= 0)
            {
                container.AddNext(currentPriceData.Bars, null, null, replayBack);
                var levels = TechCalculations.CalculateLevels(container.High, container.Low);
                var sar = SupportAndResistance.Calculate(levels, container.Mintick);
                var lastLevel = response.LastOrDefault();

                if (lastLevel == null || lastLevel.levels.ZE != levels.Level128.ZeroEight || lastLevel.levels.EE != levels.Level128.EightEight)
                {
                    var responseItem = ToLevels(levels, sar);
                    response.Add(new BacktestExtensionsResponse {
                        levels = responseItem,
                        timestamp = container.Time.LastOrDefault()
                    });
                }
            }

            return response;
        }

        private CalculationResponse toExtHitTestResponse(Levels levels, SupportAndResistanceResult sar)
        {
            return new CalculationResponse
            {
                levels = ToLevels(levels, sar)
            };
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

            return new Strategy2CalculationResponse
            {
                top_ex2 = top_ext2,
                top_ex1 = top_ext1,
                r = resistance,
                s = support,
                n = natural,
                bottom_ex1 = bottom_ext1,
                bottom_ex2 = bottom_ext2,
                trend = calculationData.trend,
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

            var tradeResponse = new StrategyModeV1
            {
                AlgoTP2 = trade.algo_TP2,
                AlgoTP1High = trade.algo_TP1_high,
                AlgoTP1Low = trade.algo_TP1_low,
                AlgoEntryHigh = trade.algo_Entry_high,
                AlgoEntryLow = trade.algo_Entry_low,
                AlgoEntry = trade.algo_Entry,
                AlgoStop = trade.algo_Stop,
                AlgoRisk = trade.algo_Risk,
                AlgoInfo = algoInfo
            };

            return new CalculationResponse
            {
                levels = ToLevels(levels, sar),
                trade = tradeResponse
            };
        }

        private StrategyModeV2 toStrategyModeV2(ScanResponse scanRes)
        {
            if (scanRes == null)
            {
                return null;
            }

            return new StrategyModeV2
            {
                trend = scanRes.trend,
                type = scanRes.type,
                tp = scanRes.tp,
                tte = scanRes.tte,
                entry = scanRes.entry,
                stop = scanRes.stop,
                risk = scanRes.risk,
                sl_ratio = scanRes.sl_ratio,
                entry_h = scanRes.entry_h,
                entry_l = scanRes.entry_l,
                take_profit = scanRes.take_profit,
                take_profit_h = scanRes.take_profit_h,
                take_profit_l = scanRes.take_profit_l
            };
        }

        private CalculationResponseV2 toResponseV2(Levels levels, SupportAndResistanceResult sar, ScanResponse scanRes, decimal size)
        {
            return new CalculationResponseV2
            {
                levels = ToLevels(levels, sar),
                trade = toStrategyModeV2(scanRes),
                size = size
            };
        }
        private CalculationMarketInfoResponse tryGetCalculateMarketInfoFromCache(Instrument instrument)
        {
            var hash = instrument.ToString() + "_marketinfo";
            try
            {
                if (_cache.TryGetValue(_cachePrefix, hash, out CalculationMarketInfoResponse cachedResponse))
                {
                    return cachedResponse;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get cached response for marketinfo");
                _logger.LogError(e.Message);
            }

            return null;
        }

        private void tryAddCalculateMarketInfoInCache(Instrument instrument, CalculationMarketInfoResponse data)
        {
            var hash = instrument.ToString() + "_marketinfo";
            try
            {
                _cache.Set(_cachePrefix, hash, data, TimeSpan.FromMinutes(10));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to add cached response for marketinfo");
                _logger.LogError(e.Message);
            }
        }
        private CalculationMarketInfoResponse tryGetCalculateMarketInfoV2FromCache(Instrument instrument, int timeframe)
        {
            var hash = instrument.ToString() + timeframe + "_marketinfoV2";
            try
            {
                if (_cache.TryGetValue(_cachePrefix, hash, out CalculationMarketInfoResponse cachedResponse))
                {
                    return cachedResponse;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get cached response for marketinfo");
                _logger.LogError(e.Message);
            }

            return null;
        }

        private void tryAddCalculateMarketInfoV2InCache(Instrument instrument, int timeframe, CalculationMarketInfoResponse data)
        {
            var hash = instrument.ToString() + timeframe + "_marketinfoV2";
            try
            {
                _cache.Set(_cachePrefix, hash, data, TimeSpan.FromMinutes(10));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to add cached response for marketinfo");
                _logger.LogError(e.Message);
            }
        }

        public CalculationLevels ToLevels(Levels levels, SupportAndResistanceResult sar)
        {
            return new CalculationLevels
            {
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
            };
        }
    }
}
