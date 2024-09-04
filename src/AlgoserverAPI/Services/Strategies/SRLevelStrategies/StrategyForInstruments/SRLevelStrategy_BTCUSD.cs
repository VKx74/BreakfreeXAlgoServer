using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_BTCUSD : SRLevelStrategyBase
    {
        public SRLevelStrategy_BTCUSD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 54,
                CatReflexPeriodSuperSmoother = 110.4,
                CatReflexPeriodPostSmooth = 131,
                CatReflexConfirmationPeriod = 14,
                CatReflexMinLevel = 0.04,
                CatReflexMaxLevel = 2.0,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}