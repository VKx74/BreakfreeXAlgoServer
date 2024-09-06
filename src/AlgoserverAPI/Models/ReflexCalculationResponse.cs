using System.Collections.Generic;

namespace Algoserver.API.Models.REST
{
    public class ReflexCalculationResponse {
        public IEnumerable<long> dates { get; set; }
        public IEnumerable<double> values { get; set; }
        public string id { get; set; }
    }
}
