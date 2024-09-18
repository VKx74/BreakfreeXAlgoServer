using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    public class SRLevelStrategy_CADJPY_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_CADJPY_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 141,
                CatReflexPeriodSuperSmoother = 188,
                CatReflexPeriodPostSmooth = 97,
                CatReflexConfirmationPeriod = 16,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 2.3,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}