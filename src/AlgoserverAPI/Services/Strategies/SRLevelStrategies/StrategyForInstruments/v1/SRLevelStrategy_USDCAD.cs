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
                CatReflexPeriodReflex = 17,
                CatReflexPeriodSuperSmoother = 159.2,
                CatReflexPeriodPostSmooth = 56,
                CatReflexConfirmationPeriod = 15,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 2.3,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}