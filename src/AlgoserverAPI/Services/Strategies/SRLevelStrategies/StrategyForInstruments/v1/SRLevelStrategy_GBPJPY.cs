using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_GBPJPY : SRLevelStrategyBase
    {
        public SRLevelStrategy_GBPJPY(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 29,
                CatReflexPeriodSuperSmoother = 152.8,
                CatReflexPeriodPostSmooth = 158,
                CatReflexConfirmationPeriod = 1,
                CatReflexMinLevel = 0.06,
                CatReflexMaxLevel = 2,
                CatReflexValidateZeroCrossover = false
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}