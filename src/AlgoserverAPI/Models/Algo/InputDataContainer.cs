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
        public decimal[] Open { get; set; }

        [JsonProperty("high")]
        public decimal[] High { get; set; }

        [JsonProperty("low")]
        public decimal[] Low { get; set; }

        [JsonProperty("close")]
        public decimal[] Close { get; set; }

        [JsonProperty("time")]
        public long[] Time { get; set; }

        [JsonProperty("open_D")]
        public decimal[] OpenD { get; set; }

        [JsonProperty("high_D")]
        public decimal[] HighD { get; set; }

        [JsonProperty("low_D")]
        public decimal[] LowD { get; set; }

        [JsonProperty("close_D")]
        public decimal[] CloseD { get; set; }

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
            if (priceDataCount > 300) {
                priceData = priceData.TakeLast(300);
            }

            Open = priceData.Select(i => i.Open).ToArray();
            High = priceData.Select(i => i.High).ToArray();
            Low = priceData.Select(i => i.Low).ToArray(); 
            Close = priceData.Select(i => i.Close).ToArray();
            Time = priceData.Select(i => i.Timestamp).ToArray();
            
            var lastCandle = priceData.LastOrDefault();
            var lastCandleTime = lastCandle != null ? lastCandle.Timestamp : 0;

            var dailyPrices = dailyPriceData.TakeWhile(i => i.Timestamp <= lastCandleTime);

            var dailyPriceCount = dailyPrices.Count();
            if (dailyPriceCount > 300) {
                dailyPrices = dailyPrices.TakeLast(300);
            }

            OpenD = dailyPrices.Select(i => i.Open).ToArray();
            HighD = dailyPrices.Select(i => i.High).ToArray();
            LowD = dailyPrices.Select(i => i.Low).ToArray();
            CloseD = dailyPrices.Select(i => i.Close).ToArray();
            var lastDailyCandle = dailyPrices.LastOrDefault();
            var lastDailyCandleTime = lastDailyCandle != null ? lastDailyCandle.Timestamp : long.MaxValue;

            if (replayBack != 0 && dailyPrices.Any()) {
                var currentDayBars = priceData.Where(i => i.Timestamp >= lastDailyCandleTime);

                if (!currentDayBars.Any()) {
                    return;
                }

                var close = currentDayBars.Last().Close;
                var high = currentDayBars.Max(i => i.High);
                var low = currentDayBars.Min(i => i.Low);

                CloseD[CloseD.Length - 1] = close;
                HighD[HighD.Length - 1] = high;
                LowD[LowD.Length - 1] = low;
            }
        }
    }
}
