using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    public class SRLevelStrategy_EURUSD_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_EURUSD_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 22,
                CatReflexPeriodSuperSmoother = 96,
                CatReflexPeriodPostSmooth = 88,
                CatReflexConfirmationPeriod = 3,
                CatReflexMinLevel = 0.01,
                CatReflexMaxLevel = 1.8,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}