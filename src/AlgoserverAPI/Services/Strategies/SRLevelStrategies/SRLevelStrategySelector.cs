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
            if (context.symbol == "AUD_NZD")
            {
                return new SRLevelStrategy_AUDNZD(context);
            } 
            if (context.symbol == "AUD_CAD")
            {
                return new SRLevelStrategy_AUDCAD(context);
            } 
            if (context.symbol == "GBP_USD")
            {
                return new SRLevelStrategy_GBPUSD(context);
            } 
            if (context.symbol == "USD_CHF")
            {
                return new SRLevelStrategy_USDCHF(context);
            } 
            if (context.symbol == "BTC_USD" || context.symbol == "BTCUSD" || context.symbol == "BTC_USDT" || context.symbol == "BTCUSDT")
            {
                return new SRLevelStrategy_BTCUSD(context);
            } 
            if (context.symbol == "SPX500_USD" || context.symbol == "SPX500" || context.symbol == "US500")
            {
                return new SRLevelStrategy_SPX500(context);
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
            if (context.symbol.ToUpper().EndsWith("NZD"))
            {
                return new SRLevelStrategy_AUDNZD(context);
            } 
            if (context.symbol.ToUpper().EndsWith("CAD"))
            {
                return new SRLevelStrategy_AUDCAD(context);
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