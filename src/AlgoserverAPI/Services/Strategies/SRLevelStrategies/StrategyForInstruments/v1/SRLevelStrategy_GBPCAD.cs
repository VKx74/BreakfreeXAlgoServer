using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_GBPCAD : SRLevelStrategyBase
    {
        public SRLevelStrategy_GBPCAD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 44,
                CatReflexPeriodSuperSmoother = 52,
                CatReflexPeriodPostSmooth = 96,
                CatReflexConfirmationPeriod = 10,
                CatReflexMinLevel = 0.08,
                CatReflexMaxLevel = 2.2,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}