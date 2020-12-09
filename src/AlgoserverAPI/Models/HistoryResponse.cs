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

    public interface IInstrument {
        string Symbol { get; }
        string Datafeed { get; }
        string Exchange { get; }
    }
    
    public class OandaInstruments : IInstrument
    {
        public string Datafeed { get; set; }
        public string Exchange { get; set; } = "Oanda";
        public decimal PricePrecision { get; set; }
        public string Symbol { get; set; }
        public string Type { get; set; }
    } 
    
    public class TwelvedatsInstrumentsResponse
    {
        public int Count { get; set; }
        public IEnumerable<TwelvedataInstruments> Data { get; set; }
    } 
    public class KaikoInstrumentsResponse
    {
        public int Count { get; set; }
        public IEnumerable<KaikoInstruments> Data { get; set; }
    } 
    
    public class TwelvedataInstruments : IInstrument
    {
        public string Datafeed { get; set; } = "Twelvedata";
        public string Exchange { get; set; }
        public string Symbol { get; set; }
    }
    public class KaikoInstruments : IInstrument
    {
        public string Datafeed { get; set; } = "Kaiko";
        public string Symbol { get; set; }
        public string Exchange { get; set; }
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
