using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_NAS100 : SRLevelStrategyBase
    {
        public SRLevelStrategy_NAS100(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 22,
                CatReflexPeriodSuperSmoother = 43.2,
                CatReflexPeriodPostSmooth = 48,
                CatReflexConfirmationPeriod = 7,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 1.9,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}