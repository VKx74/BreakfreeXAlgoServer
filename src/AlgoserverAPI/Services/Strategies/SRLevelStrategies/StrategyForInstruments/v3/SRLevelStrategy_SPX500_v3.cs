using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    // Settings version SPX500_combined_v4.2_2209.set
    public class SRLevelStrategy_SPX500_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_SPX500_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.MIN1_GRANULARITY,
                CatReflexPeriodReflex = 3,
                CatReflexPeriodSuperSmoother = 3,
                CatReflexPeriodPostSmooth = 3,
                CatReflexConfirmationPeriod = 3,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 3.4,
                CatReflexValidateZeroCrossover = false,

                UseCatReflex2 = true,
                CatReflexGranularity2 = TimeframeHelper.HOUR4_GRANULARITY,
                CatReflexPeriodReflex2 = 48,
                CatReflexPeriodSuperSmoother2 = 12,
                CatReflexPeriodPostSmooth2 = 93,
                CatReflexConfirmationPeriod2 = 3,
                CatReflexMinLevel2 = 0,
                CatReflexMaxLevel2 = 3.4,
                CatReflexValidateZeroCrossover2 = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}