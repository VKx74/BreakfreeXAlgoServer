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
        public Dictionary<string, int> risksPerMarket { get; set; }
        public List<UserDefinedMarketData> markets { get; set; }
        public int accountRisk { get; set; }
        public int defaultMarketRisk { get; set; }
        public bool useManualTrading { get; set; }

        public UserInfoData()
        {
            risksPerMarket = new Dictionary<string, int>();
            markets = new List<UserDefinedMarketData>();
            useManualTrading = false;
            accountRisk = 30;
            defaultMarketRisk = 25;
        }
    }
}