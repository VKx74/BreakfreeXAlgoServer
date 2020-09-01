using Algoserver.API.Models.Algo;

namespace Algoserver.API.Models.REST
{
    public class Strategy2BacktestRequest : BacktestRequest
    {
        public decimal stoploss_rr { get; set; } = 0.15m;
        public bool place_on_sr { get; set; } = true;
        public bool place_on_ex1 { get; set; } = true;
        public bool use_hourly_trend { get; set; } = true;
        public bool use_daily_trend { get; set; } = true;
        public decimal risk_rewards { get; set; } = 1.7m;
        public decimal? hourly_mesa_fast { get; set; } = 0.25m;
        public decimal? hourly_mesa_slow { get; set; } = 0.05m;
        public decimal? hourly_mesa_diff { get; set; } = 0.1m;
    }
}
