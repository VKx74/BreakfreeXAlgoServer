using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    public class SRLevelStrategy_SPX500_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_SPX500_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.HOUR4_GRANULARITY,
                CatReflexPeriodReflex = 48,
                CatReflexPeriodSuperSmoother = 18,
                CatReflexPeriodPostSmooth = 207,
                CatReflexConfirmationPeriod = 1,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 2.0,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}