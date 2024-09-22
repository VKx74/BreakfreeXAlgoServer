using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_USDJPY : SRLevelStrategyBase
    {
        public SRLevelStrategy_USDJPY(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.HOUR4_GRANULARITY,
                CatReflexPeriodReflex = 130,
                CatReflexPeriodSuperSmoother = 81.6,
                CatReflexPeriodPostSmooth = 38,
                CatReflexConfirmationPeriod = 13,
                CatReflexMinLevel = 0.1,
                CatReflexMaxLevel = 1.7,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}