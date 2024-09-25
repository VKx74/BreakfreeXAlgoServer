using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.Strategies.LevelStrategy;
using Algoserver.Strategies.SRLevelStrategy.V3;

namespace Algoserver.Strategies.SRLevelStrategy
{
    public static class SRLevelStrategySelector
    {
        public static SRLevelStrategyBase SelectStrategy(StrategyInputContext context)
        {   
            if (context.symbol == "EUR_USD")
            {
                return new SRLevelStrategy_EURUSD(context);
            } 
            if (context.symbol == "CAD_JPY")
            {
                return new SRLevelStrategy_CADJPY(context);
            } 
            if (context.symbol == "EUR_CAD")
            {
                return new SRLevelStrategy_EURCAD(context);
            } 
            if (context.symbol == "NZD_CAD")
            {
                return new SRLevelStrategy_NZDCAD(context);
            } 
            if (context.symbol == "AUD_CAD")
            {
                return new SRLevelStrategy_AUDCAD(context);
            } 
            if (context.symbol == "GBP_CAD")
            {
                return new SRLevelStrategy_GBPCAD(context);
            } 
            if (context.symbol == "USD_CAD")
            {
                return new SRLevelStrategy_USDCAD(context);
            } 
            if (context.symbol == "USD_JPY")
            {
                return new SRLevelStrategy_USDJPY(context);
            } 
            if (context.symbol == "GBP_AUD")
            {
                return new SRLevelStrategy_GBPAUD(context);
            } 
            if (context.symbol == "AUD_CHF")
            {
                return new SRLevelStrategy_AUDCHF(context);
            } 
            if (context.symbol == "CAD_CHF")
            {
                return new SRLevelStrategy_CADCHF(context);
            } 
            if (context.symbol == "XAU_USD")
            {
                return new SRLevelStrategy_XAUUSD(context);
            }
            if (context.symbol == "BTC_USD" || context.symbol == "BTCUSD" || context.symbol == "BTC_USDT" || context.symbol == "BTCUSDT")
            {
                return new SRLevelStrategy_BTCUSD(context);
            } 
            if (context.symbol == "SPX500_USD" || context.symbol == "SPX500" || context.symbol == "US500")
            {
                return new SRLevelStrategy_SPX500(context);
            } 

            // All
            if (context.symbol == "AUD_CAD")
            {
                return new SRLevelStrategy_AUDCAD(context);
            } 
            if (context.symbol == "AUD_CHF")
            {
                return new SRLevelStrategy_AUDCHF(context);
            } 
            if (context.symbol == "AUD_JPY")
            {
                return new SRLevelStrategy_AUDJPY(context);
            } 
            if (context.symbol == "AUD_NZD")
            {
                return new SRLevelStrategy_AUDNZD(context);
            } 
            if (context.symbol == "AUD_USD")
            {
                return new SRLevelStrategy_AUDUSD(context);
            } 
            if (context.symbol == "CAD_CHF")
            {
                return new SRLevelStrategy_CADCHF(context);
            } 
            if (context.symbol == "CAD_JPY")
            {
                return new SRLevelStrategy_CADJPY(context);
            } 
            if (context.symbol == "EUR_AUD")
            {
                return new SRLevelStrategy_EURAUD(context);
            } 
            if (context.symbol == "EUR_CAD")
            {
                return new SRLevelStrategy_EURCAD(context);
            } 
            if (context.symbol == "EUR_CHF")
            {
                return new SRLevelStrategy_EURCHF(context);
            } 
            if (context.symbol == "EUR_GBP")
            {
                return new SRLevelStrategy_EURGBP(context);
            } 
            if (context.symbol == "EUR_JPY")
            {
                return new SRLevelStrategy_EURJPY(context);
            } 
            if (context.symbol == "EUR_NZD")
            {
                return new SRLevelStrategy_EURNZD(context);
            } 
            if (context.symbol == "EUR_USD")
            {
                return new SRLevelStrategy_EURUSD(context);
            } 
            if (context.symbol == "GBP_AUD")
            {
                return new SRLevelStrategy_GBPAUD(context);
            } 
            if (context.symbol == "GBP_CAD")
            {
                return new SRLevelStrategy_GBPCAD(context);
            } 
            if (context.symbol == "GBP_CHF")
            {
                return new SRLevelStrategy_GBPCHF(context);
            } 
            if (context.symbol == "GBP_JPY")
            {
                return new SRLevelStrategy_GBPJPY(context);
            } 
            if (context.symbol == "GBP_USD")
            {
                return new SRLevelStrategy_GBPUSD(context);
            } 
            if (context.symbol == "NZD_CAD")
            {
                return new SRLevelStrategy_NZDCAD(context);
            } 
            if (context.symbol == "NZD_CHF")
            {
                return new SRLevelStrategy_NZDCHF(context);
            } 
            if (context.symbol == "NZD_JPY")
            {
                return new SRLevelStrategy_NZDJPY(context);
            } 
            if (context.symbol == "NZD_USD")
            {
                return new SRLevelStrategy_NZDUSD(context);
            } 
            if (context.symbol == "USD_CAD")
            {
                return new SRLevelStrategy_USDCAD(context);
            } 
            if (context.symbol == "USD_CHF")
            {
                return new SRLevelStrategy_USDCHF(context);
            } 
            if (context.symbol == "USD_JPY")
            {
                return new SRLevelStrategy_USDJPY(context);
            } 
            if (context.symbol == "USD_ZAR")
            {
                return new SRLevelStrategy_USDZAR(context);
            } 
            if (context.symbol == "XAG_USD")
            {
                return new SRLevelStrategy_XAGUSD(context);
            }
            if (context.symbol == "XAU_USD")
            {
                return new SRLevelStrategy_XAUUSD(context);
            }
            if (context.symbol == "BTC_USD" || context.symbol == "BTCUSD" || context.symbol == "BTC_USDT" || context.symbol == "BTCUSDT")
            {
                return new SRLevelStrategy_BTCUSD(context);
            } 
            if (context.symbol == "SPX500_USD" || context.symbol == "SPX500" || context.symbol == "US500")
            {
                return new SRLevelStrategy_SPX500(context);
            } 
            if (context.symbol == "DE30_EUR" || context.symbol == "DE30")
            {
                return new SRLevelStrategy_DE30(context);
            } 
            if (context.symbol == "NAS100_USD" || context.symbol == "NAS100" || context.symbol == "US100")
            {
                return new SRLevelStrategy_NAS100(context);
            } 
            if (context.symbol == "JP225_USD" || context.symbol == "JP225")
            {
                return new SRLevelStrategy_JP225(context);
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
            res.Logs = strategy.GetLogs();
            return res;
        }
    }
}