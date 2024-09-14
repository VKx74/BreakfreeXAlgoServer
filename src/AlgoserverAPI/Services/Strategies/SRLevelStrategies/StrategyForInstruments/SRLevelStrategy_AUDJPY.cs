using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_AUDJPY : SRLevelStrategyBase
    {
        public SRLevelStrategy_AUDJPY(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 10,
                CatReflexPeriodSuperSmoother = 172.8,
                CatReflexPeriodPostSmooth = 45,
                CatReflexConfirmationPeriod = 9,
                CatReflexMinLevel = 0.09,
                CatReflexMaxLevel = 1.8,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}