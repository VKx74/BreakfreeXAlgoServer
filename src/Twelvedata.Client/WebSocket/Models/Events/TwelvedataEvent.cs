using Newtonsoft.Json;

namespace Twelvedata.Client.WebSocket.Models.Events
{
    public class TwelvedataEvent
    {   
        [JsonProperty("event")]
        public string Event { get; set; }
    }
}
