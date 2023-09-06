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
    }
}