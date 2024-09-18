using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy.V3
{
    public class SRLevelStrategy_XAUUSD_v3 : SRLevelStrategyBase
    {
        public SRLevelStrategy_XAUUSD_v3(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 93,
                CatReflexPeriodSuperSmoother = 17.6,
                CatReflexPeriodPostSmooth = 141,
                CatReflexConfirmationPeriod = 4,
                CatReflexMinLevel = 0.02,
                CatReflexMaxLevel = 1.8,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            result.Skip1MinTrades = true;
            return result;
        }
    }
}