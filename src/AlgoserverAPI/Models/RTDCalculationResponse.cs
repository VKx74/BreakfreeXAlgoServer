using System.Collections.Generic;

namespace Algoserver.API.Models.REST
{
    public class RTDCalculationResponse {
        public long[] dates { get; set; }
        public decimal[] fast { get; set; }
        public decimal[] slow { get; set; }
        public decimal[] fast_2 { get; set; }
        public decimal[] slow_2 { get; set; }
        public decimal local_trend_spread { get; set; }
        public decimal global_trend_spread { get; set; }
    }
}
