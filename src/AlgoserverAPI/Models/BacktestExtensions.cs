using Newtonsoft.Json;

namespace Algoserver.API.Models.REST
{
    public class BacktestExtensions
    {
        [JsonProperty("granularity")]
        public int? Granularity { get; set; }

        [JsonProperty("instrument")]
        public Instrument Instrument { get; set; }

        [JsonProperty("from")]
        public long From { get; set; }

        [JsonProperty("to")]
        public long To { get; set; }
    }
}
