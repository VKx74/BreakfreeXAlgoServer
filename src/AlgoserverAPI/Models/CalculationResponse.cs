using System;
using System.Collections.Generic;
using Algoserver.API.Helpers;
using Algoserver.API.Models.Algo;
using Algoserver.API.Services;
using Newtonsoft.Json;

namespace Algoserver.API.Models.REST
{
    public class CalculationResponse
    {
        public CalculationLevels levels { get; set; }
        public StrategyModeV1 trade { get; set; }
    }

    [Serializable]
    public class CalculationMarketInfoResponse
    {
        public decimal daily_support { get; set; }
        public decimal daily_resistance { get; set; }
        public decimal daily_natural { get; set; }
        public decimal support { get; set; }
        public decimal resistance { get; set; }
        public decimal natural { get; set; }
        public decimal last_price { get; set; }
        public Trend local_trend { get; set; }
        public Trend global_trend { get; set; }
        public bool is_overhit { get; set; }
        public decimal local_trend_spread { get; set; }
        public decimal global_trend_spread { get; set; }
        public decimal cvar { get; set; }
    }

    [Serializable]
    public class CVarInfoResponse
    {
        public decimal? cvar { get; set; }
    }

    public class CalculationResponseV2
    {
        public CalculationLevels levels { get; set; }
        public StrategyModeV2 trade { get; set; }
        public decimal size { get; set; }
        public string id { get; set; }
    }

    public class CalculationResponseV3
    {
        public List<SaRResponse> sar { get; set; }
        public Dictionary<int, List<SaRResponse>> sar_additional { get; set; }
        public List<SaRResponse> sar_prediction { get; set; }
        public List<decimal> mema_prediction { get; set; }
        public List<decimal> fama_prediction { get; set; }
        public CalculationLevels levels { get; set; }
        public StrategyModeV2 trade { get; set; }
        public decimal? support_prob { get; set; }
        public decimal? resistance_prob { get; set; }
        public decimal? support_ext_prob { get; set; }
        public decimal? resistance_ext_prob { get; set; }
        public decimal size { get; set; }
        public string id { get; set; }
        public RTDCalculationResponse rtd { get; set; }
        public bool prediction_exists { get; set; }
    }

    public class CalculatePositionSizeResponse
    {
        public decimal size { get; set; }
    }

    public class CalculatePriceRatioResponse
    {
        public decimal ratio { get; set; }
    }

    public class CalculationLevels
    {
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
    }

    public class SaRResponse
    {
        public decimal r_p28 { get; set; }
        public decimal r_p18 { get; set; }
        public decimal r { get; set; }
        public decimal n { get; set; }
        public decimal s_m28 { get; set; }
        public decimal s_m18 { get; set; }
        public decimal s { get; set; }
        public long date { get; set; }
    }

    public class StrategyModeV2
    {
        public Trend trend { get; set; }
        public TradeType type { get; set; }
        public int tte { get; set; }
        public TradeProbability tp { get; set; }
        public decimal entry { get; set; }
        public decimal entry_h { get; set; }
        public decimal entry_l { get; set; }
        public decimal take_profit { get; set; }
        public decimal take_profit_h { get; set; }
        public decimal take_profit_l { get; set; }
        public decimal stop { get; set; }
        public decimal sl_ratio { get; set; }
        public decimal risk { get; set; }
    }

    public class StrategyModeV1
    {

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


    public class MLDataResponseItem
    {
        public decimal open { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal close { get; set; }
        public long time { get; set; }
        public decimal n { get; set; }
        public decimal s { get; set; }
        public decimal r { get; set; }
        public decimal upExt1 { get; set; }
        public decimal upExt2 { get; set; }
        public decimal downExt1 { get; set; }
        public decimal downExt2 { get; set; }
    }

    public class MLDataResponse
    {
        public List<MLDataResponseItem> data { get; set; }
    }

}
