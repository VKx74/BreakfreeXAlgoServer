using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_GBPCHF : SRLevelStrategyBase
    {
        public SRLevelStrategy_GBPCHF(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 9,
                CatReflexPeriodSuperSmoother = 49.6,
                CatReflexPeriodPostSmooth = 68,
                CatReflexConfirmationPeriod = 7,
                CatReflexMinLevel = 0.04,
                CatReflexMaxLevel = 1.8,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}