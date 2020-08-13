namespace Algoserver.API.Models.REST
{
    public class BacktestRequest : CalculationRequest
    {
        public int hma_period { get; set; } = 200;
        public int breakeven_candles { get; set; } = 0;
    }
    
    public class HittestRequest : BacktestRequest
    {
        public int entry_target_box { get; set; } = 25; // 15%
        public int stoploss_rr { get; set; } = 25; // 15%
    }
}
