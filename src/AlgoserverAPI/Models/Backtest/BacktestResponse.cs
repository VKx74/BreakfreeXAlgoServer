using System.Collections.Generic;
using Algoserver.API.Helpers;
using Algoserver.API.Models.Algo;
using Algoserver.API.Models.Broker;

namespace Algoserver.API.Models.REST
{   
    public class BacktestSignal
    {
        public long timestamp { get; set; }
        public long end_timestamp { get; set; }
        public CalculationResponse data { get; set; }
    }

    public class ScannerBacktestSignal
    {
        public long timestamp { get; set; }
        public long end_timestamp { get; set; }
        public CalculationResponseV2 data { get; set; }
    }

    public class ExtHitTestSignal
    {
        public long timestamp { get; set; }
        public long end_timestamp { get; set; }
        public decimal top_sl { get; set; }
        public decimal bottom_sl { get; set; }
        public decimal top_entry { get; set; }
        public decimal bottom_entry { get; set; }
        public CalculationResponse data { get; set; }
        public bool topext1hit { get; set; }
        public bool bottomext1hit { get; set; }
        public bool backhit { get; set; }
        public bool wentout { get; set; }
        public bool breakeven { get; set; }
        public Trend trend { get; set; }
    }

    public class Strategy2BacktestSignal
    {
        public long timestamp { get; set; }
        public long end_timestamp { get; set; }
        public Strategy2BacktestData data { get; set; }
    }

    public class Strategy2BacktestData
    {
        public decimal top_ex2 { get; set; }
        public decimal top_ex1 { get; set; }
        public decimal r { get; set; }
        public decimal n { get; set; }
        public decimal s { get; set; }
        public decimal bottom_ex1 { get; set; }
        public decimal bottom_ex2 { get; set; }
        public Trend trend { get; set; }
        public TradeEntryV2Result trade_sr { get; set; }
        public TradeEntryV2Result trade_ex1 { get; set; }
    }
 
    public class BacktestResponse 
    {
        public List<BacktestSignal> signals { get; set; }
        public List<Order> orders { get; set; }
    }
    public class ScannerBacktestResponse 
    {
        public List<ScannerBacktestSignal> signals { get; set; }
        public List<Order> orders { get; set; }
    }
    
     public class ExtensionBacktestResponse {
        public CalculationLevels levels {get;set;}
        public long timestamp { get; set; }
    }
    
    public class Strategy2BacktestResponse 
    {
        public List<Strategy2BacktestSignal> signals { get; set; }
        public List<Order> orders { get; set; }
    }

    public class ExtHitTestResponse 
    {
        public List<ExtHitTestSignal> signals { get; set; }
    }
}
