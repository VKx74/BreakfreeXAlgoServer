using Algoserver.API.Models.Algo;
using Newtonsoft.Json;

namespace Algoserver.API.Models.REST
{
    public class BacktestRequest : CalculationRequest
    {
        public int? hma_period { get; set; } = 200;
        public decimal? global_fast { get; set; } = 0.25m;
        public decimal? global_slow { get; set; } = 0.05m;
        public decimal? local_fast { get; set; } = 1.2m;
        public decimal? local_slow { get; set; } = 0.6m;
        public decimal? mesa_diff { get; set; } = 0.1m;
        public TrendDetectorType trend_detector { get; set; }
        public int breakeven_candles { get; set; } = 0;
    }

    public class Strategy2BacktestRequest : BacktestRequest
    {
        public decimal stoploss_rr { get; set; } = 0.15m;
        public bool place_on_sr { get; set; } = true;
        public bool place_on_ex1 { get; set; } = true;
        public decimal risk_rewards { get; set; } = 1.7m;
    }

    public class ScannerBacktestRequest : CalculationRequest
    {
        public int breakeven_candles { get; set; } = 0;
        public int cancellation_candles { get; set; } = 0;
        public bool single_position { get; set; }
        public TradeType type { get; set; }
    }

    public class HittestRequest : BacktestRequest
    {
        public int entry_target_box { get; set; } = 25; // 15%
        public int stoploss_rr { get; set; } = 25; // 15%
    }
    
    public class ExtensionsBacktestRequest
    {
        [JsonProperty("granularity")]
        public int? Granularity { get; set; }

        [JsonProperty("instrument")]
        public Instrument Instrument { get; set; }

        [JsonProperty("from")]
        public long From { get; set; }

        [JsonProperty("to")]
        public long To { get; set; }
    }
}
