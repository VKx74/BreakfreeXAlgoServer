namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategySettings
    {
        public SRLevelStrategySettings()
        {
            CheckTrendsStrength = true;
            LowGroupStrength = 0;
            HighGroupStrength = 1;
        }

        // Strength settings
        public bool CheckTrendsStrength { get; set; }
        public decimal LowGroupStrength { get; set; }
        public decimal HighGroupStrength { get; set; }

        // Cat Reflex 1
        public bool UseCatReflex { get; set; }
        public int CatReflexGranularity { get; set; }
        public int CatReflexPeriodReflex { get; set; }
        public double CatReflexPeriodSuperSmoother { get; set; }
        public double CatReflexPeriodPostSmooth { get; set; }
        public double CatReflexMinLevel { get; set; }
        public double CatReflexMaxLevel { get; set; }
        public int CatReflexConfirmationPeriod { get; set; }
        public bool CatReflexValidateZeroCrossover { get; set; }

        // Cat Reflex 2
        public bool UseCatReflex2 { get; set; }
        public int CatReflexGranularity2 { get; set; }
        public int CatReflexPeriodReflex2 { get; set; }
        public double CatReflexPeriodSuperSmoother2 { get; set; }
        public double CatReflexPeriodPostSmooth2 { get; set; }
        public double CatReflexMinLevel2 { get; set; }
        public double CatReflexMaxLevel2 { get; set; }
        public int CatReflexConfirmationPeriod2 { get; set; }
        public bool CatReflexValidateZeroCrossover2 { get; set; }

        // Cat Reflex 3
        public bool UseCatReflex3 { get; set; }
        public int CatReflexGranularity3 { get; set; }
        public int CatReflexPeriodReflex3 { get; set; }
        public double CatReflexPeriodSuperSmoother3 { get; set; }
        public double CatReflexPeriodPostSmooth3 { get; set; }
        public double CatReflexMinLevel3 { get; set; }
        public double CatReflexMaxLevel3 { get; set; }
        public int CatReflexConfirmationPeriod3 { get; set; }
        public bool CatReflexValidateZeroCrossover3 { get; set; }



    }
}