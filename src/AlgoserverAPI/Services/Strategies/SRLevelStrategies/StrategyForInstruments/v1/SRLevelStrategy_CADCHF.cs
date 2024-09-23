using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    // Settings version CADCHF_combined_v2_1609.set
    public class SRLevelStrategy_CADCHF : SRLevelStrategyBase
    {
        public SRLevelStrategy_CADCHF(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.HOURLY_GRANULARITY,
                CatReflexPeriodReflex = 87,
                CatReflexPeriodSuperSmoother = 212,
                CatReflexPeriodPostSmooth = 22,
                CatReflexConfirmationPeriod = 3,
                CatReflexMinLevel = 0.01,
                CatReflexMaxLevel = 2,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}