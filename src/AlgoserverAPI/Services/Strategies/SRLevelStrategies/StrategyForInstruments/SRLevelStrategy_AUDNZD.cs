using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_AUDNZD : SRLevelStrategyBase
    {
        public SRLevelStrategy_AUDNZD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 5,
                CatReflexPeriodSuperSmoother = 128,
                CatReflexPeriodPostSmooth = 167,
                CatReflexConfirmationPeriod = 6,
                CatReflexMinLevel = 0.04,
                CatReflexMaxLevel = 1.7,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}