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

        public void ScanMarkets()
        {
            var _15Mins = _historyService.Get15MinDataDictionary();
            var _1Hour = _historyService.Get1HDataDictionary();
            var _4Hour = _historyService.Get4HDataDictionary();
            var _1Day = _historyService.Get1DData();
            var res = new List<ScannerResponseItem>();

            foreach (var dailyHistory in _1Day)
            {
                var response = new ScanInstrumentResponse
                {
                    trend = Trend.Undefined,
                    type = TradeType.EXT
                };
                var calculation_input = dailyHistory.Bars.Select(_ => _.Close).ToList();
                var trendData = TrendDetector.CalculateByMesaWithTrendAdjusted(calculation_input);
                if (trendData == Trend.Undefined)
                {
                    continue;
                }

                if (_15Mins.TryGetValue(_historyService.GetKey(dailyHistory), out var history15Min))
                {
                    var scanningResult = _scanner.ScanExt(history15Min, trendData);
                    if (scanningResult != null)
                    {
                        res.Add(_toResponse(scanningResult, history15Min, trendData, TimeframeHelper.MIN15_GRANULARITY));
                    }
                }

                if (_1Hour.TryGetValue(_historyService.GetKey(dailyHistory), out var history1H))
                {
                    var scanningResult = _scanner.ScanExt(history1H, trendData);
                    if (scanningResult != null)
                    {
                        res.Add(_toResponse(scanningResult, history1H, trendData, TimeframeHelper.HOURLY_GRANULARITY));
                    }
                }

                if (_4Hour.TryGetValue(_historyService.GetKey(dailyHistory), out var history4H))
                {
                    var scanningResult = _scanner.ScanExt(history4H, trendData);
                    if (scanningResult != null)
                    {
                        res.Add(_toResponse(scanningResult, history4H, trendData, TimeframeHelper.HOUR4_GRANULARITY));
                    }
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

        private ScannerResponseItem _toResponse(ScanResponse response, HistoryData data, Trend trend, int timeframe)
        {
            var lastBar = data.Bars.LastOrDefault();
            return new ScannerResponseItem
            {
                exchange = data.Exchange,
                symbol = data.Symbol,
                timeframe = timeframe,
                trend = trend,
                tp = response.tp,
                tte = response.tte,
                open = lastBar.Open,
                high = lastBar.High,
                low = lastBar.Low,
                close = lastBar.Close
            };
        }
    }
}
