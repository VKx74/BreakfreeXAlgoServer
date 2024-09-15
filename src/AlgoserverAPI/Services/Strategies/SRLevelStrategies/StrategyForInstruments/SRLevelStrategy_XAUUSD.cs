using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_XAUUSD : SRLevelStrategyBase
    {
        public SRLevelStrategy_XAUUSD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 64,
                CatReflexPeriodSuperSmoother = 38.4,
                CatReflexPeriodPostSmooth = 49,
                CatReflexConfirmationPeriod = 3,
                CatReflexMinLevel = 0.0,
                CatReflexMaxLevel = 1.8,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}