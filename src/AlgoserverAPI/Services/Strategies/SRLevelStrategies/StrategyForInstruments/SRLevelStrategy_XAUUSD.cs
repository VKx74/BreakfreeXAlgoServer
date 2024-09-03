using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_XAUUSD : SRLevelStrategyBase
    {
        public SRLevelStrategy_XAUUSD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 86,
                CatReflexPeriodSuperSmoother = 166.6,
                CatReflexPeriodPostSmooth = 29,
                CatReflexConfirmationPeriod = 11,
                CatReflexMinLevel = 0.03,
                CatReflexMaxLevel = 2.3,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}