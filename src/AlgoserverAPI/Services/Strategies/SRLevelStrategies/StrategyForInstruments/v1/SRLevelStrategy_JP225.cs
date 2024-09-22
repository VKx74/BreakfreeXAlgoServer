using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_JP225 : SRLevelStrategyBase
    {
        public SRLevelStrategy_JP225(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 109,
                CatReflexPeriodSuperSmoother = 149.6,
                CatReflexPeriodPostSmooth = 78,
                CatReflexConfirmationPeriod = 13,
                CatReflexMinLevel = 0.08,
                CatReflexMaxLevel = 2.1,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}