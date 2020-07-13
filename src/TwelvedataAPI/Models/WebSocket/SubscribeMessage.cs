using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Algoserver.API.Models.WebSocket
{
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChannelType
    {
        [EnumMember(Value = "trade")]
        Trade,
        [EnumMember(Value = "level2")]
        Level2
    }

    public class SubscribeMessage : BaseMessage
    {
        public SubscribeMessage()
        {
            MsgType = nameof(SubscribeMessage);
        }

        public ChannelType Channel { get; set; }

        public string Product { get; set; }

        public string Market { get; set; }

        public bool IsSubscribe { get; set; }

        public string AccessToken { get; set; }
    }
}
