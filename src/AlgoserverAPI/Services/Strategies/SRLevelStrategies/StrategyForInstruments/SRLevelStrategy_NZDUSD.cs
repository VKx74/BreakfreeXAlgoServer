using System.Threading.Tasks;
using Algoserver.API.Services;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public class SRLevelStrategy_NZDUSD : SRLevelStrategyBase
    {
        public SRLevelStrategy_NZDUSD(StrategyInputContext _context) : base(_context)
        {
        }

        public override async Task<SRLevelStrategyResponse> Calculate()
        {
            var settings = new SRLevelStrategySettings
            {
                UseCatReflex = true,
                CatReflexGranularity = TimeframeHelper.DAILY_GRANULARITY,
                CatReflexPeriodReflex = 52,
                CatReflexPeriodSuperSmoother = 7.2,
                CatReflexPeriodPostSmooth = 122,
                CatReflexConfirmationPeriod = 17,
                CatReflexMinLevel = 0.08,
                CatReflexMaxLevel = 2.2,
                CatReflexValidateZeroCrossover = true
            };

            var result = await CalculateInternal(settings);
            return result;
        }
    }
}