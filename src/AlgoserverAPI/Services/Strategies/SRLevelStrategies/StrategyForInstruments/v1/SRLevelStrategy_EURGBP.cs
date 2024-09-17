using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_EURGBP : SRLevelStrategyBase
    {
        public SRLevelStrategy_EURGBP(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 85,
                CatReflexPeriodSuperSmoother = 152.8,
                CatReflexPeriodPostSmooth = 40,
                CatReflexConfirmationPeriod = 12,
                CatReflexMinLevel = 0.02,
                CatReflexMaxLevel = 1.7,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}