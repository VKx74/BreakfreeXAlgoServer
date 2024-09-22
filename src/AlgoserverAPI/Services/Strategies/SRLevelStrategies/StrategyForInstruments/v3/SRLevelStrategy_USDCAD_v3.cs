using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    // Settings version USDCAD_combined_v4.2_2209.set
    public class SRLevelStrategy_USDCAD_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_USDCAD_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.HOUR4_GRANULARITY,
                CatReflexPeriodReflex = 6,
                CatReflexPeriodSuperSmoother = 48,
                CatReflexPeriodPostSmooth = 534,
                CatReflexConfirmationPeriod = 3,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 3.4,
                CatReflexValidateZeroCrossover = false,

                UseCatReflex2 = true,
                CatReflexGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                CatReflexPeriodReflex2 = 12,
                CatReflexPeriodSuperSmoother2 = 15,
                CatReflexPeriodPostSmooth2 = 408,
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