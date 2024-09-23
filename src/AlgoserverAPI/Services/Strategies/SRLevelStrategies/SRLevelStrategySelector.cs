using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.Strategies.LevelStrategy;
using Algoserver.Strategies.SRLevelStrategy.V3;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public static class SRLevelStrategySelector
    {
        public static SRLevelStrategyBase SelectStrategy(StrategyInputContext context)
        {   if (context.symbol == "CAD_CHF")
            {
                return new SRLevelStrategy_CADCHF_v3(context);
            } 
            if (context.symbol == "USD_CAD")
            {
                return new SRLevelStrategy_USDCAD_v3(context);
            } 
            if (context.symbol == "GBP_CAD")
            {
                return new SRLevelStrategy_GBPCAD_v3(context);
            } 
            if (context.symbol == "XAU_USD")
            {
                return new SRLevelStrategy_XAUUSD_v3(context);
            }
            if (context.symbol == "CAD_JPY")
            {
                return new SRLevelStrategy_CADJPY_v3(context);
            } 
            if (context.symbol == "EUR_CAD")
            {
                return new SRLevelStrategy_EURCAD_v3(context);
            } 
            if (context.symbol == "EUR_USD")
            {
                return new SRLevelStrategy_EURUSD_v3(context);
            } 
            if (context.symbol == "BTC_USD" || context.symbol == "BTCUSD" || context.symbol == "BTC_USDT" || context.symbol == "BTCUSDT")
            {
                return new SRLevelStrategy_BTCUSD_v3(context);
            } 
            if (context.symbol == "SPX500_USD" || context.symbol == "SPX500" || context.symbol == "US500")
            {
                return new SRLevelStrategy_SPX500_v3(context);
            } 
            return null;
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