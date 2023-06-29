using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

        protected override bool scanSymbol(string symbol)
        {
            if (String.Equals(symbol, "BTC_USD", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (String.Equals(symbol, "ETH_USD", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            return true;
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

        protected override bool scanSymbol(string symbol)
        {
            return true;
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

        protected override bool scanSymbol(string symbol)
        {
            return true;
        }
    }

    public abstract class ScannerCacheService
    {
        private static int longMinHistoryCount = 21600;
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
        private List<MESADataSummary> MESADataSummaryCache;
        private int MESADataSummaryCacheMinute = 0;

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

        public List<MESADataSummary> GetMesaSummary()
        {
            var currentMinute = DateTime.UtcNow.Minute;
            if (MESADataSummaryCache != null && MESADataSummaryCacheMinute == currentMinute)
            {
                return MESADataSummaryCache.ToList();
            }
            MESADataSummaryCacheMinute = currentMinute;

            if (_cache.TryGetValue(cachePrefix(), "mesa_data_summary", out List<MESADataSummary> cachedResponse))
            {
                MESADataSummaryCache = cachedResponse;
                return cachedResponse.ToList();
            }

            return new List<MESADataSummary>();
        }

        public async Task<List<MESADataSummary>> GetMesaSummaryAsync()
        {
            var currentMinute = DateTime.UtcNow.Minute;
            if (MESADataSummaryCache != null && MESADataSummaryCacheMinute == currentMinute)
            {
                return MESADataSummaryCache.ToList();
            }
            MESADataSummaryCacheMinute = currentMinute;

            var cachedResponse = await _cache.TryGetValueAsync<List<MESADataSummary>>(cachePrefix(), "mesa_data_summary");
            if (cachedResponse != null)
            {
                MESADataSummaryCache = cachedResponse;
                return cachedResponse.ToList();
            }

            return new List<MESADataSummary>();
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

        public async Task CalculateMinuteMesa()
        {
            await Task.Run(async () =>
                       {
                           await CalculateMinuteMesaAsync();
                       });
        }

        private async Task CalculateMinuteMesaAsync()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var _1Mins = _historyService.Get1MinLongData();
            var _1Hour = _historyService.Get1HDataDictionary();
            var count = 0;
            var summary = new List<MESADataSummary>();

            foreach (var minHistory in _1Mins)
            {
                try
                {
                    if (minHistory == null || minHistory.Bars == null)
                    {
                        continue;
                    }

                    var calculation_input = minHistory.Bars.Select(_ => _.Close);
                    if (calculation_input.Count() < 45000)
                    {
                        continue;
                    }

                    HistoryData hourlyHistory;
                    if (!_1Hour.TryGetValue(minHistory.Symbol + minHistory.Exchange, out hourlyHistory))
                    {
                        continue;
                    }

                    if (hourlyHistory == null)
                    {
                        continue;
                    }

                    var hourly_calculation_input = hourlyHistory.Bars.Select(_ => _.Close);

                    // "granularity": ['1min', '5min', '15min', '60min', '240min', '1440min'],
                    // "limits": [(0.0325, 0.0325), (0.0085, 0.0085), (0.0032, 0.0032), (0.0012, 0.0012), (0.0007, 0.0007), (0.00039, 0.00039)],
                    var mesa1driver = TechCalculations.MESA(calculation_input.TakeLast(28000).ToList(), 0.0325, 0.0325);
                    var mesa1min = TechCalculations.MESA(calculation_input.TakeLast(32000).ToList(), 0.0085, 0.0085);
                    var mesa5min = TechCalculations.MESA(calculation_input.TakeLast(36000).ToList(), 0.0032, 0.0032);
                    var mesa15min = TechCalculations.MESA(calculation_input.TakeLast(40000).ToList(), 0.0012, 0.0012);
                    var mesa1h = TechCalculations.MESA(calculation_input.TakeLast(44000).ToList(), 0.0007, 0.0007);
                    var mesa4h = TechCalculations.MESA(calculation_input.ToList(), 0.00039, 0.00039);
                    var mesa1d = TechCalculations.MESA(hourly_calculation_input.ToList(), 0.0085, 0.0085);

                    var mesa1driverCut = mesa1driver.TakeLast(longMinHistoryCount).ToList();
                    var mesa1minCut = mesa1min.TakeLast(longMinHistoryCount).ToList();
                    var mesa5minCut = mesa5min.TakeLast(longMinHistoryCount).ToList();
                    var mesa15minCut = mesa15min.TakeLast(longMinHistoryCount).ToList();
                    var mesa1hCut = mesa1h.TakeLast(longMinHistoryCount).ToList();
                    var mesa4hCut = mesa4h.TakeLast(longMinHistoryCount).ToList();
                    var mesa1driverDataPoints = new List<MESADataPoint>();
                    var mesa1minDataPoints = new List<MESADataPoint>();
                    var mesa5minDataPoints = new List<MESADataPoint>();
                    var mesa15minDataPoints = new List<MESADataPoint>();
                    var mesa1hDataPoints = new List<MESADataPoint>();
                    var mesa4hDataPoints = new List<MESADataPoint>();

                    var minuteTimesCut = minHistory.Bars.TakeLast(longMinHistoryCount).Select(_ => _.Timestamp).ToList();
                    for (var i = 0; i < minuteTimesCut.Count; i++)
                    {
                        if (minuteTimesCut[i] % (60 * 5) == 0 || i == minuteTimesCut.Count - 1)
                        {
                            mesa1driverDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa1driverCut[i].Fast,
                                s = (float)mesa1driverCut[i].Slow,
                                t = minuteTimesCut[i]
                            });
                            mesa1minDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa1minCut[i].Fast,
                                s = (float)mesa1minCut[i].Slow,
                                t = minuteTimesCut[i]
                            });
                            mesa5minDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa5minCut[i].Fast,
                                s = (float)mesa5minCut[i].Slow,
                                t = minuteTimesCut[i]
                            });
                            mesa15minDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa15minCut[i].Fast,
                                s = (float)mesa15minCut[i].Slow,
                                t = minuteTimesCut[i]
                            });
                        }

                        if (minuteTimesCut[i] % (60 * 10) == 0 || i == minuteTimesCut.Count - 1)
                        {
                            mesa1hDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa1hCut[i].Fast,
                                s = (float)mesa1hCut[i].Slow,
                                t = minuteTimesCut[i]
                            });
                            mesa4hDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa4hCut[i].Fast,
                                s = (float)mesa4hCut[i].Slow,
                                t = minuteTimesCut[i]
                            });
                        }
                    }

                    var hourTfCount = 360;
                    var hourTimesCut = hourlyHistory.Bars.TakeLast(hourTfCount).Select(_ => _.Timestamp).ToList();
                    var mesa1dCut = mesa1d.TakeLast(hourTfCount).ToList();
                    var mesa1dDataPoints = new List<MESADataPoint>();

                    for (var i = 0; i < hourTimesCut.Count; i++)
                    {
                        mesa1dDataPoints.Add(new MESADataPoint
                        {
                            f = (float)mesa1dCut[i].Fast,
                            s = (float)mesa1dCut[i].Slow,
                            t = hourTimesCut[i]
                        });
                    }


                    var symbol = minHistory.Symbol;
                    var datafeed = minHistory.Datafeed;

                    // if (symbol == "BTC_USD" && datafeed == "Oanda") {
                    //     datafeed = "Binance";
                    //     symbol = "BTCUSDT";
                    // }

                    // if (symbol == "ETH_USD" && datafeed == "Oanda") {
                    //     datafeed = "Binance";
                    //     symbol = "ETHUSDT";
                    // }

                    var key = datafeed + "_" + symbol;
                    var mesaDataPointsMap = new Dictionary<int, List<MESADataPoint>>();
                    mesaDataPointsMap.Add(1, mesa1driverDataPoints);
                    mesaDataPointsMap.Add(60, mesa1minDataPoints);
                    mesaDataPointsMap.Add(300, mesa5minDataPoints);
                    mesaDataPointsMap.Add(900, mesa15minDataPoints);
                    mesaDataPointsMap.Add(3600, mesa1hDataPoints);
                    mesaDataPointsMap.Add(14400, mesa4hDataPoints);
                    mesaDataPointsMap.Add(86400, mesa1dDataPoints);
                    await SetMinuteMesaCache(mesaDataPointsMap, key);

                    // var task1 = SetMinuteMesaCache(mesa1driverDataPoints, key + "_1");
                    // var task2 = SetMinuteMesaCache(mesa1minDataPoints, key + "_60");
                    // var task3 = SetMinuteMesaCache(mesa5minDataPoints, key + "_300");
                    // var task4 = SetMinuteMesaCache(mesa15minDataPoints, key + "_900");
                    // var task5 = SetMinuteMesaCache(mesa1hDataPoints, key + "_3600");
                    // var task6 = SetMinuteMesaCache(mesa4hDataPoints, key + "_14400");
                    // var task7 = SetMinuteMesaCache(mesa1dDataPoints, key + "_86400");
                    // await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7);
                    count++;

                    var tfSummary = new Dictionary<int, MESADataPoint>();
                    tfSummary.Add(1, mesa1driverDataPoints.LastOrDefault());
                    tfSummary.Add(60, mesa1minDataPoints.LastOrDefault());
                    tfSummary.Add(300, mesa5minDataPoints.LastOrDefault());
                    tfSummary.Add(900, mesa15minDataPoints.LastOrDefault());
                    tfSummary.Add(3600, mesa1hDataPoints.LastOrDefault());
                    tfSummary.Add(14400, mesa4hDataPoints.LastOrDefault());
                    tfSummary.Add(86400, mesa1dDataPoints.LastOrDefault());

                    var tfAvgSummary = new Dictionary<int, float>();
                    tfAvgSummary.Add(1, (float)mesa1driver.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa1driver.Length);
                    tfAvgSummary.Add(60, (float)mesa1min.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa1min.Length);
                    tfAvgSummary.Add(300, (float)mesa5min.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa5min.Length);
                    tfAvgSummary.Add(900, (float)mesa15min.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa15min.Length);
                    tfAvgSummary.Add(3600, (float)mesa1h.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa1h.Length);
                    tfAvgSummary.Add(14400, (float)mesa4h.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa4h.Length);
                    tfAvgSummary.Add(86400, (float)mesa1d.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa1d.Length);

                    var totalStrength = 0f;
                    var timeframeStrengths = new Dictionary<int, float>();
                    var d = mesa1driverDataPoints.LastOrDefault();
                    var ast = tfAvgSummary[1];
                    if (d != null && ast > 0)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        totalStrength += currentStrength * 0.033f;
                        timeframeStrengths.Add(1, currentStrength);
                    }

                    d = mesa1minDataPoints.LastOrDefault();
                    ast = tfAvgSummary[60];
                    if (d != null)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        totalStrength += currentStrength * 0.066f;
                        timeframeStrengths.Add(60, currentStrength);
                    }

                    d = mesa5minDataPoints.LastOrDefault();
                    ast = tfAvgSummary[300];
                    if (d != null)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        totalStrength += currentStrength * 0.1f;
                        timeframeStrengths.Add(300, currentStrength);
                    }

                    d = mesa15minDataPoints.LastOrDefault();
                    ast = tfAvgSummary[900];
                    if (d != null)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        totalStrength += currentStrength * 0.15f;
                        timeframeStrengths.Add(900, currentStrength);
                    }

                    d = mesa1hDataPoints.LastOrDefault();
                    ast = tfAvgSummary[3600];
                    if (d != null)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        totalStrength += currentStrength * 0.2f;
                        timeframeStrengths.Add(3600, currentStrength);
                    }

                    d = mesa4hDataPoints.LastOrDefault();
                    ast = tfAvgSummary[14400];
                    if (d != null)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        totalStrength += currentStrength * 0.2f;
                        timeframeStrengths.Add(14400, currentStrength);
                    }

                    d = mesa1dDataPoints.LastOrDefault();
                    ast = tfAvgSummary[86400];
                    if (d != null)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        totalStrength += currentStrength * 0.25f;
                        timeframeStrengths.Add(86400, currentStrength);
                    }

                    var length = calculation_input.Count();

                    summary.Add(new MESADataSummary
                    {
                        Symbol = symbol,
                        Datafeed = datafeed,
                        Strength = tfSummary,
                        AvgStrength = tfAvgSummary,
                        TimeframeStrengths = timeframeStrengths,
                        TotalStrength = totalStrength,
                        LastPrice = (float)calculation_input.LastOrDefault(),
                        Price60 = (float)calculation_input.ElementAt(length - 1),
                        Price300 = (float)calculation_input.ElementAt(length - 5),
                        Price900 = (float)calculation_input.ElementAt(length - 15),
                        Price3600 = (float)calculation_input.ElementAt(length - 60),
                        Price14400 = (float)calculation_input.ElementAt(length - 240),
                        Price86400 = (float)calculation_input.ElementAt(length - 1440),
                    });

                }
                catch (Exception ex)
                {
                    Console.WriteLine(">>> " + minHistory.Symbol);
                    Console.WriteLine(ex);
                }
            }

            try
            {
                await SetMesaSummaryCache(summary);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            stopWatch.Stop();
            TimeSpan ts1 = stopWatch.Elapsed;
            string elapsedTime1 = String.Format(" * 1 min MESA calculation {0:00}:{1:00} - instruments count " + count, ts1.Minutes, ts1.Seconds);
            Console.WriteLine(">>> " + elapsedTime1);
        }

        private async Task SetMinuteMesaCache(Dictionary<int, List<MESADataPoint>> mesa, string key)
        {
            try
            {
                await _cache.SetAsync(_mesaCachePrefix, key.ToLower(), mesa, TimeSpan.FromDays(1));
            }
            catch (Exception ex) { }
        }

        private async Task SetMesaSummaryCache(List<MESADataSummary> mesa)
        {
            try
            {
                await _cache.SetAsync(cachePrefix(), "mesa_data_summary", mesa, TimeSpan.FromDays(1));
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

                if (!scanSymbol(dailyHistory.Symbol))
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

        protected abstract bool scanSymbol(string symbol);
    }
}
