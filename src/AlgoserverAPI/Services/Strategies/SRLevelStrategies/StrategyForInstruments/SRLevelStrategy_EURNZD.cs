using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_EURNZD : SRLevelStrategyBase
    {
        public SRLevelStrategy_EURNZD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 13,
                CatReflexPeriodSuperSmoother = 178.4,
                CatReflexPeriodPostSmooth = 110,
                CatReflexConfirmationPeriod = 1,
                CatReflexMinLevel = 0.05,
                CatReflexMaxLevel = 2.0,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}