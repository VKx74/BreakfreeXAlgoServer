using Newtonsoft.Json;

namespace Algoserver.Client.WebSocket.Models.Events
{
    public class TwelvedataEvent
    {   
        [JsonProperty("event")]
        public string Event { get; set; }
    }
}
