using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    public class SRLevelStrategy_BTCUSD_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_BTCUSD_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 101,
                CatReflexPeriodSuperSmoother = 33.6,
                CatReflexPeriodPostSmooth = 195,
                CatReflexConfirmationPeriod = 13,
                CatReflexMinLevel = 0.02,
                CatReflexMaxLevel = 2.1,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}