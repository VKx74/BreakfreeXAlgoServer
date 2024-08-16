namespace Algoserver.Strategies.NLevelStrategy
{
    public class NLevelStrategySettings
    {
        // Volatility settings
        public bool UseVolatilityFilter { get; set; }
        public int VolatilityGranularity { get; set; }
        public int VolatilityMin { get; set; }
        public int VolatilityMax { get; set; }

        // Zone settings
        public bool UseOverheatZone1DFilter { get; set; }
        public decimal OverheatZone1DThreshold { get; set; }
        public bool UseOverheatZone4HFilter { get; set; }
        public decimal OverheatZone4HThreshold { get; set; }
        public bool UseOverheatZone1HFilter { get; set; }
        public decimal OverheatZone1HThreshold { get; set; }

        // Trend settings
        public bool CheckTrends { get; set; }
        public bool CheckTrendsStrength { get; set; }
        public decimal LowGroupStrength { get; set; }
        public decimal HighGroupStrength { get; set; }

        // RSI filter
        public bool CheckRSI { get; set; }
        public int RSIMin { get; set; }
        public int RSIMax { get; set; }
        public int RSIPeriod { get; set; }

        // Strength increase filter
        public bool CheckStrengthIncreasing { get; set; }
        public int CheckStrengthReducePeriod { get; set; }
        public int CheckStrengthResetPeriod { get; set; }
        public int CheckStrengthReduceGranularity { get; set; }
        public int CheckStrengthResetGranularity { get; set; }

        // Strength increase filter
        public bool CheckPeaks { get; set; }
        public int PeakDetectionGranularity { get; set; }
        public int PeakDetectionPeriod { get; set; }
        public int PeakDetectionThreshold { get; set; }
    }
}