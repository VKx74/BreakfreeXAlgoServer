using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_GBPAUD : SRLevelStrategyBase
    {
        public SRLevelStrategy_GBPAUD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 28,
                CatReflexPeriodSuperSmoother = 115.2,
                CatReflexPeriodPostSmooth = 168,
                CatReflexConfirmationPeriod = 7,
                CatReflexMinLevel = 0.04,
                CatReflexMaxLevel = 1.7,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}