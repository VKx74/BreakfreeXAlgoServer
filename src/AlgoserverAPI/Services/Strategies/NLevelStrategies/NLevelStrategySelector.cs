using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.Strategies.LevelStrategy;
using Algoserver.Strategies.NLevelStrategy.V2;

namespace Algoserver.Strategies.NLevelStrategy
{
    public static class NLevelStrategySelector
    {
        public static NLevelStrategyBase SelectStrategy(StrategyInputContext context)
        {   
            if (context.symbol == "XAU_USD")
            {
                return new NLevelStrategy_XAUUSD_v2(context);
            }
            if (context.symbol == "EUR_USD")
            {
                return new NLevelStrategy_EURUSD_v2(context);
            } 
            if (context.symbol == "USD_JPY")
            {
                return new NLevelStrategy_USDJPY_v2(context);
            } 
            if (context.symbol == "AUD_NZD")
            {
                return new NLevelStrategy_AUDNZD_v2(context);
            } 
            // if (context.symbol == "BTC_USD" || context.symbol == "BTCUSD" || context.symbol == "BTC_USDT" || context.symbol == "BTCUSDT")
            // {
            //     return new NLevelStrategy_BTCUSD(context);
            // } 

            
            var type = InstrumentsHelper.GetInstrumentType(context.symbol);

            if (type == InstrumentTypes.Metals)
            {
                return new NLevelStrategy_XAUUSD_v2(context);
            }

            if (type != InstrumentTypes.MajorForex && type != InstrumentTypes.ForexMinors && type != InstrumentTypes.ForexExotics)
            {
                return null;
            }

            if (context.symbol.ToUpper().EndsWith("JPY"))
            {
                return new NLevelStrategy_USDJPY_v2(context);
            }
            if (context.symbol.ToUpper().EndsWith("USD"))
            {
                return new NLevelStrategy_EURUSD_v2(context);
            } 
            if (context.symbol.ToUpper().EndsWith("NZD"))
            {
                return new NLevelStrategy_AUDNZD_v2(context);
            } 
            
            return new NLevelStrategy_GeneralReflexOscillator(context);
        }

        public static async Task<NLevelStrategyResponse> Calculate(StrategyInputContext context)
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