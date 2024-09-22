using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    public class SRLevelStrategy_EURCHF_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_EURCHF_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.HOURLY_GRANULARITY,
                CatReflexPeriodReflex = 235,
                CatReflexPeriodSuperSmoother = 197.6,
                CatReflexPeriodPostSmooth = 149,
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