using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_NZDJPY : SRLevelStrategyBase
    {
        public SRLevelStrategy_NZDJPY(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 52,
                CatReflexPeriodSuperSmoother = 119.2,
                CatReflexPeriodPostSmooth = 180,
                CatReflexConfirmationPeriod = 8,
                CatReflexMinLevel = 0.05,
                CatReflexMaxLevel = 2,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}