using Newtonsoft.Json;

namespace Algoserver.Client.REST.Response
{
    public class ForexInstrument
    {
        public string Symbol { get; set; }

        [JsonProperty("currency_group")]
        public string CurrencyGroup { get; set; }

        [JsonProperty("currency_base")]
        public string CurrencyBase { get; set; }

        [JsonProperty("currency_quote")]
        public string CurrencyQuote { get; set; }
    }
}
