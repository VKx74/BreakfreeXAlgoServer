using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    public class SRLevelStrategy_CADCHF_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_CADCHF_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.HOURLY_GRANULARITY,
                CatReflexPeriodReflex = 87,
                CatReflexPeriodSuperSmoother = 96,
                CatReflexPeriodPostSmooth = 212,
                CatReflexConfirmationPeriod = 3,
                CatReflexMinLevel = 0.01,
                CatReflexMaxLevel = 2.0,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}