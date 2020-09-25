using Algoserver.API.Models.Algo;

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
    
    public class HittestRequest : BacktestRequest
    {
        public int entry_target_box { get; set; } = 25; // 15%
        public int stoploss_rr { get; set; } = 25; // 15%
    }
}
