using Newtonsoft.Json;

namespace Algoserver.API.Models.REST
{
    public class ReflexCalculationRequest
    {
        
        [JsonProperty("timeframe")]
        public Timeframe Timeframe { get; set; }
        
        [JsonProperty("instrument")]
        public Instrument Instrument { get; set; }

        [JsonProperty("barsCount")]
        public int BarsCount { get; set; }

        [JsonProperty("period")]
        public int Period { get; set; }

        [JsonProperty("superSmooth")]
        public double SuperSmooth { get; set; }

        [JsonProperty("postSmooth")]
        public double PostSmooth { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

    }
}