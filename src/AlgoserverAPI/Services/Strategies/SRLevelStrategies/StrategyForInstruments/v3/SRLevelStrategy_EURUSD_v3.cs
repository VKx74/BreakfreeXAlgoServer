using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    // Settings version EURUSD_combined_v4.21_2309.set
    public class SRLevelStrategy_EURUSD_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_EURUSD_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.MIN1_GRANULARITY,
                CatReflexPeriodReflex = 27,
                CatReflexPeriodSuperSmoother = 39,
                CatReflexPeriodPostSmooth = 336,
                CatReflexConfirmationPeriod = 3,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 3.4,
                CatReflexValidateZeroCrossover = false,

                UseCatReflex2 = true,
                CatReflexGranularity2 = TimeframeHelper.HOUR4_GRANULARITY,
                CatReflexPeriodReflex2 = 60,
                CatReflexPeriodSuperSmoother2 = 39,
                CatReflexPeriodPostSmooth2 = 48,
                CatReflexConfirmationPeriod2 = 5,
                CatReflexMinLevel2 = 0,
                CatReflexMaxLevel2 = 2,
                CatReflexValidateZeroCrossover2 = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}