using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.REST;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Algoserver.API.Models.Algo
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TrendDetectorType
    {
        hma,
        mesa
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Trend
    {
        Up,
        Down,
        Undefined
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradeType
    {
        EXT,
        SwingN,
        SwingExt,
        BRC
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TradeProbability
    {
        Low,
        Mid,
        High
    }

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

        [JsonProperty("time")]
        public List<long> Time { get; set; }

        [JsonProperty("open_D")]
        public List<decimal> OpenD { get; set; }

        [JsonProperty("high_D")]
        public List<decimal> HighD { get; set; }

        [JsonProperty("low_D")]
        public List<decimal> LowD { get; set; }

        [JsonProperty("close_D")]
        public List<decimal> CloseD { get; set; }

        [JsonProperty("time_d")]
        public List<long> TimeD { get; set; }

        [JsonProperty("open_H")]
        public List<decimal> OpenH { get; set; }

        [JsonProperty("high_H")]
        public List<decimal> HighH { get; set; }

        [JsonProperty("low_H")]
        public List<decimal> LowH { get; set; }

        [JsonProperty("close_H")]
        public List<decimal> CloseH { get; set; }

        [JsonProperty("time_H")]
        public List<long> TimeH { get; set; }

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
        public int InputSplitPositions { get; set; }

        [JsonProperty("input_accountsize")]
        public decimal InputAccountSize { get; set; }

        [JsonProperty("contract_size")]
        public decimal? ContractSize { get; set; }

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
                ContractSize = req.ContractSize,
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

        public static int MIN_BARS_COUNT = 500;

        public void InsertHistory(IEnumerable<BarMessage> currentPriceData, IEnumerable<BarMessage> hourlyPriceData, IEnumerable<BarMessage> dailyPriceData, int replayBack)
        {
            var priceData = currentPriceData.Take(currentPriceData.Count() - replayBack);

            var priceDataCount = priceData.Count();
            if (priceDataCount > MIN_BARS_COUNT)
            {
                priceData = priceData.TakeLast(MIN_BARS_COUNT);
            }

            Open = priceData.Select(i => i.Open).ToList();
            High = priceData.Select(i => i.High).ToList();
            Low = priceData.Select(i => i.Low).ToList();
            Close = priceData.Select(i => i.Close).ToList();
            Time = priceData.Select(i => i.Timestamp).ToList();

            var lastCandle = priceData.LastOrDefault();
            var lastCandleTime = lastCandle != null ? lastCandle.Timestamp : 0;

            if (dailyPriceData != null)
            {
                var dailyPrices = dailyPriceData.TakeWhile(i => i.Timestamp <= lastCandleTime);
                var dailyPriceCount = dailyPrices.Count();
                if (dailyPriceCount > MIN_BARS_COUNT)
                {
                    dailyPrices = dailyPrices.TakeLast(MIN_BARS_COUNT);
                }

                OpenD = dailyPrices.Select(i => i.Open).ToList();
                HighD = dailyPrices.Select(i => i.High).ToList();
                LowD = dailyPrices.Select(i => i.Low).ToList();
                CloseD = dailyPrices.Select(i => i.Close).ToList();
                TimeD = dailyPrices.Select(i => i.Timestamp).ToList();
                var lastTime = TimeD.LastOrDefault();
                var correctionCandles = priceData.Where((_) => _.Timestamp >= lastTime).ToList();
                var openBar = priceData.FirstOrDefault();
                var closeBar = priceData.LastOrDefault();
                var min = correctionCandles.Select((_) => _.Low).Min();
                var max = correctionCandles.Select((_) => _.High).Max();

                if (closeBar != null)
                {
                    CloseD[CloseD.Count - 1] = closeBar.Close;
                }

                if (min > 0)
                {
                    LowD[LowD.Count - 1] = min;
                }

                if (max > 0)
                {
                    HighD[HighD.Count - 1] = max;
                }
            }

            if (hourlyPriceData != null)
            {
                var hourlyPrices = hourlyPriceData.TakeWhile(i => i.Timestamp <= lastCandleTime);
                var hourlyPriceCount = hourlyPrices.Count();
                if (hourlyPriceCount > MIN_BARS_COUNT)
                {
                    hourlyPrices = hourlyPrices.TakeLast(MIN_BARS_COUNT);
                }

                OpenH = hourlyPrices.Select(i => i.Open).ToList();
                HighH = hourlyPrices.Select(i => i.High).ToList();
                LowH = hourlyPrices.Select(i => i.Low).ToList();
                CloseH = hourlyPrices.Select(i => i.Close).ToList();
                TimeH = hourlyPrices.Select(i => i.Timestamp).ToList();
                CloseH[CloseH.Count - 1] = Close[Close.Count - 1];
            }
            // var lastDailyCandle = dailyPrices.LastOrDefault();
            // var lastDailyCandleTime = lastDailyCandle != null ? lastDailyCandle.Timestamp : long.MaxValue;

            // if (replayBack != 0 && dailyPrices.Any()) {
            //     var currentDayBars = priceData.Where(i => i.Timestamp >= lastDailyCandleTime);

            //     if (!currentDayBars.Any()) {
            //         return;
            //     }

            //     var close = currentDayBars.Last().Close;
            //     var high = currentDayBars.Max(i => i.High);
            //     var low = currentDayBars.Min(i => i.Low);

            //     CloseD[CloseD.Length - 1] = close;
            //     HighD[HighD.Length - 1] = high;
            //     LowD[LowD.Length - 1] = low;
            // }
        }

        public void AddNext(IEnumerable<BarMessage> currentPriceData, IEnumerable<BarMessage> hourlyPriceData, IEnumerable<BarMessage> dailyPriceData, int replayBack)
        {
            if (Open == null || !Open.Any())
            {
                InsertHistory(currentPriceData, hourlyPriceData, dailyPriceData, replayBack);
                return;
            }

            var priceData = currentPriceData.Take(currentPriceData.Count() - replayBack);
            var lastCandle = priceData.LastOrDefault();

            Open.Add(lastCandle.Open);
            High.Add(lastCandle.High);
            Low.Add(lastCandle.Low);
            Close.Add(lastCandle.Close);
            Time.Add(lastCandle.Timestamp);

            if (hourlyPriceData != null) {
                var lastExistingTime = TimeH.LastOrDefault();
                var hourlyPrices = hourlyPriceData.Where(i => i.Timestamp <= lastCandle.Timestamp && i.Timestamp > lastExistingTime);
                var length = hourlyPrices.Count();

                foreach(var hourlyPrice in hourlyPrices) {
                    OpenH.Add(hourlyPrice.Open);
                    HighH.Add(hourlyPrice.High);
                    LowH.Add(hourlyPrice.Low);
                    CloseH.Add(hourlyPrice.Close);
                    TimeH.Add(hourlyPrice.Timestamp);
                }
            } 
            
            if (dailyPriceData != null) {
                var lastExistingTime = TimeD.LastOrDefault();
                var dailyPrices = dailyPriceData.Where(i => i.Timestamp <= lastCandle.Timestamp && i.Timestamp > lastExistingTime);

                foreach(var dailyPrice in dailyPrices) {
                    OpenD.Add(dailyPrice.Open);
                    HighD.Add(dailyPrice.High);
                    LowD.Add(dailyPrice.Low);
                    CloseD.Add(dailyPrice.Close);
                    TimeD.Add(dailyPrice.Timestamp);
                }
            }
        }
    }
}
