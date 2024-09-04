using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_AUDCAD : SRLevelStrategyBase
    {
        public SRLevelStrategy_AUDCAD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 47,
                CatReflexPeriodSuperSmoother = 101.6,
                CatReflexPeriodPostSmooth = 110,
                CatReflexConfirmationPeriod = 3,
                CatReflexMinLevel = 0.03,
                CatReflexMaxLevel = 2.3,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}