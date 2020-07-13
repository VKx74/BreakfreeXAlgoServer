using Newtonsoft.Json;

namespace Twelvedata.Client.Serialization
{
    public class MessageSerializer
    {
        private static JsonSerializerSettings DefaultSerializerSettings { get; } = new JsonSerializerSettings
        {
            FloatParseHandling = FloatParseHandling.Decimal,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value, DefaultSerializerSettings);
        }

        public static T DeserializeObject<T>(string contentBody)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(contentBody, DefaultSerializerSettings);
            }
            catch
            {
                return default(T);
            }
        }

        public static bool TryDeserializeObject<T>(string contendBody, out T data)
        {
            try
            {
                data = JsonConvert.DeserializeObject<T>(contendBody, DefaultSerializerSettings);
                return data == null ? false : true;
            }
            catch
            {
                data = default(T);
                return false;
            }
        }
    }
}
