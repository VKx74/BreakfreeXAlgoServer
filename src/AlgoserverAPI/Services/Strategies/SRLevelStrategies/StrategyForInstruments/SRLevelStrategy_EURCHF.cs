using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_EURCHF : SRLevelStrategyBase
    {
        public SRLevelStrategy_EURCHF(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 9,
                CatReflexPeriodSuperSmoother = 14.4,
                CatReflexPeriodPostSmooth = 68,
                CatReflexConfirmationPeriod = 15,
                CatReflexMinLevel = 0.4,
                CatReflexMaxLevel = 1.7,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}