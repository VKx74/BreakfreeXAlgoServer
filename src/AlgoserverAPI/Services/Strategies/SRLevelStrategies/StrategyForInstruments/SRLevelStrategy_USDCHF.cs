using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_USDCHF : SRLevelStrategyBase
    {
        public SRLevelStrategy_USDCHF(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 15,
                CatReflexPeriodSuperSmoother = 67.2,
                CatReflexPeriodPostSmooth = 165,
                CatReflexConfirmationPeriod = 8,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 21,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}