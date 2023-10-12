using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Helpers;
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
        public List<string> disabledMarkets { get; set; }
        public int accountRisk { get; set; }
        public int defaultMarketRisk { get; set; }
        public bool useManualTrading { get; set; }
        public bool botShutDown { get; set; }

        public UserInfoData()
        {
            risksPerMarket = new Dictionary<string, int>();
            markets = new List<UserDefinedMarketData>();
            disabledMarkets = new List<string>();
            useManualTrading = false;
            botShutDown = false;
            accountRisk = 30;
            defaultMarketRisk = 12;
        }

        public void Validate()
        {
            if (risksPerMarket == null)
            {
                risksPerMarket = new Dictionary<string, int>();
            }

            if (markets == null)
            {
                markets = new List<UserDefinedMarketData>();
            }
            
            if (disabledMarkets == null)
            {
                disabledMarkets = new List<string>();
            }

            if (accountRisk <= 0)
            {
                accountRisk = 30;
            }

            if (defaultMarketRisk <= 0)
            {
                defaultMarketRisk = 12;
            }

            var excludeInstruments = InstrumentsHelper.ExcludeListForLongHistory.Select((_) => InstrumentsHelper.NormalizedInstrumentWithCrypto(_));
            markets.RemoveAll((_) => {
                var normalizedInstrument = InstrumentsHelper.NormalizedInstrumentWithCrypto(_.symbol);
                return excludeInstruments.Any((ex) => string.Equals(ex, normalizedInstrument, StringComparison.InvariantCultureIgnoreCase));
            }); 
            disabledMarkets.RemoveAll((_) => {
                var normalizedInstrument = InstrumentsHelper.NormalizedInstrumentWithCrypto(_);
                return excludeInstruments.Any((ex) => string.Equals(ex, normalizedInstrument, StringComparison.InvariantCultureIgnoreCase));
            });
        }
    }
}