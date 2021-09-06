using System.Collections.Generic;

namespace Algoserver.API.Models.REST
{
    public class ScanningHistory
    {
        public List<decimal> Open { get; set; }
        public List<decimal> High { get; set; }
        public List<decimal> Low { get; set; }
        public List<decimal> Close { get; set; }
        public List<long> Time { get; set; }
    }
}
