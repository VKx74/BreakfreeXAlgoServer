using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_EURCAD : SRLevelStrategyBase
    {
        public SRLevelStrategy_EURCAD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 27,
                CatReflexPeriodSuperSmoother = 8.8,
                CatReflexPeriodPostSmooth = 164,
                CatReflexConfirmationPeriod = 9,
                CatReflexMinLevel = 0.04,
                CatReflexMaxLevel = 1.7,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}