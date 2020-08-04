using System.Collections.Generic;
using Algoserver.API.Services;
using Newtonsoft.Json;

namespace Algoserver.API.Models.REST
{
    public class CalculationResponse
    {
        [JsonProperty("clean")]
        public bool Clean { get; set; }

        [JsonProperty("ee")]
        public decimal EE { get; set; }

        [JsonProperty("ee1")]
        public decimal EE1 { get; set; }

        [JsonProperty("ee2")]
        public decimal EE2 { get; set; }

        [JsonProperty("ee3")]
        public decimal EE3 { get; set; }

        [JsonProperty("fe")]
        public decimal FE { get; set; }

        [JsonProperty("fe1")]
        public decimal FE1 { get; set; }

        [JsonProperty("fe2")]
        public decimal FE2 { get; set; }

        [JsonProperty("fe3")]
        public decimal FE3 { get; set; }

        [JsonProperty("ze")]
        public decimal ZE { get; set; }

        [JsonProperty("ze1")]
        public decimal ZE1 { get; set; }

        [JsonProperty("ze2")]
        public decimal ZE2 { get; set; }

        [JsonProperty("ze3")]
        public decimal ZE3 { get; set; }

        [JsonProperty("vr100")]
        public bool VR100 { get; set; }

        [JsonProperty("vr75a")]
        public bool VR75A { get; set; }

        [JsonProperty("vr75b")]
        public bool VR75B { get; set; }

        [JsonProperty("vn100")]
        public bool VN100 { get; set; }

        [JsonProperty("vn75a")]
        public bool VN75A { get; set; }

        [JsonProperty("vn75b")]
        public bool VN75B { get; set; }

        [JsonProperty("vs100")]
        public bool VS100 { get; set; }

        [JsonProperty("vs75a")]
        public bool VS75A { get; set; }

        [JsonProperty("vs75b")]
        public bool VS75B { get; set; }

        [JsonProperty("vscs")]
        public bool VSCS { get; set; }

        [JsonProperty("vscs2")]
        public bool VSCS2 { get; set; }

        [JsonProperty("vexttp")]
        public bool VEXTTP { get; set; }

        [JsonProperty("vexttp2")]
        public bool VEXTTP2 { get; set; }

        [JsonProperty("m18")]
        public decimal M18 { get; set; }

        [JsonProperty("m28")]
        public decimal M28 { get; set; }

        [JsonProperty("p18")]
        public decimal P18 { get; set; }

        [JsonProperty("p28")]
        public decimal P28 { get; set; }

        [JsonProperty("algo_TP2")]
        public decimal? AlgoTP2 { get; set; }

        [JsonProperty("algo_TP1_high")]
        public decimal? AlgoTP1High { get; set; }

        [JsonProperty("algo_TP1_low")]
        public decimal? AlgoTP1Low { get; set; }

        [JsonProperty("algo_Entry_high")]
        public decimal? AlgoEntryHigh { get; set; }

        [JsonProperty("algo_Entry_low")]
        public decimal? AlgoEntryLow { get; set; }

        [JsonProperty("algo_Entry")]
        public decimal? AlgoEntry { get; set; }

        [JsonProperty("algo_Stop")]
        public decimal? AlgoStop { get; set; }

        [JsonProperty("algo_Risk")]
        public decimal AlgoRisk { get; set; }

        [JsonProperty("algo_Info")]
        public AlgoInfo AlgoInfo { get; set; }
    }

    public class AlgoInfo
    {
        [JsonProperty("objective")]
        public string Objective { get; set; } // None

        [JsonProperty("status")]
        public string Status { get; set; } // Trade found

        [JsonProperty("suggestedrisk")]
        public string Suggestedrisk { get; set; }

        [JsonProperty("positionsize")]
        public string Positionsize { get; set; } // 0 | Split: 0

        [JsonProperty("pas")]
        public string Pas { get; set; } // Market is neutral

        [JsonProperty("macrotrend")]
        public string Macrotrend { get; set; }

        [JsonProperty("n_currencySymbol")]
        public string NCurrencySymbol { get; set; }
    }

    public class BacktestSignal { 
        public long timestamp { get; set; }
        public CalculationResponse data { get; set; }
    } 

    public class ExtHitTestSignal { 
        public long timestamp { get; set; }
        public CalculationResponse data { get; set; }
        public bool topext1hit { get; set; }
        public bool topext2hit { get; set; }
        public bool bottomext1hit { get; set; }
        public bool bottomext2hit { get; set; }
        public bool backhit { get; set; }
        public bool wentout { get; set; }
    } 
    
    public class BacktestAction { 
        public long timestamp { get; set; }
        public CalculationResponse data { get; set; }
    }
}
