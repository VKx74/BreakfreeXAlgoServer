using Newtonsoft.Json;
using System.Reflection;

namespace Algoserver.Client.WebSocket.Models.Actions
{
    public abstract class TwelvedataAction
    {
        [JsonProperty("action")]
        public string MesssagenType => GetType().GetCustomAttribute<MessageTypeAttribute>()?.Type;
    }
}
