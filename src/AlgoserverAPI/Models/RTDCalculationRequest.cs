using Newtonsoft.Json;

namespace Algoserver.API.Models.REST
{

    public class RTDCalculationRequest
    {
        
        [JsonProperty("timeframe")]
        public Timeframe Timeframe { get; set; }
        
        [JsonProperty("instrument")]
        public Instrument Instrument { get; set; }

        [JsonProperty("barsCount")]
        public int BarsCount { get; set; }

        [JsonProperty("inputDataRowName")]
        public string InputDataRowName { get; set; }

        [JsonProperty("fastLimit")]
        public double FastLimit { get; set; }

        [JsonProperty("slowLimit")]
        public double SlowLimit { get; set; }

        [JsonProperty("fastLimit2")]
        public double FastLimit2 { get; set; }

        [JsonProperty("slowLimit2")]
        public double SlowLimit2 { get; set; }

    }
}