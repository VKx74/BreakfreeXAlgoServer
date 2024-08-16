namespace Algoserver.Strategies.NLevelStrategy
{
    public class NLevelStrategyResponse
    {
        public uint State { get; set; }
        public uint StrategyType { get; set; }
        public decimal SL1M { get; set; }
        public decimal SL5M { get; set; }
        public decimal SL15M { get; set; }
        public decimal SL1H { get; set; }
        public decimal SL4H { get; set; }
        public decimal OppositeSL1M { get; set; }
        public decimal OppositeSL5M { get; set; }
        public decimal OppositeSL15M { get; set; }
        public decimal OppositeSL1H { get; set; }
        public decimal OppositeSL4H { get; set; }
        public bool Skip1MinTrades { get; set; }
        public bool Skip5MinTrades { get; set; }
        public bool Skip15MinTrades { get; set; }
        public bool Skip1HourTrades { get; set; }
        public bool Skip4HourTrades { get; set; }
        public decimal MinStrength1M { get; set; }
        public decimal MinStrength5M { get; set; }
        public decimal MinStrength15M { get; set; }
        public decimal MinStrength1H { get; set; }
        public decimal MinStrength4H { get; set; }
        public bool DDClosePositions { get; set; }
        public int DDCloseInitialInterval { get; set; }
        public int DDCloseIncreasePeriod { get; set; }
        public decimal DDCloseIncreaseThreshold { get; set; }
    }

}