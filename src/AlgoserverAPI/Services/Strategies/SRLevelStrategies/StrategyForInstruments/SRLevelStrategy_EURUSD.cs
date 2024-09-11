using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_EURUSD : SRLevelStrategyBase
    {
        public SRLevelStrategy_EURUSD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 23,
                CatReflexPeriodSuperSmoother = 164,
                CatReflexPeriodPostSmooth = 151,
                CatReflexConfirmationPeriod = 10,
                CatReflexMinLevel = 0.04,
                CatReflexMaxLevel = 2.2,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}