using Fintatech.TDS.Common.Protocol.BaseMessages.RequestResponse;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Algoserver.API.Models.REST
{
    public class CalculatePriceRatioRequest
    {
        [JsonProperty("instrument")]
        public Instrument Instrument { get; set; }

        [JsonProperty("account_currency")]
        public string AccountCurrency { get; set; }
    }

    public class CalculatePositionSizeRequest
    {

        [JsonProperty("input_accountsize")]
        public decimal InputAccountSize { get; set; }

        [JsonProperty("account_currency")]
        public string AccountCurrency { get; set; }

        [JsonProperty("contract_size")]
        public decimal? ContractSize { get; set; }

        [JsonProperty("instrument")]
        public Instrument Instrument { get; set; }

        [JsonProperty("input_risk")]
        public decimal InputRisk { get; set; }
        
        [JsonProperty("price_diff")]
        public decimal PriceDiff { get; set; }
    }

    public class MarketInfoCalculationRequest
    {

        [JsonProperty("granularity")]
        public int? Granularity { get; set; }

        [JsonProperty("instrument")]
        public Instrument Instrument { get; set; }
    }

    public class HistoryDataRequest
    {

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("timeframe")]
        public int Timeframe { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

    }

    public class CalculationRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("timeframe")]
        public Timeframe Timeframe { get; set; }

        [JsonProperty("instrument")]
        public Instrument Instrument { get; set; }

        [JsonProperty("input_stoplossratio")]
        public decimal InputStoplossRatio { get; set; }

        [JsonProperty("input_detectlowhigh")]
        public bool? inputDetectlowHigh { get; set; }

        [JsonProperty("input_splitpositions")]
        public int InputSplitPositions { get; set; }

        [JsonProperty("input_accountsize")]
        public decimal InputAccountSize { get; set; }
        
        [JsonProperty("account_currency")]
        public string AccountCurrency { get; set; }

        [JsonProperty("contract_size")]
        public decimal? ContractSize { get; set; }

        [JsonProperty("input_risk")]
        public decimal InputRisk { get; set; }

        [JsonProperty("replay_back")]
        public int? ReplayBack { get; set; }
    }

    public class Timeframe
    {
        [JsonProperty("periodicity")]
        public string Periodicity { get; set; }

        [JsonProperty("interval")]
        public int Interval { get; set; }
    }

    public class Instrument
    {
        [JsonProperty("id")]
        public string Id { get; set; } // AUD_USD

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("datafeed")]
        public string Datafeed { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } // Forex

        [JsonProperty("tickSize")]
        public decimal TickSize { get; set; }

        [JsonProperty("baseInstrument")]
        public string BaseInstrument { get; set; } // USD

        [JsonProperty("dependInstrument")]
        public string DependInstrument { get; set; } // AUD
        
        public override string ToString()
        {
            return $"{Id}-{Exchange}-{Datafeed}";
        }
    }
}
