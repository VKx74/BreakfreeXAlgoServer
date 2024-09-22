using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_NZDCHF : SRLevelStrategyBase
    {
        public SRLevelStrategy_NZDCHF(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 17,
                CatReflexPeriodSuperSmoother = 154.4,
                CatReflexPeriodPostSmooth = 98,
                CatReflexConfirmationPeriod = 8,
                CatReflexMinLevel = 0.04,
                CatReflexMaxLevel = 2.3,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}