using Newtonsoft.Json;

namespace Algoserver.Client.WebSocket.Models.Events
{
    public class SubscribeStatusEvent : TwelvedataEvent
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("messages")]
        public string[] Messages { get; set; }
    }
}
