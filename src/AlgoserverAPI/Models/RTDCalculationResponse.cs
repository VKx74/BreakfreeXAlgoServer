using System.Collections.Generic;

namespace Algoserver.API.Models.REST
{

    public class RTDCalculationResponse {
        public List<long> dates { get; set; }
        public List<decimal> fast { get; set; }
        public List<decimal> slow { get; set; }
        public List<decimal> fast_2 { get; set; }
        public List<decimal> slow_2 { get; set; }
        public decimal local_trend_spread { get; set; }
        public decimal global_trend_spread { get; set; }
    }
}
