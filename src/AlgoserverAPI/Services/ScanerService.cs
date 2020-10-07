using System;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.Algo;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    internal class ScanResponse
    {
        public int tte { get; set; }
        public int tp { get; set; }
    }

    public class ScanerService
    {

        private readonly HistoryService _historyService;

        public ScanerService(HistoryService historyService)
        {
            _historyService = historyService;
        }

        internal Task<ScanInstrumentResponse> ScanInstrument(ScanInstrumentRequest req)
        {
            return Task.Run(async () =>
            {
                return await scanInstrument(req);
            });
        }

        private async Task<ScanInstrumentResponse> scanInstrument(ScanInstrumentRequest req)
        {
            var response = new ScanInstrumentResponse
            {
                trend = Trend.Undefined
            };

            var Exchange = req.Instrument.Exchange.ToLowerInvariant();
            var Datafeed = req.Instrument.Datafeed.ToLowerInvariant();
            var Type = req.Instrument.Type.ToLowerInvariant();
            var Symbol = req.Instrument.Id;

            var dailyPriceData = await _historyService.GetHistory(Symbol, TimeframeHelper.DAILY_GRANULARITY, Datafeed, Exchange, Type);
            var calculation_input = dailyPriceData.Bars.Select(_ => _.Close).ToList();
            var trendData = TrendDetector.CalculateByMesa(calculation_input);
            if (trendData == Trend.Undefined)
            {
                return response;
            }

            var hour4 = _historyService.GetHistory(Symbol, TimeframeHelper.HOUR4_GRANULARITY, Datafeed, Exchange, Type);
            var hourly = _historyService.GetHistory(Symbol, TimeframeHelper.HOURLY_GRANULARITY, Datafeed, Exchange, Type);
            var min15 = _historyService.GetHistory(Symbol, TimeframeHelper.MIN15_GRANULARITY, Datafeed, Exchange, Type);
            var task = await Task.WhenAll<HistoryData>(new[] { hour4, hourly, min15 });
            var hour4PriceData = task[0];
            var hourlyPriceData = task[1];
            var min15PriceData = task[2];

            var hour4ScanningResult = this.scanData(hour4PriceData, trendData);
            var hourlyScanningResult = this.scanData(hourlyPriceData, trendData);
            var min15ScanningResult = this.scanData(min15PriceData, trendData);

            if (hour4ScanningResult != null) {
                response.tte_240 = hour4ScanningResult.tte;
                response.tp_240 = hour4ScanningResult.tp;
            }
            if (hourlyScanningResult != null) {
                response.tte_60 = hourlyScanningResult.tte;
                response.tp_60 = hourlyScanningResult.tp;
            }
            if (min15ScanningResult != null) {
                response.tte_15 = min15ScanningResult.tte;
                response.tp_15 = min15ScanningResult.tp;
            }

            response.trend = trendData;
            return response;
        }

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
            if (trend == Trend.Up && lastBar.Close > natural) {
                return null;
            }
            
            if (trend == Trend.Down && lastBar.Close < natural) {
                return null;
            }

            var candlesPerformance = TechCalculations.CalculatePriceMoveDirection(high, low, close);
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

            if (priceDiffToHit <= 0) {
                return null;
            }

            var length = 14;
            var deviation = TechCalculations.StdDev(close.TakeLast(100).ToList(), length);
            var avgDeviation = deviation.Sum() / deviation.Count;
            var currentDeviation = deviation.TakeLast(length).Sum() / length;
            var deviationSpeed = Math.Round((currentDeviation - avgDeviation) / avgDeviation * 100, 0);
            var candlesToHit = Math.Round(priceDiffToHit / Math.Abs(candlesPerformance), 0) + 1;

            return new ScanResponse {
                tte = (int)candlesToHit,
                tp = (int)deviationSpeed
            };
        }
    }
}