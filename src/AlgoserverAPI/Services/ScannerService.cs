using System;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.Algo;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public class ScanResponse
    {
        public int tte { get; set; }
        public int tp { get; set; }
    }

    public class ScannerService
    {

        private readonly HistoryService _historyService;

        public ScannerService(HistoryService historyService)
        {
            _historyService = historyService;
        }

        internal Task<ScanInstrumentResponse> ScanInstrument(ScanInstrumentRequest req)
        {
            return Task.Run((Func<Task<ScanInstrumentResponse>>)(async () =>
            {
                return await this.scanExtTrades((ScanInstrumentRequest)req);
            }));
        }

        protected async Task<ScanInstrumentResponse> scanExtTrades(ScanInstrumentRequest req)
        {
            var response = new ScanInstrumentResponse
            {
                trend = Trend.Undefined,
                type = TradeType.EXT
            };

            var Exchange = req.Instrument.Exchange.ToLowerInvariant();
            var Datafeed = req.Instrument.Datafeed.ToLowerInvariant();
            var Type = req.Instrument.Type.ToLowerInvariant();
            var Symbol = req.Instrument.Id;

            var dailyPriceData = await _historyService.GetHistory(Symbol, TimeframeHelper.DAILY_GRANULARITY, Datafeed, Exchange, Type);
            var calculation_input = dailyPriceData.Bars.Select(_ => _.Close).ToList();
            var trendData = TrendDetector.CalculateByMesaWithTrendAdjusted(calculation_input);
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

            var hour4ScanningResult = this.ScanExt(hour4PriceData, trendData);
            var hourlyScanningResult = this.ScanExt(hourlyPriceData, trendData);
            var min15ScanningResult = this.ScanExt(min15PriceData, trendData);

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

        public ScanResponse ScanExt(HistoryData history, Trend trend) {
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
