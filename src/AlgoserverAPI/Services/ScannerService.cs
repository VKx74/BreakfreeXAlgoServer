using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public ScanResponse ScanSwingOldStrategy(ScanningHistory history) 
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
            var tradeEntry = TradeEntry.CalculateV2(calculationData);
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
                stop = tradeEntry.stop
            };
        }

        public ScanResponse ScanSwing(ScanningHistory history, Trend trendGlobal, Trend trendLocal)
        {
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
                var swingN = ScanBRC(history, trendGlobal);
                if (swingN != null)
                {
                    swingN.type = TradeType.SwingN;
                }
                return swingN;
            }
            else
            {
                var swingExt = ScanExt(history, trendGlobal);
                if (swingExt != null)
                {
                    swingExt.type = TradeType.SwingExt;
                }
                return swingExt;
            }
        }

        public ScanResponse ScanBRC(ScanningHistory history, Trend trend)
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

            if (trend == Trend.Up)
            {
                if (prc < natural || prc >= (natural + resistance) / 2)
                {
                    return null;
                }
                stop = natural - (Math.Abs(natural - support) / 4);
            }
            else
            {
                if (prc > natural || prc <= (natural + support) / 2)
                {
                    return null;
                }
                stop = natural + (Math.Abs(natural - support) / 4);
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
            if (candlesToHit > 20)
            {
                direction.TradeProbability = TradeProbability.Low;
            } 
            if (candlesToHit <= 2 && direction.TradeProbability != TradeProbability.Low)
            {
                direction.TradeProbability = TradeProbability.High;
            }

            return new ScanResponse
            {
                tte = (int)candlesToHit,
                tp = direction.TradeProbability,
                type = TradeType.BRC,
                trend = trend,
                entry = natural,
                stop = stop
            };
        }

        public ScanResponse ScanExt(ScanningHistory history, Trend trend)
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
            var shift = Math.Abs((double)(levels.Plus28 - levels.Plus18)) * 0.3;
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
                stop = levels.Minus28 - (decimal)shift;
            }
            else
            {
                avgEntry = (topExt + resistance) / 2;
                stop = levels.Plus28 + (decimal)shift;
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
            if (candlesToHit > 20)
            {
                direction.TradeProbability = TradeProbability.Low;
            }
            if (candlesToHit <= 2 && direction.TradeProbability != TradeProbability.Low)
            {
                direction.TradeProbability = TradeProbability.High;
            }

            return new ScanResponse
            {
                tte = (int)candlesToHit,
                tp = direction.TradeProbability,
                type = TradeType.EXT,
                trend = trend,
                entry = avgEntry,
                stop = stop
            };
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
