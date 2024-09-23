using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    // Settings version XAUUSD_combined_v4_2009.set
    public class SRLevelStrategy_XAUUSD : SRLevelStrategyBase
    {
        public SRLevelStrategy_XAUUSD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.HOUR4_GRANULARITY,
                CatReflexPeriodReflex = 83,
                CatReflexPeriodSuperSmoother = 104,
                CatReflexPeriodPostSmooth = 116,
                CatReflexConfirmationPeriod = 17,
                CatReflexMinLevel = 0.09,
                CatReflexMaxLevel = 2.3,
                CatReflexValidateZeroCrossover = false,
                
                UseCatReflex2 = true,
                CatReflexGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                CatReflexPeriodReflex2 = 14,
                CatReflexPeriodSuperSmoother2 = 27,
                CatReflexPeriodPostSmooth2 = 441,
                CatReflexConfirmationPeriod2 = 7,
                CatReflexMinLevel2 = 0.05,
                CatReflexMaxLevel2 = 3.4,
                CatReflexValidateZeroCrossover2 = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}