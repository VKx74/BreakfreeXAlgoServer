using System.Collections.Generic;

namespace Algoserver.API.Models.REST
{
    public class HistoryResponse
    {
        public string Symbol { get; set; }
        public string Exchange { get; set; }
        public string Datafeed { get; set; }
        public long Granularity { get; set; }
        public IEnumerable<BarMessage> Bars { get; set; }
    }

    public class BarMessage
    {
        public long Timestamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }
}
