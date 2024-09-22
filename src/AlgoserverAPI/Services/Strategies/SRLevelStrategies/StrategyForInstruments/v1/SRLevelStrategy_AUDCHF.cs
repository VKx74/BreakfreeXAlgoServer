using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_AUDCHF : SRLevelStrategyBase
    {
        public SRLevelStrategy_AUDCHF(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 130,
                CatReflexPeriodSuperSmoother = 194.4,
                CatReflexPeriodPostSmooth = 123,
                CatReflexConfirmationPeriod = 17,
                CatReflexMinLevel = 0.08,
                CatReflexMaxLevel = 2.2,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}