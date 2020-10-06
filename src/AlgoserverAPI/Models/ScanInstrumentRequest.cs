using Newtonsoft.Json;

namespace Algoserver.API.Models.REST
{
    public class ScanInstrumentRequest {

        [JsonProperty("instrument")]
        public Instrument Instrument { get; set; }
    }
}