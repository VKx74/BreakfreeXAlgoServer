using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Helpers;
using Algoserver.API.Models.Algo;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public class ScanResponse
    {
        public decimal entry { get; set; }
        public decimal stop { get; set; }
        public int tte { get; set; }
        public TradeProbability tp { get; set; }
        public TradeType type { get; set; }
        public Trend trend { get; set; }
        public decimal entry_h { get; set; }
        public decimal entry_l { get; set; }
        public decimal take_profit { get; set; }
        public decimal take_profit_h { get; set; }
        public decimal take_profit_l { get; set; }
        public decimal risk { get; set; }
        public decimal sl_ratio { get; set; }
    }

    public class ScanningHistory
    {
        public List<decimal> Open { get; set; }
        public List<decimal> High { get; set; }
        public List<decimal> Low { get; set; }
        public List<decimal> Close { get; set; }
    }

    public class ScannerService
    {
        private readonly HistoryService _historyService;

        public ScannerService(HistoryService historyService)
        {
            _historyService = historyService;
        }

        protected ScanResponse _scanSwingOldStrategy(ScanningHistory history, decimal sl_ration = 1.7m)
        {
            if (history.Close == null)
            {
                return null;
            }

            var high = history.High;
            var low = history.Low;
            var close = history.Close;
            var open = history.Open;
            var levels = TechCalculations.CalculateLevels(high, low);
            var tick = 0.00001m;
            var sar = SupportAndResistance.Calculate(levels, tick);
            var trend = TrendDetector.CalculateByMesa(close);
            var calculationData = new TradeSignalCalculationData
            {
                hma_period = 200,
                levels = levels,
                randomize = false,
                sar = sar,
                trend = trend,
                history = history
            };
            var tradeEntry = TradeEntry.CalculateV2(calculationData, sl_ration);
            if (tradeEntry.entry == Decimal.Zero) {
                return null;
            }

            var difference = TechCalculations.CalculateAvdCandleDifference(open, close);
            var candlesPerformance = TechCalculations.CalculatePriceMoveDirection(close);
            var priceDiffToHit = 0m;
            var lastClose = history.Close.LastOrDefault();

            // check is price go needed direction
            if (trend == Trend.Up)
            {
                priceDiffToHit = lastClose - tradeEntry.entry;
            }
            if (trend == Trend.Down)
            {
                priceDiffToHit = tradeEntry.entry - lastClose;
            }

            var candlesToHit = Math.Round(priceDiffToHit / Math.Abs(difference), 0);
            var direction = TechCalculations.ApproveDirection(close, trend, TradeType.BRC);

            if (candlesToHit < 0) {
                candlesToHit = 1;
            }
            
            if (trend == Trend.Up)
            {
                if (candlesPerformance > 0)
                {
                    candlesToHit *= 2;
                }
            }
            if (trend == Trend.Down)
            {
                if (candlesPerformance < 0)
                {
                    candlesToHit *= 2;
                }
            }

            return new ScanResponse
            {
                tte = (int)candlesToHit,
                tp = direction.TradeProbability,
                type = TradeType.SwingN,
                trend = trend,
                entry = tradeEntry.entry,
                entry_h = tradeEntry.entry_h,
                entry_l = tradeEntry.entry_l,
                take_profit = tradeEntry.tp,
                take_profit_h = tradeEntry.tp_h,
                take_profit_l = tradeEntry.tp_l,
                stop = tradeEntry.stop,
                sl_ratio = sl_ration
            };
        }
        public ScanResponse ScanSwingOldStrategy(ScanningHistory history, decimal sl_ration = 1.7m)
        {
            try {
                return this._scanSwingOldStrategy(history, sl_ration);
            } catch(Exception ex) {
                return null;
            }
        }


        protected ScanResponse _scanSwing(ScanningHistory history, ScanningHistory dailyHistory, Trend trendGlobal, Trend trendLocal, decimal sl_ration = 1.7m) {
            if (history.Close == null)
            {
                return null;
            }

            if (trendGlobal == Trend.Undefined)
            {
                return null;
            }

            if (trendGlobal == trendLocal)
            {
                var swingN = ScanBRC(history, trendGlobal, sl_ration);
                if (swingN != null)
                {
                    swingN.type = TradeType.SwingN;
                }
                return swingN;
            }
            else
            {
                var swingExt = ScanExt(history, dailyHistory, trendGlobal, sl_ration);
                if (swingExt != null)
                {
                    swingExt.type = TradeType.SwingExt;
                }
                return swingExt;
            }
        }
        public ScanResponse ScanSwing(ScanningHistory history, ScanningHistory dailyHistory, Trend trendGlobal, Trend trendLocal, decimal sl_ration = 1.7m)
        {
            try {
                return this._scanSwing(history, dailyHistory, trendGlobal, trendLocal, sl_ration);
            } catch(Exception ex) {
                return null;
            }
        }


        protected ScanResponse _scanBRC(ScanningHistory history, Trend trend, decimal sl_ration = 1.7m) 
        {
            if (history.Close == null)
            {
                return null;
            }

            if (trend == Trend.Undefined)
            {
                return null;
            }

            var lastClose = history.Close.LastOrDefault();
            var lastHigh = history.High.LastOrDefault();
            var lastLow = history.Low.LastOrDefault();
            var prevLow = history.Low[history.Low.Count - 2];
            var prevHigh = history.High[history.High.Count - 2];
            var prc = trend == Trend.Up ? prevLow : prevHigh;

            var high = history.High;
            var low = history.Low;
            var close = history.Close;
            var open = history.Open;
            var levels = TechCalculations.CalculateLevel128(high, low);

            var topExt = levels.Plus18;
            var natural = levels.FourEight;
            var bottomExt = levels.Minus18;
            var support = levels.ZeroEight;
            var resistance = levels.EightEight;
            var stop = 0m;
            var take_profit = 0m;
            var take_profit2 = 0m;
            var shift = (Math.Abs(natural - support) / 4);

            if (trend == Trend.Up)
            {
                if (prc < natural || prc >= (natural + resistance) / 2)
                {
                    return null;
                }
                stop = natural - shift;
                take_profit = natural + (shift * sl_ration);
                take_profit2 = natural + (shift * sl_ration / 2);
            }
            else
            {
                if (prc > natural || prc <= (natural + support) / 2)
                {
                    return null;
                }
                stop = natural + shift;
                take_profit = natural - (shift * sl_ration);
                take_profit2 = natural - (shift * sl_ration / 2);
            }

            var direction = TechCalculations.ApproveDirection(close, trend, TradeType.BRC);
            var directionApproved = direction.Approved;
            if (!directionApproved)
            {
                return null;
            }

            var overLevelCount = TechCalculations.BRCOverLevelCount(close, trend, natural);
            if (overLevelCount < 3 || overLevelCount > 40)
            {
                return null;
            }

            var count = 100;
            var approveLevel = 90;
            var belowLevelCount = TechCalculations.BRCBelowLevelCount(close, trend, natural, count);
            if ((double)belowLevelCount / (double)count * 100 < approveLevel)
            {
                return null;
            }

            var difference = TechCalculations.CalculateAvdCandleDifference(open, close);
            var candlesPerformance = TechCalculations.CalculatePriceMoveDirection(close);
            var priceDiffToHit = 0m;

            // check is price go needed direction
            if (trend == Trend.Up)
            {
                if (candlesPerformance > 0)
                {
                    return null;
                }

                priceDiffToHit = lastClose - natural;
            }
            if (trend == Trend.Down)
            {
                if (candlesPerformance < 0)
                {
                    return null;
                }

                priceDiffToHit = natural - lastClose;
            }

            var candlesToHit = Math.Round(priceDiffToHit / Math.Abs(difference), 0);

            if (candlesToHit <= 0)
            {
                candlesToHit = 1;
            }

            if (candlesToHit > 30)
            {
                return null;
            } 
            if (candlesToHit > 10)
            {
                direction.TradeProbability = TradeProbability.Low;
            } 
            if (candlesToHit <= 2 && direction.TradeProbability != TradeProbability.Low)
            {
                direction.TradeProbability = TradeProbability.High;
            }

            var avgrange = TechCalculations.AverageRange(50, high, low);

            var return_Entry = natural;
            var return_Entry_high = return_Entry + (avgrange * 0.25m);
            var return_Entry_low = return_Entry - (avgrange * 0.25m);
            var return_TP_high = take_profit2 + (avgrange * 0.125m);
            var return_TP_low = take_profit2 - (avgrange * 0.125m);
            return new ScanResponse
            {
                tte = (int)candlesToHit,
                tp = direction.TradeProbability,
                type = TradeType.BRC,
                trend = trend,
                entry = return_Entry,
                entry_h = return_Entry_high,
                entry_l = return_Entry_low,
                take_profit = take_profit,
                take_profit_h = return_TP_high,
                take_profit_l = return_TP_low,
                stop = stop,
                sl_ratio = sl_ration
            };
        }

        public ScanResponse ScanBRC(ScanningHistory history, Trend trend, decimal sl_ration = 1.7m)
        {
            try {
                return this._scanBRC(history, trend, sl_ration);
            } catch(Exception ex) {
                return null;
            }
        }


        protected ScanResponse _scanExt(ScanningHistory history, ScanningHistory dailyHistory, Trend trend, decimal sl_ration = 1.7m)
        {
            if (history.Close == null)
            {
                return null;
            }

            if (trend == Trend.Undefined)
            {
                return null;
            }

            var lastClose = history.Close.LastOrDefault();
            var lastHigh = history.High.LastOrDefault();
            var lastLow = history.Low.LastOrDefault();
            var prevLow = history.Low[history.Low.Count - 2];
            var prevHigh = history.High[history.High.Count - 2];
            var prc = trend == Trend.Up ? prevLow : prevHigh;

            var high = history.High;
            var low = history.Low;
            var close = history.Close;
            var open = history.Open;
            var levels = TechCalculations.CalculateLevel128(high, low);

            var topExt = levels.Plus18;
            var natural = levels.FourEight;
            var bottomExt = levels.Minus18;
            var support = levels.ZeroEight;
            var resistance = levels.EightEight;
            var shift = (decimal)(Math.Abs((double)(levels.Plus28 - levels.Plus18)) * 0.3);
            var take_profit = 0m;
            var take_profit2 = 0m;
            var avgEntry = 0m;
            var stop = 0m;

            // check is price above/below natural level
            if (trend == Trend.Up && prc > (natural + bottomExt) / 2)
            {
                return null;
            }

            if (trend == Trend.Down && prc < (natural + topExt) / 2)
            {
                return null;
            }

            if (trend == Trend.Up)
            {
                avgEntry = (bottomExt + support) / 2;
                stop = levels.Minus28 - shift;
                var diff = Math.Abs(avgEntry - stop);
                take_profit = avgEntry + (diff * sl_ration);
                take_profit2 = avgEntry + (diff * sl_ration / 2);
            }
            else
            {
                avgEntry = (topExt + resistance) / 2;
                stop = levels.Plus28 + (decimal)shift;
                var diff = Math.Abs(stop - avgEntry);
                take_profit = avgEntry - (diff * sl_ration);
                take_profit2 = avgEntry - (diff * sl_ration / 2);
            }

            var direction = TechCalculations.ApproveDirection(close, trend, TradeType.EXT);
            var directionApproved = direction.Approved;

            if (!directionApproved)
            {
                return null;
            }

            var candlesPerformance = TechCalculations.CalculatePriceMoveDirection(close);
            var difference = TechCalculations.CalculateAvdCandleDifference(open, close);
            var priceDiffToHit = 0m;

            // check is price go needed direction
            if (trend == Trend.Up)
            {
                if (candlesPerformance > 0)
                {
                    return null;
                }

                priceDiffToHit = lastClose - bottomExt;
            }

            if (trend == Trend.Down)
            {
                if (candlesPerformance < 0)
                {
                    return null;
                }
                priceDiffToHit = topExt - lastClose;
            }

            // if (priceDiffToHit <= 0) {
            //     return null;
            // }

            var candlesToHit = Math.Round(priceDiffToHit / Math.Abs(difference), 0);

            if (candlesToHit <= 0)
            {
                candlesToHit = 1;
            }

            if (candlesToHit > 30)
            {
                return null;
            }
            if (candlesToHit > 10)
            {
                direction.TradeProbability = TradeProbability.Low;
            }
            if (candlesToHit <= 2 && direction.TradeProbability != TradeProbability.Low)
            {
                direction.TradeProbability = TradeProbability.High;
            }

            var isDailySAndRValid = isDailySupportAndResistanceValid(dailyHistory, trend);
            if (!isDailySAndRValid) {
                return null;
            }

            var avgrange = TechCalculations.AverageRange(50, high, low);
            var return_Entry = avgEntry;
            var return_Entry_high = support;
            var return_Entry_low = bottomExt;
            var return_TP_high = take_profit2 + (avgrange * 0.25m);
            var return_TP_low = take_profit2 - (avgrange * 0.25m);

            if (trend == Trend.Down) {
                return_Entry_high = topExt;
                return_Entry_low = resistance;
            }

            return new ScanResponse
            {
                tte = (int)candlesToHit,
                tp = direction.TradeProbability,
                type = TradeType.EXT,
                trend = trend,
                entry = return_Entry,
                entry_h = return_Entry_high,
                entry_l = return_Entry_low,
                take_profit = take_profit,
                take_profit_h = return_TP_high,
                take_profit_l = return_TP_low,
                stop = stop,
                sl_ratio = sl_ration
            };
        }

        public ScanResponse ScanExt(ScanningHistory history, ScanningHistory dailyHistory, Trend trend, decimal sl_ration = 1.7m)
        {
            try {
                return this._scanExt(history, dailyHistory, trend, sl_ration);
            } catch(Exception ex) {
                return null;
            }
        }


        private bool isDailySupportAndResistanceValid(ScanningHistory dailyHistory, Trend trend) {
            var high = dailyHistory.High;
            var low = dailyHistory.Low;
            var close = dailyHistory.Close;
            var levels = TechCalculations.CalculateLevel128(high, low);
            var natural = levels.FourEight;
            var support = levels.ZeroEight;
            var resistance = levels.EightEight;
            var allowedDiff = Math.Abs(natural - support) / 5;
            var lastClose = close.LastOrDefault();

            if (lastClose == decimal.Zero) {
                return false;
            }

            if (trend == Trend.Down) {
                var priceToTargetDiff = Math.Abs(support - lastClose);
                if (support >= lastClose || allowedDiff > priceToTargetDiff) {
                    return false;
                }
            } else {
                var priceToTargetDiff = Math.Abs(resistance - lastClose);
                if (lastClose >= resistance || allowedDiff > priceToTargetDiff) {
                    return false;
                }
            }

            return true;
        }

        public ScanningHistory ToScanningHistory(IEnumerable<BarMessage> history)
        {
            return new ScanningHistory
            {
                Open = history.Select(_ => _.Open).ToList(),
                High = history.Select(_ => _.High).ToList(),
                Low = history.Select(_ => _.Low).ToList(),
                Close = history.Select(_ => _.Close).ToList(),
            };
        }
    }
}
