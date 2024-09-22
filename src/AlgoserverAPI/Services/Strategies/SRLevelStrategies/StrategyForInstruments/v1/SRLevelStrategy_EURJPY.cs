using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_EURJPY : SRLevelStrategyBase
    {
        public SRLevelStrategy_EURJPY(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 26,
                CatReflexPeriodSuperSmoother = 29.6,
                CatReflexPeriodPostSmooth = 38,
                CatReflexConfirmationPeriod = 7,
                CatReflexMinLevel = 0.08,
                CatReflexMaxLevel = 2.3,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}