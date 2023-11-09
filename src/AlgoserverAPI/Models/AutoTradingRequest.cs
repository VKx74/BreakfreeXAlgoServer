using Newtonsoft.Json;

namespace Algoserver.API.Models.REST
{
    public class AutoTradingSymbolInfoRequest
    {
        [JsonProperty("instrument")]
        public Instrument Instrument { get; set; }

        [JsonProperty("minStrength1h")]
        public decimal MinStrength1h { get; set; } = 15m;

        [JsonProperty("minStrength4h")]
        public decimal MinStrength4h { get; set; }

        [JsonProperty("minStrength1d")]
        public decimal MinStrength1d { get; set; }

        [JsonProperty("minStrength")]
        public decimal MinStrength { get; set; }

        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
    
    public class AutoTradeInstrumentsRequest
    {
        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("logs")]
        public string Logs { get; set; }

        [JsonProperty("errors")]
        public string Errors { get; set; }

        [JsonProperty("balance")]
        public string Balance { get; set; }

        [JsonProperty("pnl")]
        public string PNL { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }
    }
    
    public class AutoTradeMarketsConfigRequest
    {
        [JsonProperty("account")]
        public string Account { get; set; }
    }
}
