using System;
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

        public Task<CalculatePositionSizeResponse> CalculatePositionSize(CalculatePositionSizeRequest req)
        {
            return Task.Run(async () =>
            {
                return await calculatePositionSize(req);
            });
        }

        public Task<CalculatePriceRatioResponse> CalculatePriceRatio(CalculatePriceRatioRequest req)
        {
            return Task.Run(async () =>
            {
                return await calculatePriceRatio(req);
            });
        }

        // Legacy
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
            var result = DataMappingHelper.ToResponse(levels, sar, trade);
            return result;
        }

        // Legacy
        public Task<CalculationMarketInfoResponse> CalculateMarketInfoAsync(Instrument instrument)
        {
            return Task.Run(() =>
            {
                return calculateMarketInfoAsync(instrument);
            });
        }

        public Task<decimal?> CalculateCVarAsync(Instrument instrument)
        {
            return Task.Run(() =>
            {
                return calculateCVarAsync(instrument);
            });
        }

        public Task<CalculationMarketInfoResponse> CalculateMarketInfoV2Async(MarketInfoCalculationRequest request)
        {
            return Task.Run(() =>
            {
                return calculateMarketInfoV2Async(request);
            });
        }

        public Task<CalculationResponseV2> CalculateV2Async(CalculationRequest req)
        {
            return Task.Run(() =>
            {
                return calculateV2Async(req);
            });
        }

        private async Task<decimal?> calculateCVarAsync(Instrument instrument)
        {
            var cvar = tryGetCVarFromCache(instrument);

            if (!cvar.HasValue)
            {
                var dataDaily = await _historyService.GetHistory(instrument.Id, TimeframeHelper.DAILY_GRANULARITY, instrument.Datafeed, instrument.Exchange, instrument.Type);
                var closeDaily = dataDaily.Bars.Select(_ => _.Close).ToList();
                cvar = TechCalculations.CalculateCVAR(closeDaily);
                tryAddCVarInCache(instrument, cvar.Value);
            }

            return cvar;
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
            var cvar = tryGetCVarFromCache(instrument);

            if (!cvar.HasValue)
            {
                cvar = TechCalculations.CalculateCVAR(close);
                tryAddCVarInCache(instrument, cvar.Value);
            }

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
                local_trend_spread = trend.LocalTrendSpread,
                cvar = cvar.GetValueOrDefault(0)
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
            var cvar = tryGetCVarFromCache(instrument);

            if (!cvar.HasValue)
            {
                cvar = TechCalculations.CalculateCVAR(closeDaily);
                tryAddCVarInCache(instrument, cvar.Value);
            }

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
                local_trend_spread = trend.LocalTrendSpread,
                cvar = cvar.GetValueOrDefault(0)
            };

            tryAddCalculateMarketInfoV2InCache(instrument, timeframe, result);

            return result;
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
                Time = container.Time
            };

            var dailyScanningHistory = new ScanningHistory
            {
                Open = container.OpenD,
                High = container.HighD,
                Low = container.LowD,
                Close = container.CloseD,
                Time = container.TimeD
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

            var result = DataMappingHelper.ToResponseV2(levels, sar, scanRes, size);
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

            return new CalculatePositionSizeResponse
            {
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

            return new CalculatePriceRatioResponse
            {
                ratio = usdRatio
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

        private decimal? tryGetCVarFromCache(Instrument instrument)
        {
            var hash = instrument.ToString() + "_cvar";
            try
            {
                if (_cache.TryGetValue(_cachePrefix, hash, out decimal cachedResponse))
                {
                    return cachedResponse;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get cached response for cvar");
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

        private void tryAddCVarInCache(Instrument instrument, decimal value)
        {
            var hash = instrument.ToString() + "_cvar";
            try
            {
                _cache.Set(_cachePrefix, hash, value, TimeSpan.FromDays(1));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to add cached response for marketinfo");
                _logger.LogError(e.Message);
            }
        }

        // -------- for ML //

        public Task<MLDataResponse> CalculateSRAsync(string symbol, int granularity)
        {
            return Task.Run(async () =>
            {
                return await calculateSRAsync(symbol, granularity);
            });
        }

        private async Task<MLDataResponse> calculateSRAsync(string symbol, int granularity)
        {
            var res = new MLDataResponse();
            res.data = new List<MLDataResponseItem>();

            var data = await _historyService.GetHistory(symbol, granularity, "Oanda", "Oanda", "Forex");
            var bars = data.Bars.ToList();

            for (var i = 0; i < bars.Count; i++) 
            {
                var item = new MLDataResponseItem {
                    open = bars[i].Open,
                    high = bars[i].High,
                    low = bars[i].Low,
                    close = bars[i].Close,
                    time = bars[i].Timestamp
                };

                res.data.Add(item);

                if (i > 128) {
                    var high = res.data.Select(_ => _.high);
                    var low = res.data.Select(_ => _.low);
                    var levels = TechCalculations.CalculateLevel128(high, low);
                    
                    item.upExt1 = levels.Plus18;
                    item.upExt2 = levels.Plus28;
                    item.downExt1 = levels.Minus18;
                    item.downExt2 = levels.Minus28;

                    item.n = levels.FourEight;
                    item.s = levels.ZeroEight;
                    item.r = levels.EightEight;
                }
            }

            return res;
        }
        
    }
}
