using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_USDCAD : SRLevelStrategyBase
    {
        public SRLevelStrategy_USDCAD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 31,
                CatReflexPeriodSuperSmoother = 91.2,
                CatReflexPeriodPostSmooth = 119,
                CatReflexConfirmationPeriod = 10,
                CatReflexMinLevel = 0.08,
                CatReflexMaxLevel = 1.8,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}