using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_DE30 : SRLevelStrategyBase
    {
        public SRLevelStrategy_DE30(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 28,
                CatReflexPeriodSuperSmoother = 54.4,
                CatReflexPeriodPostSmooth = 123,
                CatReflexConfirmationPeriod = 13,
                CatReflexMinLevel = 0.08,
                CatReflexMaxLevel = 2.3,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}