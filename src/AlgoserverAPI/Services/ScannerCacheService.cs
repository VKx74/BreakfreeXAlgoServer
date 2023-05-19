using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Algoserver.API.Helpers;
using Algoserver.API.Models.Algo;
using Algoserver.API.Models.REST;
using Algoserver.API.Services.CacheServices;

namespace Algoserver.API.Services
{
    public class ScannerForexCacheService : ScannerCacheService
    {
        public ScannerForexCacheService(ScannerForexHistoryService historyService, ScannerService scanner, ICacheService cache) : base(historyService, scanner, cache)
        {
        }

        protected override string cachePrefix()
        {
            return "ForexScanner_";
        }
    }

    public class ScannerStockCacheService : ScannerCacheService
    {
        public ScannerStockCacheService(ScannerStockHistoryService historyService, ScannerService scanner, ICacheService cache) : base(historyService, scanner, cache)
        {
        }
        protected override string cachePrefix()
        {
            return "StockScanner_";
        }
    }

    public class ScannerCryptoCacheService : ScannerCacheService
    {
        public ScannerCryptoCacheService(ScannerCryptoHistoryService historyService, ScannerService scanner, ICacheService cache) : base(historyService, scanner, cache)
        {
        }
        protected override string cachePrefix()
        {
            return "CryptoScanner_";
        }
    }

    public abstract class ScannerCacheService
    {
        protected string _mesaCachePrefix = "MesaCache_";
        protected readonly ICacheService _cache;
        protected readonly ScannerHistoryService _historyService;
        protected readonly ScannerService _scanner;
        protected readonly string _scannerDataCacheKey = "data";
        protected readonly string _scannerHistoryCacheKey = "history";
        // protected readonly List<ScannerResponseItem> _scannerResults = new List<ScannerResponseItem>();
        protected readonly List<ScannerResponseHistoryItem> _resultHistory = new List<ScannerResponseHistoryItem>();
        protected int scanning_time { get; set; }
        protected int data_count_15_min { get; set; }
        protected int data_count_1_h { get; set; }
        protected int data_count_4_h { get; set; }
        protected int data_count_1_d { get; set; }
        public string RefreshAllMarketsTime { get; set; }
        public string RefreshLongMinuteHistoryTime { get; set; }
        public string RefreshMarketsTime { get; set; }

        public ScannerCacheService(ScannerHistoryService historyService, ScannerService scanner, ICacheService cache)
        {
            _cache = cache;
            _historyService = historyService;
            _scanner = scanner;
        }

        public List<ScannerResponseItem> GetData()
        {
            if (_cache.TryGetValue(cachePrefix(), _scannerDataCacheKey, out List<ScannerResponseItem> cachedResponse))
            {
                return cachedResponse.ToList();
            }

            return new List<ScannerResponseItem>();
        }

        public List<ScannerResponseHistoryItem> GetHistoryData()
        {
            if (_cache.TryGetValue(cachePrefix(), _scannerHistoryCacheKey, out List<ScannerResponseHistoryItem> cachedResponse))
            {
                return cachedResponse.ToList();
            }

            return new List<ScannerResponseHistoryItem>();
        }

        public ScannerCacheItem GetSonarHistoryCache(string symbol, string exchange, int timeframe, long time)
        {
            var key = $"{symbol}-{exchange}-{timeframe}-{time}".ToUpper();
            if (_cache.TryGetValue(cachePrefix(), key, out ScannerCacheItem cachedResponse))
            {
                return cachedResponse;
            }

            return null;
        }

        public void CalculateMinuteMesa()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var _1Mins = _historyService.Get1MinLongData();
            var count = 0;

            foreach (var minHistory in _1Mins)
            {
                try
                {
                    if (minHistory.Bars == null || minHistory == null) 
                    {
                        continue;
                    }

                    var calculation_input = minHistory.Bars.Select(_ => _.Close).ToList();
                    if (calculation_input.Count < 44000)
                    {
                        continue;
                    }
                    // "granularity": ['1min', '5min', '15min', '60min', '240min', '1440min'],
                    // "limits": [(0.0325, 0.0325), (0.0085, 0.0085), (0.0032, 0.0032), (0.0012, 0.0012), (0.0007, 0.0007), (0.00039, 0.00039)],
                    var mesa1min = TechCalculations.MESA(calculation_input, 0.0325, 0.0325);
                    var mesa5min = TechCalculations.MESA(calculation_input, 0.0085, 0.0085);
                    var mesa15min = TechCalculations.MESA(calculation_input, 0.0032, 0.0032);
                    var mesa1h = TechCalculations.MESA(calculation_input, 0.0012, 0.0012);
                    var mesa4h = TechCalculations.MESA(calculation_input, 0.0007, 0.0007);
                    var mesa1d = TechCalculations.MESA(calculation_input, 0.00039, 0.00039);
                    var res = new Dictionary<int, List<MESAData>>();
                    res.Add(60, mesa1min.TakeLast(10000).ToList());
                    res.Add(300, mesa5min.TakeLast(10000).ToList());
                    res.Add(900, mesa15min.TakeLast(10000).ToList());
                    res.Add(3600, mesa1h.TakeLast(10000).ToList());
                    res.Add(14400, mesa4h.TakeLast(10000).ToList());
                    res.Add(86400, mesa1d.TakeLast(10000).ToList());

                    SetMinuteMesaCache(res, minHistory.Datafeed + "_" + minHistory.Symbol);
                    count++;
                }
                catch (Exception ex) { 
                    Console.WriteLine(">>> " + minHistory.Symbol);
                    Console.WriteLine(ex);
                }
            }

