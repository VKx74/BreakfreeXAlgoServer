using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.Algo;
using Algoserver.API.Models.REST;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.Services
{
    // Legacy
    public class BacktestService 
    {
        private readonly ILogger<BacktestService> _logger;
        private readonly HistoryService _historyService;
        private readonly ScannerService _scanner;

        public BacktestService(ILogger<BacktestService> logger, HistoryService historyService, ScannerService scanner)
        {
            _logger = logger;
            _historyService = historyService;
            _scanner = scanner;
        }
        
        public Task<BacktestResponse> BacktestAsync(BacktestRequest req)
        {
            return Task.Run(async () =>
            {
                return await backtestAsync(req);
            });
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

            var granularity = AlgoHelper.ConvertTimeframeToGranularity(container.TimeframeInterval, container.TimeframePeriod);
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

                    var result = DataMappingHelper.ToResponse(levels, sar, trade);
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

        public Task<Strategy2BacktestResponse> Strategy2BacktestAsync(Strategy2BacktestRequest req)
        {
            return Task.Run(async () =>
            {
                return await strategy2BacktestAsync(req);
            });
        }
        
        private async Task<Strategy2BacktestResponse> strategy2BacktestAsync(Strategy2BacktestRequest req)
        {
            var container = InputDataContainer.MapCalculationRequestToInputDataContainer(req);
            // no need USD rate for this test
            container.setUsdRatio(1);

            var granularity = AlgoHelper.ConvertTimeframeToGranularity(container.TimeframeInterval, container.TimeframePeriod);
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
            var response = new Strategy2BacktestResponse();
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
                    var result = DataMappingHelper.ToStrategy2BacktestData(calculationData, tradeSR, tradeEx1);
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
    
        public Task<ExtHitTestResponse> HitTestExtensionsAsync(HittestRequest req)
        {
            return Task.Run(async () =>
            {
                return await hitTestExtensionsAsync(req);
            });
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

            var granularity = AlgoHelper.ConvertTimeframeToGranularity(container.TimeframeInterval, container.TimeframePeriod);
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
                var result = DataMappingHelper.ToResponse(levels, sar);
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
    
        public Task<List<ExtensionBacktestResponse>> BacktestExtensionsAsync(ExtensionsBacktestRequest req)
        {
            return Task.Run(async () =>
            {
                return await backtestExtensionsAsync(req);
            });
        }
        
        private async Task<List<ExtensionBacktestResponse>> backtestExtensionsAsync(ExtensionsBacktestRequest req)
        {
            var instrument = req.Instrument;
            var granularity = req.Granularity.GetValueOrDefault(TimeframeHelper.DAILY_GRANULARITY);
            var currentPriceData = await _historyService.GetHistoryByDates(instrument.Datafeed, instrument.Id, granularity, instrument.Exchange, req.From, req.To);
            var container = new InputDataContainer();
            var bars = currentPriceData.Bars.ToList();

            // need at least 128 bars to calculate SAR
            var replayBack = bars.Count - 128;
            var response = new List<ExtensionBacktestResponse>();

            while (replayBack-- >= 0)
            {
                container.AddNext(currentPriceData.Bars, null, null, replayBack);
                var levels = TechCalculations.CalculateLevels(container.High, container.Low);
                var sar = SupportAndResistance.Calculate(levels, container.Mintick);
                var lastLevel = response.LastOrDefault();

                if (lastLevel == null || lastLevel.levels.ZE != levels.Level128.ZeroEight || lastLevel.levels.EE != levels.Level128.EightEight)
                {
                    var responseItem = DataMappingHelper.ToLevels(levels, sar);
                    response.Add(new ExtensionBacktestResponse {
                        levels = responseItem,
                        timestamp = container.Time.LastOrDefault()
                    });
                }
            }

            return response;
        }
    }
}
