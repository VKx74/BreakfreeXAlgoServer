using Newtonsoft.Json;

namespace Algoserver.Client.WebSocket.Models.Events
{
    public class PriceEvent : TwelvedataEvent
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("currency_base")]
        public string CurrencyBase { get; set; }

        [JsonProperty("currency_quote")]
        public string CurrencyQuote { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("timestamp")]
        public long UnixTimestamp { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }
    }
}
