using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_SPX500 : SRLevelStrategyBase
    {
        public SRLevelStrategy_SPX500(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 50,
                CatReflexPeriodSuperSmoother = 86.4,
                CatReflexPeriodPostSmooth = 114,
                CatReflexConfirmationPeriod = 2,
                CatReflexMinLevel = 0.07,
                CatReflexMaxLevel = 1.9,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}