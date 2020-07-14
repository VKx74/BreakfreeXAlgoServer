using System;
using System.Linq;
using Algoserver.API.Models.REST;
using Newtonsoft.Json;

namespace Algoserver.API.Models.Algo
{
    public class InputDataContainer
    {
        /// <summary>
        /// Current bar time in UNIX format. It is the number of milliseconds that have elapsed since 00:00:00 UTC, 1 January 1970. (Only updates whilst market open)
        /// </summary>
        [JsonProperty("time")]
        public int Time { get; set; }

        /// <summary>
        /// Current time in UNIX format. It is the number of milliseconds that have elapsed since 00:00:00 UTC, 1 January 1970. (Only updates whilst market open)
        /// </summary>
        [JsonProperty("timenow")]
        public int Timenow { get; set; }

        /// <summary>
        /// Multiplier of resolution, e.g. '60' - 60, 'D' - 1, '5D' - 5, '12M' - 12
        /// </summary>
        [JsonProperty("timeframe_multiplier")]
        public int TimeframeMultiplier { get; set; }

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

        [JsonProperty("accSizeFX")]
        public decimal AccSizeFX { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("quotedCurrency")]
        public string QuotedCurrency { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }
        
        public static InputDataContainer MapCalculationRequestToInputDataContainer(CalculationRequest req)
        {
            if (req == null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            return new InputDataContainer
            {
                Time = req.Time,
                Timenow = req.Timenow,
                TimeframeMultiplier = req.Timeframe.Interval,
                TimeframePeriod = req.Timeframe.periodicity,
                Mintick = req.Instrument.TickSize,
                Type = req.Instrument.Type.ToLowerInvariant(),
                Symbol = req.Instrument.Id ?? req.Instrument.Symbol,
                InputStoplossRatio = req.InputStoplossRatio,
                InputDetectLowHigh = req.inputDetectlowHigh,
                InputSplitPositions = req.InputSplitPositions,
                InputAccountSize = req.InputAccountSize,
                InputRisk = req.InputRisk,
                Currency = req.Instrument.BaseInstrument,
                QuotedCurrency = req.Instrument.DependInstrument,
                Exchange = req.Instrument.Exchange
            };
        }

        public void Update(Bar[] currentPriceData, Bar[] dailyPriceData, decimal[] accSizeData)
        {
            Open = currentPriceData.Select(i => i.Open).ToArray();
            High = currentPriceData.Select(i => i.High).ToArray();
            Low = currentPriceData.Select(i => i.Low).ToArray(); 
            Close = currentPriceData.Select(i => i.Close).ToArray();

            OpenD = dailyPriceData.Select(i => i.Open).ToArray();
            HighD = dailyPriceData.Select(i => i.High).ToArray();
            LowD = dailyPriceData.Select(i => i.Low).ToArray();
            CloseD = dailyPriceData.Select(i => i.Close).ToArray();

            AccSizeFX = accSizeData.Any() ? accSizeData[0] : 1;
        }
    }
}
