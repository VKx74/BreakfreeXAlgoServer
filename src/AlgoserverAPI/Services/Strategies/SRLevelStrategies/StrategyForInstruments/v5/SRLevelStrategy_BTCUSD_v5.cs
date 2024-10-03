using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V5
{
    // Settings version BTCUSD_combined_v5_111.set
    public class SRLevelStrategy_BTCUSD_v5 : SRLevelStrategyBase
    {
        public SRLevelStrategy_BTCUSD_v5(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.MIN1_GRANULARITY,
                CatReflexPeriodReflex = 87,
                CatReflexPeriodSuperSmoother = 33,
                CatReflexPeriodPostSmooth = 517,
                CatReflexConfirmationPeriod = 3,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 3.4,
                CatReflexValidateZeroCrossover = false,

                UseCatReflex2 = true,
                CatReflexGranularity2 = TimeframeHelper.HOUR4_GRANULARITY,
                CatReflexPeriodReflex2 = 165,
                CatReflexPeriodSuperSmoother2 = 189,
                CatReflexPeriodPostSmooth2 = 217,
                CatReflexConfirmationPeriod2 = 5,
                CatReflexMinLevel2 = 0,
                CatReflexMaxLevel2 = 2,
                CatReflexValidateZeroCrossover2 = false,

                UseCatReflex3 = true,
                CatReflexGranularity3 = TimeframeHelper.MIN15_GRANULARITY,
                CatReflexPeriodReflex3 = 20,
                CatReflexPeriodSuperSmoother3 = 8,
                CatReflexPeriodPostSmooth3 = 10,
                CatReflexConfirmationPeriod3 = 5,
                CatReflexMinLevel3 = 0,
                CatReflexMaxLevel3 = 2,
                CatReflexValidateZeroCrossover3 = false
            };

            var result = await CalculateInternal(settings);
            
            return result;
        }

    }
}