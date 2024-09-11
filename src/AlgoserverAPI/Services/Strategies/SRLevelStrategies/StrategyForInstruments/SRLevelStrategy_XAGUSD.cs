using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_XAGUSD : SRLevelStrategyBase
    {
        public SRLevelStrategy_XAGUSD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 49,
                CatReflexPeriodSuperSmoother = 27.2,
                CatReflexPeriodPostSmooth = 73,
                CatReflexConfirmationPeriod = 16,
                CatReflexMinLevel = 0.09,
                CatReflexMaxLevel = 1.9,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}