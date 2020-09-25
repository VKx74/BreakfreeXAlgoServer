using System.Collections.Generic;
using Algoserver.API.Helpers;
using Algoserver.API.Models.Algo;
using Algoserver.API.Services;
using Newtonsoft.Json;

namespace Algoserver.API.Models.REST
{
    public class Strategy2CalculationResponse
    {

        [JsonProperty("top_ex2")]
        public decimal top_ex2 { get; set; }

        [JsonProperty("top_ex1")]
        public decimal top_ex1 { get; set; }

        [JsonProperty("r")]
        public decimal r { get; set; }

        [JsonProperty("n")]
        public decimal n { get; set; }

        [JsonProperty("s")]
        public decimal s { get; set; }

        [JsonProperty("bottom_ex1")]
        public decimal bottom_ex1 { get; set; }

        [JsonProperty("bottom_ex2")]
        public decimal bottom_ex2 { get; set; }

        [JsonProperty("trend")]
        public Trend trend { get; set; }
        
        [JsonProperty("trade_sr")]
        public TradeEntryV2Result trade_sr { get; set; }

        [JsonProperty("trade_ex1")]
        public TradeEntryV2Result trade_ex1 { get; set; }
    }

    public class Strategy2BacktestSignal
    {
        public long timestamp { get; set; }
        public long end_timestamp { get; set; }
        public Strategy2CalculationResponse data { get; set; }
    }
}