            stopWatch.Stop();
            TimeSpan ts1 = stopWatch.Elapsed;
            string elapsedTime1 = String.Format(" * 1 min MESA calculation {0:00}:{1:00} - instruments count " + count, ts1.Minutes, ts1.Seconds);
            Console.WriteLine(">>> " + elapsedTime1);
        }

        private void SetMinuteMesaCache(Dictionary<int, List<MESAData>> mesa, string key)
        {
            try
            {
                _cache.Set(_mesaCachePrefix, key.ToLower(), mesa, TimeSpan.FromDays(1));
            }
            catch (Exception ex) { }
        }

        public void ScanMarkets()
        {
            var _1Mins = _historyService.Get1MinDataDictionary();
            var _5Mins = _historyService.Get5MinDataDictionary();
            var _15Mins = _historyService.Get15MinDataDictionary();
            var _30Mins = _historyService.Get30MinDataDictionary();
            var _1Hour = _historyService.Get1HDataDictionary();
            var _4Hour = _historyService.Get4HDataDictionary();
            var _1Day = _historyService.Get1DData();
            var res = new List<ScannerResponseItem>();

            foreach (var dailyHistory in _1Day)
            {
                if (dailyHistory == null || dailyHistory.Bars.Count() < 200)
                {
                    continue;
                }

                var key = _historyService.GetKey(dailyHistory);
                var calculation_input = dailyHistory.Bars.Select(_ => _.Close).ToList();
                var extendedTrendData = TrendDetector.CalculateByMesaBy2TrendAdjusted(calculation_input);
                var trendData = TrendDetector.MergeTrends(extendedTrendData);
                var dailyScanningHistory = _scanner.ToScanningHistory(dailyHistory.Bars);

                if (_1Mins.TryGetValue(key, out var history1Min) && _1Hour.TryGetValue(key, out var history30Min))
                {
                    if (history1Min.Bars.Count >= 200 && history30Min.Bars.Count >= 200)
                    {
                        var calculation_input_30 = history30Min.Bars.Select(_ => _.Close).ToList();
                        var extendedTrendData30min = TrendDetector.CalculateByMesaBy2TrendAdjusted(calculation_input_30);
                        var trendData30min = TrendDetector.MergeTrends(extendedTrendData30min);
                        var history = _scanner.ToScanningHistory(history1Min.Bars);
                        var levels = TechCalculations.CalculateLevel128(history.High, history.Low);

                        var scanningResultExt = _scanner.ScanExt(history, _scanner.ToScanningHistory(history30Min.Bars), trendData30min, levels);
                        if (scanningResultExt != null)
                        {
                            var resp = _toResponse(scanningResultExt, history1Min, TimeframeHelper.MIN1_GRANULARITY);
                            _tryAddHistory(resp, scanningResultExt, extendedTrendData30min);
                            res.Add(resp);
                        }
                    }
                    else
                    {
                        Console.WriteLine(">>> Not enough of data " + key);
                    }
                }

                if (_5Mins.TryGetValue(key, out var history5Min) && _4Hour.TryGetValue(key, out var history1Hour))
                {
                    if (history5Min.Bars.Count >= 200 && history1Hour.Bars.Count >= 200)
                    {
                        var calculation_input_1h = history1Hour.Bars.Select(_ => _.Close).ToList();
                        var extendedTrendData1h = TrendDetector.CalculateByMesaBy2TrendAdjusted(calculation_input_1h);
                        var trendData1h = TrendDetector.MergeTrends(extendedTrendData1h);
                        var history = _scanner.ToScanningHistory(history5Min.Bars);
                        var levels = TechCalculations.CalculateLevel128(history.High, history.Low);

                        var scanningResultExt = _scanner.ScanExt(history, _scanner.ToScanningHistory(history1Hour.Bars), trendData1h, levels);
                        if (scanningResultExt != null)
                        {
                            var resp = _toResponse(scanningResultExt, history5Min, TimeframeHelper.MIN5_GRANULARITY);
                            _tryAddHistory(resp, scanningResultExt, extendedTrendData1h);
                            res.Add(resp);
                        }
                    }
                    else
                    {
                        Console.WriteLine(">>> Not enough of data " + key);
                    }
                }

                if (_15Mins.TryGetValue(key, out var history15Min) && trendData != Trend.Undefined && history15Min.Bars.Count() > 200)
                {
                    var history = _scanner.ToScanningHistory(history15Min.Bars);
                    var levels = TechCalculations.CalculateLevel128(history.High, history.Low);
                    // var scanningResultBRC = _scanner.ScanBRC(history, trendData, levels);
                    // if (scanningResultBRC != null)
                    // {
                    //     var resp = _toResponse(scanningResultBRC, history15Min, TimeframeHelper.MIN15_GRANULARITY);
                    //     _tryAddHistory(resp, scanningResultBRC, extendedTrendData);
                    //     res.Add(resp);
                    // }

                    var scanningResultExt = _scanner.ScanExt(history, dailyScanningHistory, trendData, levels);
                    if (scanningResultExt != null)
                    {
                        var resp = _toResponse(scanningResultExt, history15Min, TimeframeHelper.MIN15_GRANULARITY);
                        _tryAddHistory(resp, scanningResultExt, extendedTrendData);
                        res.Add(resp);
                    }
                }

                if (_30Mins.TryGetValue(key, out var history30M) && trendData != Trend.Undefined)
                {
                    if (history30M.Bars.Count >= 200)
                    {
                        var history = _scanner.ToScanningHistory(history30M.Bars);
                        var levels = TechCalculations.CalculateLevel128(history.High, history.Low);
                        var scanningResultExt = _scanner.ScanExt(history, dailyScanningHistory, trendData, levels);
                        if (scanningResultExt != null)
                        {
                            var resp = _toResponse(scanningResultExt, history30M, TimeframeHelper.MIN30_GRANULARITY);
                            _tryAddHistory(resp, scanningResultExt, extendedTrendData);
                            res.Add(resp);
                        }
                    }
                }

                if (_1Hour.TryGetValue(key, out var history1H) && trendData != Trend.Undefined && history1H.Bars.Count() > 200)
                {
                    var history = _scanner.ToScanningHistory(history1H.Bars);
                    var levels = TechCalculations.CalculateLevel128(history.High, history.Low);
                    // var scanningResultBRC = _scanner.ScanBRC(history, trendData, levels);
                    // if (scanningResultBRC != null)
                    // {
                    //     var resp = _toResponse(scanningResultBRC, history1H, TimeframeHelper.HOURLY_GRANULARITY);
                    //     _tryAddHistory(resp, scanningResultBRC, extendedTrendData);
                    //     res.Add(resp);
                    // }

                    var scanningResultExt = _scanner.ScanExt(history, dailyScanningHistory, trendData, levels);
                    if (scanningResultExt != null)
                    {
                        var resp = _toResponse(scanningResultExt, history1H, TimeframeHelper.HOURLY_GRANULARITY);
                        _tryAddHistory(resp, scanningResultExt, extendedTrendData);
                        res.Add(resp);
                    }
                }

                if (_4Hour.TryGetValue(key, out var history4H) && history4H.Bars.Count() > 200)
                {
                    var tradeDetermined = false;
                    var history = _scanner.ToScanningHistory(history4H.Bars);
                    var levels = TechCalculations.CalculateLevel128(history.High, history.Low);

                    if (trendData != Trend.Undefined)
                    {
                        // var scanningResultBRC = _scanner.ScanBRC(history, trendData, levels);
                        // if (scanningResultBRC != null)
                        // {
                        //     var resp = _toResponse(scanningResultBRC, history4H, TimeframeHelper.HOUR4_GRANULARITY);
                        //     _tryAddHistory(resp, scanningResultBRC, extendedTrendData);
                        //     res.Add(resp);
                        //     tradeDetermined = true;
                        // }

                        var scanningResultExt = _scanner.ScanExt(history, dailyScanningHistory, trendData, levels);
                        if (scanningResultExt != null)
                        {
                            var resp = _toResponse(scanningResultExt, history4H, TimeframeHelper.HOUR4_GRANULARITY);
                            _tryAddHistory(resp, scanningResultExt, extendedTrendData);
                            res.Add(resp);
                            tradeDetermined = true;
                        }
                    }

                    // if (!tradeDetermined && !extendedTrendData.IsOverhit)
                    // {
                    //     var swingScannerResult = _scanner.ScanSwing(history, dailyScanningHistory, extendedTrendData.GlobalTrend, extendedTrendData.LocalTrend, levels);
                    //     if (swingScannerResult != null)
                    //     {
                    //         var resp = _toResponse(swingScannerResult, history4H, TimeframeHelper.HOUR4_GRANULARITY);
                    //         _tryAddHistory(resp, swingScannerResult, extendedTrendData);
                    //         res.Add(resp);
                    //     }
                    // }
                }

                // var swingDailyScannerResult = _scanner.ScanSwingOldStrategy(dailyScanningHistory);
                // if (swingDailyScannerResult != null)
                // {
                //     var resp = _toResponse(swingDailyScannerResult, dailyHistory, TimeframeHelper.DAILY_GRANULARITY);
                //     _tryAddHistory(resp, swingDailyScannerResult, extendedTrendData);
                //     res.Add(resp);
                // }
            }

            try
            {
                _cache.Set(cachePrefix(), _scannerDataCacheKey, res.ToList(), TimeSpan.FromDays(1));
            }
            catch (Exception ex) { }

            data_count_15_min = _15Mins.Count;
            data_count_1_h = _1Hour.Count;
            data_count_4_h = _4Hour.Count;
            data_count_1_d = _1Day.Count;
            scanning_time = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        protected ScannerResponseItem _toResponse(ScanResponse response, HistoryData data, int timeframe)
        {
            var lastBar = data.Bars.LastOrDefault();
            var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            var key = $"{data.Symbol}-{data.Exchange}-{timeframe}-{response.time}".ToUpper();
            return new ScannerResponseItem
            {
                type = response.type,
                exchange = data.Exchange,
                symbol = data.Symbol,
                timeframe = timeframe,
                trend = response.trend,
                tp = response.tp,
                tte = response.tte,
                entry = response.entry,
                stop = response.stop,
                time = response.time,
                id = key,
                marketType = InstrumentsHelper.GetInstrumentType(data.Symbol),
                expiration = timestamp + (timeframe * 10)
            };
        }

        protected void _tryAddHistory(ScannerResponseItem item, ScanResponse resp, ExtendedTrendResult extendedTrendData)
        {
            try
            {
                var key = item.id;
                var expirationInSeconds = item.timeframe * 1000;
                var data = new ScannerCacheItem
                {
                    responseItem = item,
                    trend = _toTrendResponse(extendedTrendData),
                    trade = resp,
                    avgEntry = resp.entry,
                    time = AlgoHelper.UnixTimeNow()
                };
                _cache.Set(cachePrefix(), key, data, TimeSpan.FromSeconds(expirationInSeconds));
            }
            catch (Exception ex) { }

            ScannerResponseHistoryItem last;
            lock (_resultHistory)
            {
                last = _resultHistory.LastOrDefault(_ => _.responseItem.exchange == item.exchange && _.responseItem.symbol == item.symbol &&
                                                         _.responseItem.timeframe == item.timeframe && _.responseItem.type == item.type);
            }

            if (last != null && last.avgEntry == resp.entry)
            {
                return;
            }

            lock (_resultHistory)
            {
                while (_resultHistory.Count >= 1000)
                {
                    _resultHistory.RemoveAt(0);
                }
                _resultHistory.Add(new ScannerResponseHistoryItem
                {
                    responseItem = item,
                    avgEntry = resp.entry,
                    time = AlgoHelper.UnixTimeNow()
                });
            }


            try
            {
                _cache.Set(cachePrefix(), _scannerHistoryCacheKey, _resultHistory.ToList(), TimeSpan.FromDays(1));
            }
            catch (Exception ex) { }
        }

        protected TrendResponse _toTrendResponse(ExtendedTrendResult extendedTrendData)
        {
            return new TrendResponse
            {
                globalFastValue = extendedTrendData.GlobalFastValue,
                globalSlowValue = extendedTrendData.GlobalSlowValue,
                globalTrend = extendedTrendData.GlobalTrend,
                globalTrendSpread = extendedTrendData.GlobalTrendSpread,
                globalTrendSpreadValue = extendedTrendData.GlobalTrendSpreadValue,
                localFastValue = extendedTrendData.LocalFastValue,
                localSlowValue = extendedTrendData.LocalSlowValue,
                localTrend = extendedTrendData.LocalTrend,
                localTrendSpread = extendedTrendData.LocalTrendSpread,
                localTrendSpreadValue = extendedTrendData.LocalTrendSpreadValue
            };
        }

        protected abstract string cachePrefix();
    }
}
