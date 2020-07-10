using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Common.Logic.Helpers
{
    public class JsonHelper
    {
        public static string Serialize(object msesage)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            return JsonConvert.SerializeObject(msesage, settings);
        }

        public static T Deserialize<T>(string msesage)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            return JsonConvert.DeserializeObject<T>(msesage, settings);
        }
    }
}
