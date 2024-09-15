using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_CADJPY : SRLevelStrategyBase
    {
        public SRLevelStrategy_CADJPY(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 169,
                CatReflexPeriodSuperSmoother = 60.8,
                CatReflexPeriodPostSmooth = 59,
                CatReflexConfirmationPeriod = 2,
                CatReflexMinLevel = 0.09,
                CatReflexMaxLevel = 2.1,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}