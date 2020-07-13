using Fintatech.TDS.Common.Protocol.BaseMessages.RequestResponse;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Algoserver.API.Models.REST
{
    public class CalculationRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("time")]
        public int Time { get; set; } // 1594650600000

        [JsonProperty("timenow")]
        public int Timenow { get; set; }

        [JsonProperty("timeframe")]
        public Timeframe Timeframe { get; set; }

        [JsonProperty("instrument")]
        public Instrument Instrument { get; set; }

        [JsonProperty("input_stoplossratio")]
        public decimal InputStoplossratio { get; set; }

        [JsonProperty("input_splitpositions")]
        public decimal InputSplitpositions { get; set; }

        [JsonProperty("input_accountsize")]
        public decimal InputAccountsize { get; set; }

        [JsonProperty("input_risk")]
        public decimal InputRisk { get; set; }
    }

    public class Timeframe
    {
        [JsonProperty("Periodicity")]
        public string periodicity { get; set; }

        [JsonProperty("interval")]
        public int Interval { get; set; }
    }

    public class Instrument
    {
        [JsonProperty("id")]
        public string Id { get; set; } // AUD_USD

        [JsonProperty("symbol")]
        public string Symbol { get; set; } // AUDUSD

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("datafeed")]
        public string Datafeed { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } // Forex

        [JsonProperty("tickSize")]
        public decimal TickSize { get; set; }

        [JsonProperty("pricePrecision")]
        public decimal PricePrecision { get; set; }

        [JsonProperty("baseInstrument")]
        public string BaseInstrument { get; set; } // USD

        [JsonProperty("dependInstrument")]
        public string DependInstrument { get; set; } // AUD

        [JsonProperty("company")]
        public string Company { get; set; } // AUD vs USD

        [JsonProperty("tradable")]
        public bool Tradable { get; set; }
    }
}
