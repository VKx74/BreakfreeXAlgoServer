using System.Collections.Generic;
using Algoserver.API.Models.Algo;

namespace Algoserver.API.Models.REST
{
    public class ScanInstrumentResponse
    {
        public Trend trend { get; set; }
        public TradeType type { get; set; }
        public int tte_15 { get; set; }
        public int tte_60 { get; set; }
        public int tte_240 { get; set; }
        public int tp_15 { get; set; }
        public int tp_60 { get; set; }
        public int tp_240 { get; set; }
    }

    public class ScannerResponseItem
    {
        public Trend trend { get; set; }
        public TradeType type { get; set; }
        public int tte { get; set; }
        public int tp { get; set; }
        public int timeframe { get; set; }
        public string exchange { get; set; }
        public string symbol { get; set; }
    }

    public class ScannerResponse
    {
        public IEnumerable<ScannerResponseItem> items { get; set; }
        public int scanning_time { get; set; }
        public int data_count_15_min { get; set; }
        public int data_count_1_h { get; set; }
        public int data_count_4_h { get; set; }
        public int data_count_1_d { get; set; }
        public string refresh_time { get; set; }
        public string refresh_time_all { get; set; }
    }
}
