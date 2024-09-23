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
            if (context.symbol == "EUR_USD")
            {
                return new NLevelStrategy_EURUSD_v2(context);
            } 
            if (context.symbol == "CAD_JPY")
            {
                return new NLevelStrategy_CADJPY_v2(context);
            } 
            if (context.symbol == "EUR_CAD")
            {
                return new NLevelStrategy_EURCAD_v2(context);
            } 
            if (context.symbol == "NZD_CAD")
            {
                return new NLevelStrategy_NZDCAD_v2(context);
            } 
            if (context.symbol == "AUD_CAD")
            {
                return new NLevelStrategy_AUDCAD_v2(context);
            } 
            if (context.symbol == "GBP_CAD")
            {
                return new NLevelStrategy_GBPCAD_v2(context);
            } 
            if (context.symbol == "USD_CAD")
            {
                return new NLevelStrategy_USDCAD_v2(context);
            } 
            if (context.symbol == "USD_JPY")
            {
                return new NLevelStrategy_USDJPY_v2(context);
            } 
            if (context.symbol == "GBP_AUD")
            {
                return new NLevelStrategy_GBPAUD_v2(context);
            } 
            if (context.symbol == "AUD_CHF")
            {
                return new NLevelStrategy_AUDCHF_v2(context);
            } 
            if (context.symbol == "CAD_CHF")
            {
                return new NLevelStrategy_CADCHF_v2(context);
            } 
            if (context.symbol == "XAU_USD")
            {
                return new NLevelStrategy_XAUUSD_v2(context);
            }
            if (context.symbol == "BTC_USD" || context.symbol == "BTCUSD" || context.symbol == "BTC_USDT" || context.symbol == "BTCUSDT")
            {
                return new NLevelStrategy_BTCUSD_v2(context);
            } 
            if (context.symbol == "SPX500_USD" || context.symbol == "SPX500" || context.symbol == "US500")
            {
                return new NLevelStrategy_SPX500_v2(context);
            } 
            
            return null;
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