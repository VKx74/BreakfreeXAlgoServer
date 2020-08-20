using Algoserver.API.Models.Algo;

namespace Algoserver.API.Models.REST
{
    public class BacktestRequest : CalculationRequest
    {
        public int? hma_period { get; set; } = 200;
        public decimal? mesa_fast { get; set; } = 0.5m;
        public decimal? mesa_slow { get; set; } = 0.05m;
        public decimal? mesa_diff { get; set; } = 0.00001m;
        public TrendDetectorType trend_detector { get; set; }
        public int breakeven_candles { get; set; } = 0;
    }
    
    public class HittestRequest : BacktestRequest
    {
        public int entry_target_box { get; set; } = 25; // 15%
        public int stoploss_rr { get; set; } = 25; // 15%
    }
}
