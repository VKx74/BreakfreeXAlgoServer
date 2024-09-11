using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    // 16408 - 1d
    // 16388 - 4h
    // 16385 - 1h
    public class SRLevelStrategy_GeneralReflexOscillator : SRLevelStrategyBase
    {
        public SRLevelStrategy_GeneralReflexOscillator(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 30,
                CatReflexPeriodSuperSmoother = 15,
                CatReflexPeriodPostSmooth = 30,
                CatReflexConfirmationPeriod = 5,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 2,
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}