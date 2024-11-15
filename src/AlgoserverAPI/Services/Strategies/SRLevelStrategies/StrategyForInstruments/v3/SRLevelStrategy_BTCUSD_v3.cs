using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    // Settings version BTCUSD_combined_v4.2_2209.set
    public class SRLevelStrategy_BTCUSD_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_BTCUSD_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.MIN1_GRANULARITY,
                CatReflexPeriodReflex = 55,
                CatReflexPeriodSuperSmoother = 53,
                CatReflexPeriodPostSmooth = 279,
                CatReflexConfirmationPeriod = 3,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 34,
                CatReflexValidateZeroCrossover = false,

                UseCatReflex2 = true,
                CatReflexGranularity2 = TimeframeHelper.HOUR4_GRANULARITY,
                CatReflexPeriodReflex2 = 4,
                CatReflexPeriodSuperSmoother2 = 16,
                CatReflexPeriodPostSmooth2 = 96,
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