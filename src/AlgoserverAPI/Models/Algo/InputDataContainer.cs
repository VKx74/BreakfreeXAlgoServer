using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.REST;
using Newtonsoft.Json;

namespace Algoserver.API.Models.Algo
{
    public class InputDataContainer
    {
        
        /// <summary>
        /// Unique ID of request
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Multiplier of resolution, e.g. '60' - 60, 'D' - 1, '5D' - 5, '12M' - 12
        /// </summary>
        [JsonProperty("timeframe_multiplier")]
        public int TimeframeInterval { get; set; }

        /// <summary>
        /// Resolution, e.g. '60' - 60 minutes, 'D' - daily, 'W' - weekly, 'M' - monthly, '5D' - 5 days, '12M' - one year, '3M' - one quarter
        /// </summary>
        [JsonProperty("timeframe_period")]
        public string TimeframePeriod { get; set; }

        [JsonProperty("open")]
        public List<decimal> Open { get; set; }

        [JsonProperty("high")]
        public List<decimal> High { get; set; }

        [JsonProperty("low")]
        public List<decimal> Low { get; set; }

        [JsonProperty("close")]
        public List<decimal> Close { get; set; } 
        
        [JsonProperty("close")]
        public List<long> Time { get; set; }

        [JsonProperty("open_D")]
        public List<decimal> OpenD { get; set; }

        [JsonProperty("high_D")]
        public List<decimal> HighD { get; set; }

        [JsonProperty("low_D")]
        public List<decimal> LowD { get; set; }

        [JsonProperty("close_D")]
        public List<decimal> CloseD { get; set; }

        [JsonProperty("time_D")]
        public List<long> TimeD { get; set; }

        [JsonProperty("mintick")]
        public decimal Mintick { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("input_stoplossratio")]
        public decimal InputStoplossRatio { get; set; }

        [JsonProperty("input_detectlowhigh")]
        public bool InputDetectLowHigh { get; set; }

        [JsonProperty("input_splitpositions")]
        public decimal InputSplitPositions { get; set; }

        [JsonProperty("input_accountsize")]
        public decimal InputAccountSize { get; set; }

        [JsonProperty("input_risk")]
        public decimal InputRisk { get; set; }

        [JsonProperty("UsdRatio")]
        public decimal UsdRatio { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("quotedCurrency")]
        public string QuotedCurrency { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("datafeed")]
        public string Datafeed { get; set; }

        [JsonProperty("replay_back")]
        public int ReplayBack { get; set; }

        public static InputDataContainer MapCalculationRequestToInputDataContainer(CalculationRequest req)
        {
            if (req == null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            return new InputDataContainer
            {
                Id = req.Id,
                TimeframeInterval = req.Timeframe.Interval,
                TimeframePeriod = req.Timeframe.Periodicity,
                Mintick = req.Instrument.TickSize,
                Type = req.Instrument.Type.ToLowerInvariant(),
                Symbol = req.Instrument.Id,
                InputStoplossRatio = req.InputStoplossRatio,
                InputDetectLowHigh = req.inputDetectlowHigh.GetValueOrDefault(false),
                InputSplitPositions = req.InputSplitPositions,
                InputAccountSize = req.InputAccountSize,
                InputRisk = req.InputRisk,
                Currency = req.Instrument.BaseInstrument,
                QuotedCurrency = req.Instrument.DependInstrument,
                Exchange = req.Instrument.Exchange.ToLowerInvariant(),
                Datafeed = req.Instrument.Datafeed.ToLowerInvariant(),
                ReplayBack = req.ReplayBack.GetValueOrDefault(0)
            };
        }

        public void setUsdRatio(decimal usdRatio)
        {
            UsdRatio = usdRatio;
        }

        public void InsertHistory(IEnumerable<BarMessage> currentPriceData, IEnumerable<BarMessage> dailyPriceData, int replayBack)
        {
            var priceData = currentPriceData.Take(currentPriceData.Count() - replayBack);

            var priceDataCount = priceData.Count();
            if (priceDataCount > 400) {
                priceData = priceData.TakeLast(400);
            }

            Open = priceData.Select(i => i.Open).ToList();
            High = priceData.Select(i => i.High).ToList();
            Low = priceData.Select(i => i.Low).ToList(); 
            Close = priceData.Select(i => i.Close).ToList();
            Time = priceData.Select(i => i.Timestamp).ToList();
            var lastCandleTime = Time.LastOrDefault();

            var dailyPrices = dailyPriceData.TakeWhile(i => i.Timestamp <= lastCandleTime);

            var dailyPriceCount = dailyPrices.Count();
            if (dailyPriceCount > 400) {
                dailyPrices = dailyPrices.TakeLast(400);
            }

            OpenD = dailyPrices.Select(i => i.Open).ToList();
            HighD = dailyPrices.Select(i => i.High).ToList();
            LowD = dailyPrices.Select(i => i.Low).ToList();
            CloseD = dailyPrices.Select(i => i.Close).ToList();
            TimeD = dailyPrices.Select(i => i.Timestamp).ToList();
            var lastDailyCandleTime = TimeD.LastOrDefault();

            if (replayBack != 0 && dailyPrices.Any()) {
                var currentDayBars = priceData.Where(i => i.Timestamp >= lastDailyCandleTime);

                if (!currentDayBars.Any()) {
                    return;
                }

                var close = currentDayBars.Last().Close;
                var high = currentDayBars.Max(i => i.High);
                var low = currentDayBars.Min(i => i.Low);

                CloseD[CloseD.Count - 1] = close;
                HighD[HighD.Count - 1] = high;
                LowD[LowD.Count - 1] = low;
            }
        } 
        
        public bool AppendOne(IEnumerable<BarMessage> currentPriceData, IEnumerable<BarMessage> dailyPriceData, int granularity)
        {
            var lastCandleTime = Time.LastOrDefault();
            var nextCandle = currentPriceData.FirstOrDefault(_ => _.Timestamp > lastCandleTime);

            if (nextCandle == null) {
                return false;
            }

            Open.Add(nextCandle.Open);
            High.Add(nextCandle.High);
            Low.Add(nextCandle.Low);
            Close.Add(nextCandle.Close);
            Time.Add(nextCandle.Timestamp);
            lastCandleTime = nextCandle.Timestamp;

            var lastDailyCandleTime = Time.LastOrDefault();
            var nextDailyCandles = dailyPriceData.Where(_ => _.Timestamp > lastDailyCandleTime && _.Timestamp <= lastCandleTime);
            
            foreach (var nextDailyCandle in nextDailyCandles) {
                OpenD.Add(nextDailyCandle.Open);
                HighD.Add(nextDailyCandle.High);
                LowD.Add(nextDailyCandle.Low);
                CloseD.Add(nextDailyCandle.Close);
                TimeD.Add(nextDailyCandle.Timestamp);
            }

            if (granularity >= 86400) {
                return true;
            }

            lastDailyCandleTime = TimeD.LastOrDefault();

            var currentDayBars = currentPriceData.Where(i => i.Timestamp >= lastDailyCandleTime && i.Timestamp <= lastCandleTime);

            if (currentDayBars.Any()) {
                var close = currentDayBars.Last().Close;
                var high = currentDayBars.Max(i => i.High);
                var low = currentDayBars.Min(i => i.Low);
                CloseD[CloseD.Count - 1] = close;
                HighD[HighD.Count - 1] = high;
                LowD[LowD.Count - 1] = low;
            }

            return true;
        }
    }
}
