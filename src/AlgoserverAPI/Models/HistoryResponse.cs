using System.Collections.Generic;

namespace Algoserver.API.Models.REST
{
    public class OandaHistoryResponse
    {
        public HistoryData Data { get; set; }
    } 
    
    public class HistoryData
    {
        public string Symbol { get; set; }
        public string Exchange { get; set; }
        public string Datafeed { get; set; }
        public long Granularity { get; set; }
        public IEnumerable<BarMessage> Bars { get; set; }
    } 
    
    public class OandaInstrumentsResponse
    {
        public IEnumerable<OandaInstruments> Data { get; set; }
    } 
    
    public class OandaInstruments
    {
        public string Datafeed { get; set; }
        public decimal PricePrecision { get; set; }
        public string Symbol { get; set; }
        public string Type { get; set; }
    } 
    
    public class TwelvedatsInstrumentsResponse
    {
        public int Count { get; set; }
        public IEnumerable<TwelvedatsInstruments> Data { get; set; }
    } 
    
    public class TwelvedatsInstruments
    {
        public string Datafeed { get; set; }
        public string Symbol { get; set; }
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
