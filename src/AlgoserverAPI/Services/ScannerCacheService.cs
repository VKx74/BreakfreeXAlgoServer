using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.Algo;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public class ScannerCacheService
    {
        private readonly ScannerHistoryService _historyService;
        private readonly ScannerService _scanner;
        private readonly List<ScannerResponseItem> _scannerResults = new List<ScannerResponseItem>();
        private readonly List<ScannerResponseHistoryItem> _resultHistory = new List<ScannerResponseHistoryItem>();
        private int scanning_time { get; set; }
        private int data_count_15_min { get; set; }
        private int data_count_1_h { get; set; }
        private int data_count_4_h { get; set; }
        private int data_count_1_d { get; set; }

        public string RefreshAllMarketsTime { get; set; }
        public string RefreshMarketsTime { get; set; }

        public ScannerCacheService(ScannerHistoryService historyService, ScannerService scanner)
        {
            _historyService = historyService;
            _scanner = scanner;
        }

        public ScannerResponse GetData()
        {
            List<ScannerResponseItem> res;
            lock (_scannerResults)
            {
                res = _scannerResults.ToList();
            }
            return new ScannerResponse
            {
                items = res,
                data_count_15_min = data_count_15_min,
                data_count_1_d = data_count_1_d,
                data_count_1_h = data_count_1_h,
                data_count_4_h = data_count_4_h,
                scanning_time = scanning_time,
                refresh_time = RefreshMarketsTime,
                refresh_time_all = RefreshAllMarketsTime
            };
        }

        public ScannerHistoryResponse GetHistoryData()
        {
            List<ScannerResponseHistoryItem> res;
            lock (_resultHistory)
            {
                res = _resultHistory.ToList();
            }
            return new ScannerHistoryResponse
            {
                items = res
            };
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
                if (!dailyHistory.Bars.Any()) {
                    continue;
                }

                var calculation_input = dailyHistory.Bars.Select(_ => _.Close).ToList();
                var extendedTrendData = TrendDetector.CalculateByMesaBy2TrendAdjusted(calculation_input);
                var trendData = TrendDetector.MergeTrends(extendedTrendData);

                if (_15Mins.TryGetValue(_historyService.GetKey(dailyHistory), out var history15Min) && trendData != Trend.Undefined && history15Min.Bars.Any())
                {
                    var scanningResultBRC = _scanner.ScanBRC(_scanner.ToScanningHistory(history15Min.Bars), trendData);
                    if (scanningResultBRC != null)
                    {
                        var resp = _toResponse(scanningResultBRC, history15Min, TimeframeHelper.MIN15_GRANULARITY);
                        _tryAddHistory(resp, scanningResultBRC);
                        res.Add(resp);
                    }

                    var scanningResultExt = _scanner.ScanExt(_scanner.ToScanningHistory(history15Min.Bars), trendData);
                    if (scanningResultExt != null)
                    {
                        var resp = _toResponse(scanningResultExt, history15Min, TimeframeHelper.MIN15_GRANULARITY);
                        _tryAddHistory(resp, scanningResultExt);
                        res.Add(resp);
                    }
                }

                if (_1Hour.TryGetValue(_historyService.GetKey(dailyHistory), out var history1H) && trendData != Trend.Undefined && history1H.Bars.Any())
                {
                    var scanningResultBRC = _scanner.ScanBRC(_scanner.ToScanningHistory(history1H.Bars), trendData);
                    if (scanningResultBRC != null)
                    {
                        var resp = _toResponse(scanningResultBRC, history1H, TimeframeHelper.HOURLY_GRANULARITY);
                        _tryAddHistory(resp, scanningResultBRC);
                        res.Add(resp);
                    }

                    var scanningResultExt = _scanner.ScanExt(_scanner.ToScanningHistory(history1H.Bars), trendData);
                    if (scanningResultExt != null)
                    {
                        var resp = _toResponse(scanningResultExt, history1H, TimeframeHelper.HOURLY_GRANULARITY);
                        _tryAddHistory(resp, scanningResultExt);
                        res.Add(resp);
                    }
                }

                if (_4Hour.TryGetValue(_historyService.GetKey(dailyHistory), out var history4H) && history4H.Bars.Any())
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

                        var scanningResultExt = _scanner.ScanExt(_scanner.ToScanningHistory(history4H.Bars), trendData);
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
                        var swingScannerResult = _scanner.ScanSwing(_scanner.ToScanningHistory(history4H.Bars), extendedTrendData.GlobalTrend, extendedTrendData.LocalTrend);
                        if (swingScannerResult != null)
                        {
                            var resp = _toResponse(swingScannerResult, history4H, TimeframeHelper.HOUR4_GRANULARITY);
                            _tryAddHistory(resp, swingScannerResult);
                            res.Add(resp);
                        }
                    }
                }

                var swingDailyScannerResult = _scanner.ScanSwing(_scanner.ToScanningHistory(dailyHistory.Bars), extendedTrendData.GlobalTrend, extendedTrendData.LocalTrend);
                if (swingDailyScannerResult != null)
                {
                    var resp = _toResponse(swingDailyScannerResult, dailyHistory, TimeframeHelper.DAILY_GRANULARITY);
                    _tryAddHistory(resp, swingDailyScannerResult);
                    res.Add(resp);
                }
            }

            lock (_scannerResults)
            {
                _scannerResults.Clear();
                _scannerResults.AddRange(res);
            }

            data_count_15_min = _15Mins.Count;
            data_count_1_h = _1Hour.Count;
            data_count_4_h = _4Hour.Count;
            data_count_1_d = _1Day.Count;
            scanning_time = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        private ScannerResponseItem _toResponse(ScanResponse response, HistoryData data, int timeframe)
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

        private void _tryAddHistory(ScannerResponseItem item, ScanResponse resp)
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
        }
    }
}
