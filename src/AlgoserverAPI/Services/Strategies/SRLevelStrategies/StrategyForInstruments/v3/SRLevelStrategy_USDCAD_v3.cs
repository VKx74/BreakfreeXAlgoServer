using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    public class SRLevelStrategy_USDCAD_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_USDCAD_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.HOURLY_GRANULARITY,
                CatReflexPeriodReflex = 188,
                CatReflexPeriodSuperSmoother = 192,
                CatReflexPeriodPostSmooth = 75,
                CatReflexConfirmationPeriod = 7,
                CatReflexMinLevel = 0.02,
                CatReflexMaxLevel = 1.9,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}