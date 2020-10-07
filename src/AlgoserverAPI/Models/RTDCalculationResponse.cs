using System.Collections.Generic;

namespace Algoserver.API.Models.REST
{

    public class RTDCalculationResponse {
        public IEnumerable<long> dates { get; set; }
        public IEnumerable<decimal> fast { get; set; }
        public IEnumerable<decimal> slow { get; set; }
        public IEnumerable<decimal> fast_2 { get; set; }
        public IEnumerable<decimal> slow_2 { get; set; }
    }
}