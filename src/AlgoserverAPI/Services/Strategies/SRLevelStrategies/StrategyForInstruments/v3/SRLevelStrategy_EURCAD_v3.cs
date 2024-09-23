using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    // Settings version EURCAD_combined_v4.21_2309.set
    public class SRLevelStrategy_EURCAD_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_EURCAD_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.MIN1_GRANULARITY,
                CatReflexPeriodReflex = 6,
                CatReflexPeriodSuperSmoother = 63,
                CatReflexPeriodPostSmooth = 118,
                CatReflexConfirmationPeriod = 3,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 3.4,
                CatReflexValidateZeroCrossover = false,

                UseCatReflex2 = true,
                CatReflexGranularity2 = TimeframeHelper.HOUR4_GRANULARITY,
                CatReflexPeriodReflex2 = 72,
                CatReflexPeriodSuperSmoother2 = 24,
                CatReflexPeriodPostSmooth2 = 271,
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