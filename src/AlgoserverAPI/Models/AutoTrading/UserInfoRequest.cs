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

    public class UserInfoAddMarketsRequest
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }
        
        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("markets")]
        public List<UserDefinedMarketDataRequest> Markets { get; set; }
    }

    public class UserInfoRemoveMarketsRequest
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }
        
        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("markets")]
        public List<string> Markets { get; set; }
    }

    public class UserInfoChangeUseManualTradingRequest
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }
        
        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("useManualTrading")]
        public bool UseManualTrading { get; set; }
    }
}