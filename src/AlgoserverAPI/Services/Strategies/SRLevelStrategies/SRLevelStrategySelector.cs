using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.Strategies.LevelStrategy;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public static class SRLevelStrategySelector
    {
        public static SRLevelStrategyBase SelectStrategy(StrategyInputContext context)
        {   
            if (context.symbol == "XAU_USD")
            {
                return new SRLevelStrategy_XAUUSD(context);
            }
            if (context.symbol == "EUR_USD")
            {
                return new SRLevelStrategy_EURUSD(context);
            } 
            if (context.symbol == "USD_JPY")
            {
                return new SRLevelStrategy_USDJPY(context);
            } 

            var type = InstrumentsHelper.GetInstrumentType(context.symbol);

            if (type == InstrumentTypes.Metals)
            {
                return new SRLevelStrategy_XAUUSD(context);
            }

            if (type != InstrumentTypes.MajorForex && type != InstrumentTypes.ForexMinors && type != InstrumentTypes.ForexExotics)
            {
                return null;
            }

            if (context.symbol.ToUpper().EndsWith("JPY"))
            {
                return new SRLevelStrategy_USDJPY(context);
            }
            if (context.symbol.ToUpper().EndsWith("USD"))
            {
                return new SRLevelStrategy_EURUSD(context);
            } 

            return new SRLevelStrategy_GeneralReflexOscillator(context);
        }

        public static async Task<SRLevelStrategyResponse> Calculate(StrategyInputContext context)
        {
            var strategy = SelectStrategy(context);
            if (strategy == null)
            {
                return null;
            }

            var res = await strategy.Calculate();
            return res;
        }
    }
}