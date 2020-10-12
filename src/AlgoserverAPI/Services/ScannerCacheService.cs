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
            lock (_scannerResults) {
                res = _scannerResults.ToList();
            }
            return new ScannerResponse {
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

            foreach (var dailyHistory in _1Day) {
                var response = new ScanInstrumentResponse
                {
                    trend = Trend.Undefined
                };
                var calculation_input = dailyHistory.Bars.Select(_ => _.Close).ToList();
                var trendData = TrendDetector.CalculateByMesa(calculation_input);
                if (trendData == Trend.Undefined)
                {
                    continue;
                }

                if (_15Mins.TryGetValue(_historyService.GetKey(dailyHistory), out var history15Min)) {
                    var scanningResult = this.scanData(history15Min, trendData);
                    if (scanningResult != null) {
                        res.Add(_toResponse(scanningResult, history15Min, trendData, TimeframeHelper.MIN15_GRANULARITY));
                    }
                } 
                
                if (_1Hour.TryGetValue(_historyService.GetKey(dailyHistory), out var history1H)) {
                    var scanningResult = this.scanData(history1H, trendData);
                    if (scanningResult != null) {
                        res.Add(_toResponse(scanningResult, history1H, trendData, TimeframeHelper.HOURLY_GRANULARITY));
                    }
                }
                
                if (_4Hour.TryGetValue(_historyService.GetKey(dailyHistory), out var history4H)) {
                    var scanningResult = this.scanData(history4H, trendData);
                    if (scanningResult != null) {
                        res.Add(_toResponse(scanningResult, history4H, trendData, TimeframeHelper.HOUR4_GRANULARITY));
                    }
                }
            }

            lock (_scannerResults) {
                _scannerResults.Clear();
                _scannerResults.AddRange(res);
            }

            data_count_15_min = _15Mins.Count;
            data_count_1_h = _1Hour.Count;
            data_count_4_h = _4Hour.Count;
            data_count_1_d = _1Day.Count;
            scanning_time = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        private ScannerResponseItem _toResponse(ScanResponse response, HistoryData data, Trend trend, int timeframe) {
            var lastBar = data.Bars.LastOrDefault();
            return new ScannerResponseItem {
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

        // private async Task<ScanInstrumentResponse> scanInstrument(ScanInstrumentRequest req)
        // {
        //     var response = new ScanInstrumentResponse
        //     {
        //         trend = Trend.Undefined
        //     };

        //     var Exchange = req.Instrument.Exchange.ToLowerInvariant();
        //     var Datafeed = req.Instrument.Datafeed.ToLowerInvariant();
        //     var Type = req.Instrument.Type.ToLowerInvariant();
        //     var Symbol = req.Instrument.Id;

        //     var dailyPriceData = await _historyService.GetHistory(Symbol, TimeframeHelper.DAILY_GRANULARITY, Datafeed, Exchange, Type);
        //     var calculation_input = dailyPriceData.Bars.Select(_ => _.Close).ToList();
        //     var trendData = TrendDetector.CalculateByMesa(calculation_input);
        //     if (trendData == Trend.Undefined)
        //     {
        //         return response;
        //     }

        //     var hour4 = _historyService.GetHistory(Symbol, TimeframeHelper.HOUR4_GRANULARITY, Datafeed, Exchange, Type);
        //     var hourly = _historyService.GetHistory(Symbol, TimeframeHelper.HOURLY_GRANULARITY, Datafeed, Exchange, Type);
        //     var min15 = _historyService.GetHistory(Symbol, TimeframeHelper.MIN15_GRANULARITY, Datafeed, Exchange, Type);
        //     var task = await Task.WhenAll<HistoryData>(new[] { hour4, hourly, min15 });
        //     var hour4PriceData = task[0];
        //     var hourlyPriceData = task[1];
        //     var min15PriceData = task[2];

        //     var hour4ScanningResult = this.scanData(hour4PriceData, trendData);
        //     var hourlyScanningResult = this.scanData(hourlyPriceData, trendData);
        //     var min15ScanningResult = this.scanData(min15PriceData, trendData);

        //     if (hour4ScanningResult != null) {
        //         response.tte_240 = hour4ScanningResult.tte;
        //         response.tp_240 = hour4ScanningResult.tp;
        //     }
        //     if (hourlyScanningResult != null) {
        //         response.tte_60 = hourlyScanningResult.tte;
        //         response.tp_60 = hourlyScanningResult.tp;
        //     }
        //     if (min15ScanningResult != null) {
        //         response.tte_15 = min15ScanningResult.tte;
        //         response.tp_15 = min15ScanningResult.tp;
        //     }

        //     response.trend = trendData; 
        //     return response;
        // }

        private  ScanResponse scanData(HistoryData history, Trend trend) {
            var lastBar = history.Bars.LastOrDefault();
            if (lastBar == null) {
                return null;
            }

            var high = history.Bars.Select(_ => _.High).ToList();
            var low = history.Bars.Select(_ => _.Low).ToList();
            var close = history.Bars.Select(_ => _.Close).ToList();
            var levels = TechCalculations.CalculateLevel128(high, low);

            var resistance = levels.EightEight;
            var natural = levels.FourEight;
            var support = levels.ZeroEight;

            // check is price above/below natural level
            if (trend == Trend.Up && lastBar.Close > (natural * 2 + support) / 3) {
                return null;
            }
            
            if (trend == Trend.Down && lastBar.Close < (natural + resistance * 2) / 3) {
                return null;
            }

            var directionApproved = TechCalculations.ApproveDirection(high, low, close, trend);

            if (!directionApproved) {
                return null;
            }

            var candlesPerformance = TechCalculations.CalculatePriceMoveDirection(high, low, close, trend);
            var priceDiffToHit = 0m;

            // check is price go needed direction
            if (trend == Trend.Up) {
                if (candlesPerformance > 0) {
                    return null;
                }

                priceDiffToHit = lastBar.Close - support;
            }
            
            if (trend == Trend.Down) {
                if (candlesPerformance < 0) {
                    return null;
                }
                priceDiffToHit = resistance - lastBar.Close;
            }

            // if (priceDiffToHit <= 0) {
            //     return null;
            // }

            var length = 14;
            var lastDeviation = 3;
            var deviation = TechCalculations.StdDev(close.TakeLast(200).ToList(), length);
            var avgDeviation = deviation.Sum() / deviation.Count;
            var currentDeviation = deviation.TakeLast(lastDeviation).Sum() / lastDeviation;
            var deviationSpeed = Math.Round((currentDeviation - avgDeviation) / avgDeviation * 100, 0);
            var candlesToHit = Math.Round(priceDiffToHit / Math.Abs(candlesPerformance), 0);

            if (candlesToHit <= 0) {
                candlesToHit = 1;
            }

            return new ScanResponse {
                tte = (int)candlesToHit,
                tp = (int)deviationSpeed
            };
        }
    }
}
