using System;
using System.Collections.Generic;
using Algoserver.API.Services.CacheServices;

namespace Algoserver.API.Models
{
    [Serializable]
    public class UserDefinedMarketData
    {
        public string symbol { get; set; }
        public int minStrength { get; set; }
        public int minStrength1H { get; set; }
        public int minStrength4H { get; set; }
        public int minStrength1D { get; set; }
    }

    [Serializable]
    public class UserInfoData
    {
        public List<UserDefinedMarketData> markets { get; set; }
        public bool useManualTrading { get; set; }
    }
}