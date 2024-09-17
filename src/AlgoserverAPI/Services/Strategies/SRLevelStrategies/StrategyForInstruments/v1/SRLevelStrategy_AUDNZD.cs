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
                CatReflexPeriodReflex = 121,
                CatReflexPeriodSuperSmoother = 128,
                CatReflexPeriodPostSmooth = 59,
                CatReflexConfirmationPeriod = 7,
                CatReflexMinLevel = 0.06,
                CatReflexMaxLevel = 1.9,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}