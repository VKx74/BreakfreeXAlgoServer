using System;
using System.Collections.Generic;
using Algoserver.API.Models.Algo;

namespace Algoserver.API.Models.REST
{
    [Serializable]
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

    [Serializable]
    public class ScannerResponseItem
    {
        public Trend trend { get; set; }
        public TradeType type { get; set; }
        public int tte { get; set; }
        public TradeProbability tp { get; set; }
        public int timeframe { get; set; }
        public string exchange { get; set; }
        public string symbol { get; set; }
        public decimal entry { get; set; }
        public decimal stop { get; set; }
    } 
    
    [Serializable]
    public class ScannerResponseHistoryItem
    {
        public ScannerResponseItem responseItem { get; set; }
        public long time { get; set; }
        public decimal avgEntry { get; set; }
    }
    
    [Serializable]
    public class ScannerCacheItem
    {
        public ScannerResponseItem responseItem { get; set; }
        public ScanResponse trade { get; set; }
        public long time { get; set; }
        public decimal avgEntry { get; set; }
    }

    [Serializable]
    public class ScannerResponse
    {
        public IEnumerable<ScannerResponseItem> items { get; set; }
    }

    [Serializable]
    public class ScannerHistoryResponse
    {
        public IEnumerable<ScannerResponseHistoryItem> items { get; set; }
    }
}
