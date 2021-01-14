using System;
using System.Collections.Generic;
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

        public void ScanMarkets()
        {
            var _15Mins = _historyService.Get15MinDataDictionary();
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

                var calculation_input = dailyHistory.Bars.Select(_ => _.Close).ToList();
                var extendedTrendData = TrendDetector.CalculateByMesaBy2TrendAdjusted(calculation_input);
                var trendData = TrendDetector.MergeTrends(extendedTrendData);
                var dailyScanningHistory = _scanner.ToScanningHistory(dailyHistory.Bars);

                if (_15Mins.TryGetValue(_historyService.GetKey(dailyHistory), out var history15Min) && trendData != Trend.Undefined && history15Min.Bars.Count() > 200)
                {
                    var scanningResultBRC = _scanner.ScanBRC(_scanner.ToScanningHistory(history15Min.Bars), trendData);
                    if (scanningResultBRC != null)
                    {
                        var resp = _toResponse(scanningResultBRC, history15Min, TimeframeHelper.MIN15_GRANULARITY);
                        _tryAddHistory(resp, scanningResultBRC);
                        res.Add(resp);
                    }

                    var scanningResultExt = _scanner.ScanExt(_scanner.ToScanningHistory(history15Min.Bars), dailyScanningHistory, trendData);
                    if (scanningResultExt != null)
                    {
                        var resp = _toResponse(scanningResultExt, history15Min, TimeframeHelper.MIN15_GRANULARITY);
                        _tryAddHistory(resp, scanningResultExt);
                        res.Add(resp);
                    }
                }

                if (_1Hour.TryGetValue(_historyService.GetKey(dailyHistory), out var history1H) && trendData != Trend.Undefined && history1H.Bars.Count() > 200)
                {
                    var scanningResultBRC = _scanner.ScanBRC(_scanner.ToScanningHistory(history1H.Bars), trendData);
                    if (scanningResultBRC != null)
                    {
                        var resp = _toResponse(scanningResultBRC, history1H, TimeframeHelper.HOURLY_GRANULARITY);
                        _tryAddHistory(resp, scanningResultBRC);
                        res.Add(resp);
                    }

                    var scanningResultExt = _scanner.ScanExt(_scanner.ToScanningHistory(history1H.Bars), dailyScanningHistory, trendData);
                    if (scanningResultExt != null)
                    {
                        var resp = _toResponse(scanningResultExt, history1H, TimeframeHelper.HOURLY_GRANULARITY);
                        _tryAddHistory(resp, scanningResultExt);
                        res.Add(resp);
                    }
                }

                if (_4Hour.TryGetValue(_historyService.GetKey(dailyHistory), out var history4H) && history4H.Bars.Count() > 200)
                {
                    var tradeDetermined = false;
                    if (trendData != Trend.Undefined)
                    {
                        var scanningResultBRC = _scanner.ScanBRC(_scanner.ToScanningHistory(history4H.Bars), trendData);
                        if (scanningResultBRC != null)
                        {
                            var resp = _toResponse(scanningResultBRC, history4H, TimeframeHelper.HOUR4_GRANULARITY);
                            _tryAddHistory(resp, scanningResultBRC);
                            res.Add(resp);
                            tradeDetermined = true;
                        }

                        var scanningResultExt = _scanner.ScanExt(_scanner.ToScanningHistory(history4H.Bars), dailyScanningHistory, trendData);
                        if (scanningResultExt != null)
                        {
                            var resp = _toResponse(scanningResultExt, history4H, TimeframeHelper.HOUR4_GRANULARITY);
                            _tryAddHistory(resp, scanningResultExt);
                            res.Add(resp);
                            tradeDetermined = true;
                        }
                    }

                    if (!tradeDetermined)
                    {
                        var swingScannerResult = _scanner.ScanSwing(_scanner.ToScanningHistory(history4H.Bars), dailyScanningHistory, extendedTrendData.GlobalTrend, extendedTrendData.LocalTrend);
                        if (swingScannerResult != null)
                        {
                            var resp = _toResponse(swingScannerResult, history4H, TimeframeHelper.HOUR4_GRANULARITY);
                            _tryAddHistory(resp, swingScannerResult);
                            res.Add(resp);
                        }
                    }
                }

                var swingDailyScannerResult = _scanner.ScanSwingOldStrategy(dailyScanningHistory);
                if (swingDailyScannerResult != null)
                {
                    var resp = _toResponse(swingDailyScannerResult, dailyHistory, TimeframeHelper.DAILY_GRANULARITY);
                    _tryAddHistory(resp, swingDailyScannerResult);
                    res.Add(resp);
                }
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
                stop = response.stop
            };
        }

        protected void _tryAddHistory(ScannerResponseItem item, ScanResponse resp)
        {
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
                while (_resultHistory.Count >= 500)
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
        protected abstract string cachePrefix();
    }
}
