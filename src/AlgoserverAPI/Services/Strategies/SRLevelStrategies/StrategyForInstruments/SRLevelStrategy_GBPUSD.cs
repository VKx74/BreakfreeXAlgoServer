using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_GBPUSD : SRLevelStrategyBase
    {
        public SRLevelStrategy_GBPUSD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 26,
                CatReflexPeriodSuperSmoother = 148,
                CatReflexPeriodPostSmooth = 103,
                CatReflexConfirmationPeriod = 8,
                CatReflexMinLevel = 0.09,
                CatReflexMaxLevel = 1.7,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}