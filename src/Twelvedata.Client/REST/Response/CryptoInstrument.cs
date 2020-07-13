using Newtonsoft.Json;

namespace Twelvedata.Client.REST.Response
{
    public class CryptoInstrument
    {
        public string Symbol { get; set; }

        [JsonProperty("available_exchanges")]
        public string[] AvailableExchanges { get; set; }

        [JsonProperty("currency_base")]
        public string CurrencyBase { get; set; }

        [JsonProperty("currency_quote")]
        public string CurrencyQuote { get; set; }
    }

}
