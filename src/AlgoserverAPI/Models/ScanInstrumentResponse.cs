using Algoserver.API.Models.Algo;

namespace Algoserver.API.Models.REST
{
    public class ScanInstrumentResponse {
        public Trend trend { get; set; }
        public int tte_15 { get; set; }
        public int tte_60 { get; set; }
        public int tte_240 { get; set; }
        public int tp_15 { get; set; }
        public int tp_60 { get; set; }
        public int tp_240 { get; set; }
    }
}
