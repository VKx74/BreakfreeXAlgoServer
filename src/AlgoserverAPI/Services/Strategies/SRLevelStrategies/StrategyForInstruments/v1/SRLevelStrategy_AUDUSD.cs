using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_AUDUSD : SRLevelStrategyBase
    {
        public SRLevelStrategy_AUDUSD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 89,
                CatReflexPeriodSuperSmoother = 131.2,
                CatReflexPeriodPostSmooth = 12,
                CatReflexConfirmationPeriod = 6,
                CatReflexMinLevel = 0.1,
                CatReflexMaxLevel = 1.9,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}