using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_USDZAR : SRLevelStrategyBase
    {
        public SRLevelStrategy_USDZAR(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 78,
                CatReflexPeriodSuperSmoother = 72,
                CatReflexPeriodPostSmooth = 82,
                CatReflexConfirmationPeriod = 2,
                CatReflexMinLevel = 0.04,
                CatReflexMaxLevel = 2.3,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}