using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_NZDCAD : SRLevelStrategyBase
    {
        public SRLevelStrategy_NZDCAD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 27,
                CatReflexPeriodSuperSmoother = 27.2,
                CatReflexPeriodPostSmooth = 35,
                CatReflexConfirmationPeriod = 13,
                CatReflexMinLevel = 0,
                CatReflexMaxLevel = 2.2,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}