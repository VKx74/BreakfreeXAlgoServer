using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    public class SRLevelStrategy_US30_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_US30_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.HOURLY_GRANULARITY,
                CatReflexPeriodReflex = 243,
                CatReflexPeriodSuperSmoother = 213,
                CatReflexPeriodPostSmooth = 186,
                CatReflexConfirmationPeriod = 11,
                CatReflexMinLevel = 0.03,
                CatReflexMaxLevel = 2.0,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}