using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Algoserver.API.Models.REST
{
    public class UserDefinedMarketDataRequest
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("minStrength")]
        public int MinStrength { get; set; }

        [JsonProperty("minStrength1H")]
        public int MinStrength1H { get; set; }

        [JsonProperty("minStrength4H")]
        public int MinStrength4H { get; set; }

        [JsonProperty("minStrength1D")]
        public int MinStrength1D { get; set; }
    }

    [Serializable]
    public class UserInfoDataRequest
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }
        
        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("markets")]
        public List<UserDefinedMarketDataRequest> Markets { get; set; }

        [JsonProperty("useManualTrading")]
        public bool UseManualTrading { get; set; }
    }
}