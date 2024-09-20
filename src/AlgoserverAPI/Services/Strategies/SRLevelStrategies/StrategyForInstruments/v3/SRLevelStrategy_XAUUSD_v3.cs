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
                CatReflexGranularity = TimeframeHelper.HOUR4_GRANULARITY,
                CatReflexPeriodReflex = 83,
                CatReflexPeriodSuperSmoother = 104,
                CatReflexPeriodPostSmooth = 116,
                CatReflexConfirmationPeriod = 17,
                CatReflexMinLevel = 0.09,
                CatReflexMaxLevel = 2.3,
                CatReflexValidateZeroCrossover = false,
                
                UseCatReflex2 = true,
                CatReflexGranularity2 = TimeframeHelper.MIN1_GRANULARITY,
                CatReflexPeriodReflex2 = 14,
                CatReflexPeriodSuperSmoother2 = 27,
                CatReflexPeriodPostSmooth2 = 441,
                CatReflexConfirmationPeriod2 = 7,
                CatReflexMinLevel2 = 0.05,
                CatReflexMaxLevel2 = 3.4,
                CatReflexValidateZeroCrossover2 = true
            };

            var result = await CalculateInternal(settings);
            result.Skip1MinTrades = true;
            return result;
        }
    }
}