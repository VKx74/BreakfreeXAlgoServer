using System.Collections.Generic;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;
using Algoserver.API.Services;

namespace Algoserver.Strategies.NLevelStrategy
{
    public class NLevelStrategyInputContext
    {
        public AutoTradingSymbolInfoResponse symbolInfo { get; set; }
        public MESADataSummary mesaResponse { get; set; }
        public MesaResponse mesaAdditional { get; set; }
        public Dictionary<int, LevelsV3Response> levelsResponse { get; set; }
        public string symbol { get; set; }
        public string datafeed { get; set; }
        public string exchange { get; set; }
        public string type { get; set; }
        public HistoryService historyService { get; set; }
    }
}