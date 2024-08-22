using System.Threading.Tasks;
using Algoserver.API.Helpers;

namespace Algoserver.Strategies.NLevelStrategy
{
    public static class NLevelStrategySelector
    {
        public static NLevelStrategyBase SelectStrategy(NLevelStrategyInputContext context)
        {
            if (context.symbol == "XAU_USD")
            {
                return new NLevelStrategy_XAUUSD(context);
            }
            if (context.symbol == "AUD_CAD")
            {
                return new NLevelStrategy_AUDCAD(context);
            }
            if (context.symbol == "EUR_CAD")
            {
                return new NLevelStrategy_EURCAD(context);
            }
            if (context.symbol == "USD_JPY")
            {
                return new NLevelStrategy_USDJPY(context);
            }
            if (context.symbol == "EUR_USD")
            {
                return new NLevelStrategy_EURUSD(context);
            } 
            if (context.symbol == "USD_CHF")
            {
                return new NLevelStrategy_USDCHF(context);
            } 

            
            var type = InstrumentsHelper.GetInstrumentType(context.symbol);

            if (type == InstrumentTypes.Metals)
            {
                return new NLevelStrategy_XAUUSD(context);
            }

            if (type != InstrumentTypes.MajorForex && type != InstrumentTypes.ForexMinors && type != InstrumentTypes.ForexExotics)
            {
                return null;
            }

            if (context.symbol.ToUpper().EndsWith("CAD"))
            {
                return new NLevelStrategy_EURCAD(context);
            }
            if (context.symbol.ToUpper().EndsWith("JPY"))
            {
                return new NLevelStrategy_USDJPY(context);
            }
            if (context.symbol.ToUpper().EndsWith("USD"))
            {
                return new NLevelStrategy_EURUSD(context);
            } 
            if (context.symbol.ToUpper().EndsWith("CHF"))
            {
                return new NLevelStrategy_USDCHF(context);
            } 
            return null;
        }

        public static async Task<NLevelStrategyResponse> Calculate(NLevelStrategyInputContext context)
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