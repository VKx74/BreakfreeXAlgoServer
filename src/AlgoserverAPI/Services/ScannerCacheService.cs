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
    public class MesaSummaryInfo
    {
        public Dictionary<string, Dictionary<int, List<MESADataPoint>>> MesaDataPoints { get; set; }
        public List<MESADataSummary> MesaSummary { get; set; }
    }

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
            if (_cache.TryGetValue(cachePrefix(), "mesa_data_summary", out List<MESADataSummary> cachedResponse))
            {
                return cachedResponse.ToList();
            }

            return new List<MESADataSummary>();
        }

        public Dictionary<int, List<MESADataPoint>> GetMinuteMesaCache(String key)
        {
            if (_cache.TryGetValue(_mesaCachePrefix, key, out Dictionary<int, List<MESADataPoint>> cachedResponse))
            {
                return cachedResponse;
            }

            return new Dictionary<int, List<MESADataPoint>>();
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

        public MesaSummaryInfo CalculateMinuteMesa()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var _1Mins = _historyService.Get1MinLongData();
            var _5Mins = _historyService.Get5MinData();
            var _15Mins = _historyService.Get15MinData();
            var _1Hour = _historyService.Get1HData();
            var _4Hour = _historyService.Get4HData();
            var _1Day = _historyService.Get1DData();
            var count = 0;
            var summary = new List<MESADataSummary>();
            var mesaDataPoints = new Dictionary<string, Dictionary<int, List<MESADataPoint>>>();

            if (_1Hour == null || _1Mins == null || _1Day == null)
            {
                return null;
            }

            Console.WriteLine(">>> MESA Counts: " + _1Mins.Count + " - " + _1Hour.Count + " - " + _1Day.Count);

            foreach (var minHistory in _1Mins)
            {
                try
                {
                    if (minHistory == null || minHistory.Bars == null)
                    {
                        Console.WriteLine(">>> MESA Calculation Error (minHistory)");
                        continue;
                    }

                    var calculation_input = minHistory.Bars.Select(_ => _.Close);
                    if (calculation_input.Count() < 45000)
                    {
                        Console.WriteLine(">>> MESA Calculation Error (calculation_input) - " + minHistory.Symbol);
                        continue;
                    }

                    var hourlyHistory = _1Hour.FirstOrDefault((_) => String.Equals(_.Symbol, minHistory.Symbol, StringComparison.InvariantCultureIgnoreCase) && String.Equals(_.Exchange, minHistory.Exchange, StringComparison.InvariantCultureIgnoreCase));
                    if (hourlyHistory == null || hourlyHistory.Bars == null || !hourlyHistory.Bars.Any())
                    {
                        Console.WriteLine(">>> MESA Calculation Error (hourlyHistory)");
                        continue;
                    }

                    var dailyHistory = _1Day.FirstOrDefault((_) => String.Equals(_.Symbol, minHistory.Symbol, StringComparison.InvariantCultureIgnoreCase) && String.Equals(_.Exchange, minHistory.Exchange, StringComparison.InvariantCultureIgnoreCase));
                    if (dailyHistory == null || dailyHistory.Bars == null || !dailyHistory.Bars.Any())
                    {
                        Console.WriteLine(">>> MESA Calculation Error (dailyHistory)");
                        continue;
                    }

                    var hourly_calculation_input = hourlyHistory.Bars.Select(_ => _.Close);
                    var daily_calculation_input = dailyHistory.Bars.Select(_ => _.Close);

                    var mesa1driver = TechCalculations.MESA(calculation_input.TakeLast(28000).ToList(), 0.0325, 0.0325);
                    var mesa1min = TechCalculations.MESA(calculation_input.TakeLast(32000).ToList(), 0.0085, 0.0085);
                    var mesa5min = TechCalculations.MESA(calculation_input.TakeLast(36000).ToList(), 0.0032, 0.0032);
                    var mesa15min = TechCalculations.MESA(calculation_input.TakeLast(40000).ToList(), 0.0012, 0.0012);
                    var mesa1h = TechCalculations.MESA(calculation_input.TakeLast(44000).ToList(), 0.0007, 0.0007);
                    var mesa4h = TechCalculations.MESA(calculation_input.TakeLast(50000).ToList(), 0.00039, 0.00039);
                    var mesa1d = TechCalculations.MESA(hourly_calculation_input.TakeLast(3000).ToList(), 0.0085, 0.0085);
                    var mesa1month = TechCalculations.MESA(daily_calculation_input.TakeLast(5000).ToList(), 0.09, 0.09);
                    var mesa1year = TechCalculations.MESA(daily_calculation_input.TakeLast(5000).ToList(), 0.0085, 0.0085);

                    var volatilityCalculationData = minHistory.Bars.TakeLast(28000);
                    var volDriver = CalculateVolatility(volatilityCalculationData, 14);
                    var vol1min = CalculateVolatility(volatilityCalculationData, 33);
                    var vol5min = CalculateVolatility(volatilityCalculationData, 33 * 3);
                    var vol15min = CalculateVolatility(volatilityCalculationData, 33 * 9);
                    var vol1h = CalculateVolatility(volatilityCalculationData, 33 * 25);
                    var vol4h = CalculateVolatility(volatilityCalculationData, 33 * 50);
                    var vol1d = CalculateVolatility(hourlyHistory.Bars, 66);
                    var vol1month = CalculateVolatility(dailyHistory.Bars, 33);

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
                            var tt = minuteTimesCut[i];
                            mesa1driverDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa1driverCut[i].Fast,
                                s = (float)mesa1driverCut[i].Slow,
                                t = (uint)tt,
                                v = volDriver.GetValueOrDefault(tt, 0)
                            });
                            mesa1minDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa1minCut[i].Fast,
                                s = (float)mesa1minCut[i].Slow,
                                t = (uint)tt,
                                v = vol1min.GetValueOrDefault(tt, 0)
                            });
                            mesa5minDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa5minCut[i].Fast,
                                s = (float)mesa5minCut[i].Slow,
                                t = (uint)tt,
                                v = vol5min.GetValueOrDefault(tt, 0)
                            });
                            mesa15minDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa15minCut[i].Fast,
                                s = (float)mesa15minCut[i].Slow,
                                t = (uint)tt,
                                v = vol15min.GetValueOrDefault(tt, 0)
                            });
                            mesa1hDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa1hCut[i].Fast,
                                s = (float)mesa1hCut[i].Slow,
                                t = (uint)tt,
                                v = vol1h.GetValueOrDefault(tt, 0)
                            });
                            mesa4hDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa4hCut[i].Fast,
                                s = (float)mesa4hCut[i].Slow,
                                t = (uint)tt,
                                v = vol4h.GetValueOrDefault(tt, 0)
                            });
                        }
                    }

                    var hourTfCount = Math.Min(3000, hourlyHistory.Bars.Count - 1000);
                    var hourTimesCut = hourlyHistory.Bars.TakeLast(hourTfCount).Select(_ => _.Timestamp).ToList();
                    var mesa1dCut = mesa1d.TakeLast(hourTfCount).ToList();
                    var mesa1dDataPoints = new List<MESADataPoint>();

                    for (var i = 0; i < hourTimesCut.Count; i++)
                    {
                        // if (hourTimesCut[i] % (60 * 60 * 12) == 0 || i == hourTimesCut.Count - 1)
                        // {
                            var tt = hourTimesCut[i];
                            mesa1dDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa1dCut[i].Fast,
                                s = (float)mesa1dCut[i].Slow,
                                t = (uint)tt,
                                v = vol1d.GetValueOrDefault(tt, 0)
                            });
                        // }
                    }

                    var dailyTfCount = Math.Min(5000, dailyHistory.Bars.Count - 1000);
                    var dailyTimesCut = dailyHistory.Bars.TakeLast(dailyTfCount).Select(_ => _.Timestamp).ToList();
                    var mesa1monthCut = mesa1month.TakeLast(dailyTfCount).ToList();
                    var mesa1yearCut = mesa1year.TakeLast(dailyTfCount).ToList();
                    var mesa1monthDataPoints = new List<MESADataPoint>();
                    var mesa1yearDataPoints = new List<MESADataPoint>();

                    for (var i = 0; i < dailyTimesCut.Count; i++)
                    {
                        // if (dailyTimesCut[i] % (60 * 60 * 24 * 10) == 0 || i == dailyTimesCut.Count - 1)
                        // {
                            var tt = dailyTimesCut[i];
                            mesa1monthDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa1monthCut[i].Fast,
                                s = (float)mesa1monthCut[i].Slow,
                                t = (uint)tt,
                                v = vol1month.GetValueOrDefault(tt, 0)
                            });
                            mesa1yearDataPoints.Add(new MESADataPoint
                            {
                                f = (float)mesa1yearCut[i].Fast,
                                s = (float)mesa1yearCut[i].Slow,
                                t = (uint)tt
                            });
                        // }
                    }


                    var symbol = minHistory.Symbol;
                    var datafeed = minHistory.Datafeed;

                    var key = datafeed + "_" + symbol;
                    var mesaDataPointsMap = new Dictionary<int, List<MESADataPoint>>();
                    mesaDataPointsMap.Add(TimeframeHelper.DRIVER_GRANULARITY, mesa1driverDataPoints);
                    mesaDataPointsMap.Add(TimeframeHelper.MIN1_GRANULARITY, mesa1minDataPoints);
                    mesaDataPointsMap.Add(TimeframeHelper.MIN5_GRANULARITY, mesa5minDataPoints);
                    mesaDataPointsMap.Add(TimeframeHelper.MIN15_GRANULARITY, mesa15minDataPoints);
                    mesaDataPointsMap.Add(TimeframeHelper.HOURLY_GRANULARITY, mesa1hDataPoints);
                    mesaDataPointsMap.Add(TimeframeHelper.HOUR4_GRANULARITY, mesa4hDataPoints);
                    mesaDataPointsMap.Add(TimeframeHelper.DAILY_GRANULARITY, mesa1dDataPoints);
                    mesaDataPointsMap.Add(TimeframeHelper.MONTHLY_GRANULARITY, mesa1monthDataPoints);
                    mesaDataPointsMap.Add(TimeframeHelper.YEARLY_GRANULARITY, mesa1yearDataPoints);
                    mesaDataPoints.Add(key, mesaDataPointsMap);
                    SetMinuteMesaCache(mesaDataPointsMap, key);

                    count++;

                    var tfSummary = new Dictionary<int, MESADataPoint>();
                    tfSummary.Add(TimeframeHelper.DRIVER_GRANULARITY, mesa1driverDataPoints.LastOrDefault());
                    tfSummary.Add(TimeframeHelper.MIN1_GRANULARITY, mesa1minDataPoints.LastOrDefault());
                    tfSummary.Add(TimeframeHelper.MIN5_GRANULARITY, mesa5minDataPoints.LastOrDefault());
                    tfSummary.Add(TimeframeHelper.MIN15_GRANULARITY, mesa15minDataPoints.LastOrDefault());
                    tfSummary.Add(TimeframeHelper.HOURLY_GRANULARITY, mesa1hDataPoints.LastOrDefault());
                    tfSummary.Add(TimeframeHelper.HOUR4_GRANULARITY, mesa4hDataPoints.LastOrDefault());
                    tfSummary.Add(TimeframeHelper.DAILY_GRANULARITY, mesa1dDataPoints.LastOrDefault());
                    tfSummary.Add(TimeframeHelper.MONTHLY_GRANULARITY, mesa1monthDataPoints.LastOrDefault());
                    tfSummary.Add(TimeframeHelper.YEARLY_GRANULARITY, mesa1yearDataPoints.LastOrDefault());

                    var tfAvgSummary = new Dictionary<int, float>();
                    tfAvgSummary.Add(TimeframeHelper.DRIVER_GRANULARITY, (float)mesa1driver.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa1driver.Length);
                    tfAvgSummary.Add(TimeframeHelper.MIN1_GRANULARITY, (float)mesa1min.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa1min.Length);
                    tfAvgSummary.Add(TimeframeHelper.MIN5_GRANULARITY, (float)mesa5min.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa5min.Length);
                    tfAvgSummary.Add(TimeframeHelper.MIN15_GRANULARITY, (float)mesa15min.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa15min.Length);
                    tfAvgSummary.Add(TimeframeHelper.HOURLY_GRANULARITY, (float)mesa1h.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa1h.Length);
                    tfAvgSummary.Add(TimeframeHelper.HOUR4_GRANULARITY, (float)mesa4h.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa4h.Length);
                    tfAvgSummary.Add(TimeframeHelper.DAILY_GRANULARITY, (float)mesa1d.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa1d.Length);
                    tfAvgSummary.Add(TimeframeHelper.MONTHLY_GRANULARITY, (float)mesa1month.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa1month.Length);
                    tfAvgSummary.Add(TimeframeHelper.YEARLY_GRANULARITY, (float)mesa1year.Select((_) => Math.Abs(_.Fast - _.Slow)).Sum() / mesa1year.Length);

                    var totalStrength = 0f;
                    var timeframeStrengths = new Dictionary<int, float>();
                    var d = tfSummary.GetValueOrDefault(TimeframeHelper.DRIVER_GRANULARITY);
                    var ast = tfAvgSummary.GetValueOrDefault(TimeframeHelper.DRIVER_GRANULARITY);
                    if (d != null && ast > 0)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        totalStrength += currentStrength * 0.0066f;
                        timeframeStrengths.Add(TimeframeHelper.DRIVER_GRANULARITY, currentStrength);
                    }
                    else
                    {
                        timeframeStrengths.Add(TimeframeHelper.DRIVER_GRANULARITY, 0);
                    }

                    d = tfSummary.GetValueOrDefault(TimeframeHelper.MIN1_GRANULARITY);
                    ast = tfAvgSummary.GetValueOrDefault(TimeframeHelper.MIN1_GRANULARITY);
                    if (d != null && ast > 0)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        totalStrength += currentStrength * 0.0274f;
                        timeframeStrengths.Add(TimeframeHelper.MIN1_GRANULARITY, currentStrength);
                    }
                    else
                    {
                        timeframeStrengths.Add(TimeframeHelper.MIN1_GRANULARITY, 0);
                    }

                    d = tfSummary.GetValueOrDefault(TimeframeHelper.MIN5_GRANULARITY);
                    ast = tfAvgSummary.GetValueOrDefault(TimeframeHelper.MIN5_GRANULARITY);
                    if (d != null && ast > 0)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        totalStrength += currentStrength * 0.066f;
                        timeframeStrengths.Add(TimeframeHelper.MIN5_GRANULARITY, currentStrength);
                    }
                    else
                    {
                        timeframeStrengths.Add(TimeframeHelper.MIN5_GRANULARITY, 0);
                    }

                    d = tfSummary.GetValueOrDefault(TimeframeHelper.MIN15_GRANULARITY);
                    ast = tfAvgSummary.GetValueOrDefault(TimeframeHelper.MIN15_GRANULARITY);
                    if (d != null && ast > 0)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        totalStrength += currentStrength * 0.1f;
                        timeframeStrengths.Add(TimeframeHelper.MIN15_GRANULARITY, currentStrength);
                    }
                    else
                    {
                        timeframeStrengths.Add(TimeframeHelper.MIN15_GRANULARITY, 0);
                    }

                    var hour1Strength = 0;
                    d = tfSummary.GetValueOrDefault(TimeframeHelper.HOURLY_GRANULARITY);
                    ast = tfAvgSummary.GetValueOrDefault(TimeframeHelper.HOURLY_GRANULARITY);
                    if (d != null && ast > 0)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        var stateData = mesa1hDataPoints.Select((_) => (_.f - _.s) / ast * 100).ToList();
                        hour1Strength = TechCalculations.MeasureTrendState(stateData, 5);
                        totalStrength += currentStrength * 0.18f;
                        timeframeStrengths.Add(TimeframeHelper.HOURLY_GRANULARITY, currentStrength);
                    }
                    else
                    {
                        timeframeStrengths.Add(TimeframeHelper.HOURLY_GRANULARITY, 0);
                    }

                    var hour4Strength = 0;
                    d = tfSummary.GetValueOrDefault(TimeframeHelper.HOUR4_GRANULARITY);
                    ast = tfAvgSummary.GetValueOrDefault(TimeframeHelper.HOUR4_GRANULARITY);
                    if (d != null && ast > 0)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        var stateData = mesa4hDataPoints.Select((_) => (_.f - _.s) / ast * 100).ToList();
                        hour4Strength = TechCalculations.MeasureTrendState(stateData, 5);
                        totalStrength += currentStrength * 0.18f;
                        timeframeStrengths.Add(TimeframeHelper.HOUR4_GRANULARITY, currentStrength);
                    }
                    else
                    {
                        timeframeStrengths.Add(TimeframeHelper.HOUR4_GRANULARITY, 0);
                    }

                    var dailyStrength = 0;
                    d = tfSummary.GetValueOrDefault(TimeframeHelper.DAILY_GRANULARITY);
                    ast = tfAvgSummary.GetValueOrDefault(TimeframeHelper.DAILY_GRANULARITY);
                    if (d != null && ast > 0)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        var stateData = mesa1dDataPoints.Select((_) => (_.f - _.s) / ast * 100).ToList();
                        dailyStrength = TechCalculations.MeasureTrendState(stateData, 5);
                        totalStrength += currentStrength * 0.18f;
                        timeframeStrengths.Add(TimeframeHelper.DAILY_GRANULARITY, currentStrength);
                    }
                    else
                    {
                        timeframeStrengths.Add(TimeframeHelper.DAILY_GRANULARITY, 0);
                    }

                    var monthlyStrength = 0;
                    d = tfSummary.GetValueOrDefault(TimeframeHelper.MONTHLY_GRANULARITY);
                    ast = tfAvgSummary.GetValueOrDefault(TimeframeHelper.MONTHLY_GRANULARITY);
                    if (d != null && ast > 0)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        var stateData = mesa1monthDataPoints.Select((_) => (_.f - _.s) / ast * 100).ToList();
                        monthlyStrength = TechCalculations.MeasureTrendState(stateData, 5);
                        totalStrength += currentStrength * 0.1f;
                        timeframeStrengths.Add(TimeframeHelper.MONTHLY_GRANULARITY, currentStrength);
                    }
                    else
                    {
                        timeframeStrengths.Add(TimeframeHelper.MONTHLY_GRANULARITY, 0);
                    }

                    var yearlyStrength = 0;
                    d = tfSummary.GetValueOrDefault(TimeframeHelper.YEARLY_GRANULARITY);
                    ast = tfAvgSummary.GetValueOrDefault(TimeframeHelper.YEARLY_GRANULARITY);
                    if (d != null && ast > 0)
                    {
                        var currentStrength = (d.f - d.s) / ast;
                        var stateData = mesa1yearDataPoints.Select((_) => (_.f - _.s) / ast * 100).ToList();
                        yearlyStrength = TechCalculations.MeasureTrendState(stateData, 5);
                        totalStrength += currentStrength * 0.06f;
                        timeframeStrengths.Add(TimeframeHelper.YEARLY_GRANULARITY, currentStrength);
                    }
                    else
                    {
                        timeframeStrengths.Add(TimeframeHelper.YEARLY_GRANULARITY, 0);
                    }

                    var length = calculation_input.Count();

                    if (hour1Strength > 0 && hour4Strength > 0 && dailyStrength > 0 && monthlyStrength > 0 && yearlyStrength > 0)
                    {
                        if (hour1Strength == 2)
                        {
                            totalStrength += totalStrength > 0 ? 0.01f : -0.01f;
                        }
                        if (hour4Strength == 2)
                        {
                            totalStrength += totalStrength > 0 ? 0.02f : -0.02f;
                        }
                        if (dailyStrength == 2)
                        {
                            totalStrength += totalStrength > 0 ? 0.03f : -0.03f;
                        }
                        if (monthlyStrength == 2)
                        {
                            totalStrength += totalStrength > 0 ? 0.04f : -0.04f;
                        }
                    }

                    var volatility = new Dictionary<int, float>();
                    volatility.Add(TimeframeHelper.DRIVER_GRANULARITY, volDriver.LastOrDefault().Value);
                    volatility.Add(TimeframeHelper.MIN1_GRANULARITY, vol1min.LastOrDefault().Value);
                    volatility.Add(TimeframeHelper.MIN5_GRANULARITY, vol5min.LastOrDefault().Value);
                    volatility.Add(TimeframeHelper.MIN15_GRANULARITY, vol15min.LastOrDefault().Value);
                    volatility.Add(TimeframeHelper.HOURLY_GRANULARITY, vol1h.LastOrDefault().Value);
                    volatility.Add(TimeframeHelper.HOUR4_GRANULARITY, vol4h.LastOrDefault().Value);
                    volatility.Add(TimeframeHelper.DAILY_GRANULARITY, vol1d.LastOrDefault().Value);
                    volatility.Add(TimeframeHelper.MONTHLY_GRANULARITY, vol1month.LastOrDefault().Value);

                    summary.Add(new MESADataSummary
                    {
                        Symbol = symbol,
                        Datafeed = datafeed,
                        Strength = tfSummary,
                        AvgStrength = tfAvgSummary,
                        TimeframeStrengths = timeframeStrengths,
                        TotalStrength = totalStrength,
                        Volatility = volatility,
                        LastPrice = (float)calculation_input.LastOrDefault(),
                        Price60 = (float)calculation_input.ElementAt(length - 1),
                        Price300 = (float)calculation_input.ElementAt(length - 5),
                        Price900 = (float)calculation_input.ElementAt(length - 15),
                        Price3600 = (float)calculation_input.ElementAt(length - 60),
                        Price14400 = (float)calculation_input.ElementAt(length - 240),
                        Price86400 = (float)calculation_input.ElementAt(length - 1440),
                        Hour1State = hour1Strength,
                        Hour4State = hour4Strength,
                        DailyState = dailyStrength,
                        MonthlyState = monthlyStrength,
                        YearlyState = yearlyStrength
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
                SetMesaSummaryCache(summary);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            stopWatch.Stop();
            TimeSpan ts1 = stopWatch.Elapsed;
            string elapsedTime1 = String.Format(" * 1 min MESA calculation {0:00}:{1:00} - instruments count " + count, ts1.Minutes, ts1.Seconds);
            Console.WriteLine(">>> " + elapsedTime1);

            return new MesaSummaryInfo
            {
                MesaSummary = summary,
                MesaDataPoints = mesaDataPoints
            };
        }

        private Dictionary<long, float> CalculateVolatility(IEnumerable<BarMessage> bars, int period)
        {
            var dates = bars.Select(_ => _.Timestamp).ToList();
            var input = bars.Select(_ => _.Close);
            var rawStdDev = TechCalculations.StdDevPercentage(input, period);
            var stdDev = rawStdDev.TakeLast(rawStdDev.Count - period).ToList();
            var avgSpread = stdDev.Sum() / stdDev.Count;
            var res = new Dictionary<long, float>();
            for (int i = 0; i < rawStdDev.Count; i++)
            {
                var val = rawStdDev[i];
                var t = dates[i];
                res.Add(t, (float)val / (float)avgSpread * 100);
            }
            return res;
        }

        private void SetMinuteMesaCache(Dictionary<int, List<MESADataPoint>> mesa, string key)
        {
            try
            {
                _cache.Set(_mesaCachePrefix, key.ToLower(), mesa, TimeSpan.FromDays(1));
            }
            catch (Exception ex) { }
        }

        private void SetMesaSummaryCache(List<MESADataSummary> mesa)
        {
            try
            {
                _cache.Set(cachePrefix(), "mesa_data_summary", mesa, TimeSpan.FromDays(1));
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
