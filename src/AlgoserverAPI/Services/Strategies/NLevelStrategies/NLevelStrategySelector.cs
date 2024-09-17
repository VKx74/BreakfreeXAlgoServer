using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.Strategies.LevelStrategy;
using Algoserver.Strategies.NLevelStrategy.V2;
using Algoserver.Strategies.NLevelStrategy.V3;

namespace Algoserver.Strategies.NLevelStrategy
{
    public static class NLevelStrategySelector
    {
        public static NLevelStrategyBase SelectStrategy(StrategyInputContext context)
        {   
            if (context.symbol == "CAD_CHF")
            {
                return new NLevelStrategy_CADCHF_v3(context);
            } 
            if (context.symbol == "EUR_CHF")
            {
                return new NLevelStrategy_EURCHF_v3(context);
            } 
            if (context.symbol == "EUR_USD")
            {
                return new NLevelStrategy_EURUSD_v3(context);
            } 
            if (context.symbol == "USD_CAD")
            {
                return new NLevelStrategy_USDCAD_v3(context);
            } 

            return null;

            if (context.symbol == "XAU_USD")
            {
                return new NLevelStrategy_XAUUSD_v2(context);
            }
            if (context.symbol == "XAG_USD")
            {
                return new NLevelStrategy_XAGUSD_v2(context);
            }
            if (context.symbol == "EUR_USD")
            {
                return new NLevelStrategy_EURUSD_v2(context);
            } 
            if (context.symbol == "USD_JPY")
            {
                return new NLevelStrategy_USDJPY_v2(context);
            } 
            if (context.symbol == "EUR_JPY")
            {
                return new NLevelStrategy_EURJPY_v2(context);
            } 
            if (context.symbol == "AUD_NZD")
            {
                return new NLevelStrategy_AUDNZD_v2(context);
            } 
            if (context.symbol == "EUR_NZD")
            {
                return new NLevelStrategy_EURNZD_v2(context);
            } 
            if (context.symbol == "AUD_CAD")
            {
                return new NLevelStrategy_AUDCAD_v2(context);
            } 
            if (context.symbol == "GBP_USD")
            {
                return new NLevelStrategy_GBPUSD_v2(context);
            } 
            if (context.symbol == "GBP_AUD")
            {
                return new NLevelStrategy_GBPAUD_v2(context);
            } 
            if (context.symbol == "USD_CHF")
            {
                return new NLevelStrategy_USDCHF_v2(context);
            } 
            if (context.symbol == "EUR_CHF")
            {
                return new NLevelStrategy_EURCHF_v2(context);
            } 
            if (context.symbol == "EUR_GBP")
            {
                return new NLevelStrategy_EURGBP_v2(context);
            }
            if (context.symbol == "AUD_USD")
            {
                return new NLevelStrategy_AUDUSD_v2(context);
            } 
            if (context.symbol == "EUR_CAD")
            {
                return new NLevelStrategy_EURCAD_v2(context);
            } 
            if (context.symbol == "USD_CAD")
            {
                return new NLevelStrategy_USDCAD_v2(context);
            } 
            if (context.symbol == "GBP_JPY")
            {
                return new NLevelStrategy_GBPJPY_v2(context);
            } 
            if (context.symbol == "NZD_CAD")
            {
                return new NLevelStrategy_NZDCAD_v2(context);
            } 
            if (context.symbol == "USD_ZAR")
            {
                return new NLevelStrategy_USDZAR_v2(context);
            } 
            if (context.symbol == "EUR_AUD")
            {
                return new NLevelStrategy_EURAUD_v2(context);
            } 
            if (context.symbol == "GBP_CAD")
            {
                return new NLevelStrategy_GBPCAD_v2(context);
            } 
            if (context.symbol == "GBP_CHF")
            {
                return new NLevelStrategy_GBPCHF_v2(context);
            } 
            if (context.symbol == "NZD_USD")
            {
                return new NLevelStrategy_NZDUSD_v2(context);
            } 
            if (context.symbol == "AUD_CHF")
            {
                return new NLevelStrategy_AUDCHF_v2(context);
            } 
            if (context.symbol == "AUD_JPY")
            {
                return new NLevelStrategy_AUDJPY_v2(context);
            } 
            if (context.symbol == "CAD_JPY")
            {
                return new NLevelStrategy_CADJPY_v2(context);
            } 
            if (context.symbol == "NZD_CHF")
            {
                return new NLevelStrategy_NZDCHF_v2(context);
            } 
            if (context.symbol == "NZD_JPY")
            {
                return new NLevelStrategy_NZDJPY_v2(context);
            } 

            if (context.symbol == "BTC_USD" || context.symbol == "BTCUSD" || context.symbol == "BTC_USDT" || context.symbol == "BTCUSDT")
            {
                return new NLevelStrategy_BTCUSD_v2(context);
            } 
            if (context.symbol == "SPX500_USD" || context.symbol == "SPX500" || context.symbol == "US500")
            {
                return new NLevelStrategy_SPX500_v2(context);
            } 
            if (context.symbol == "NAS100_USD" || context.symbol == "NAS100" || context.symbol == "US100")
            {
                return new NLevelStrategy_NAS100_v2(context);
            } 
            if (context.symbol == "DE30_EUR" || context.symbol == "DE30" || context.symbol == "DE40_EUR" || context.symbol == "DE40")
            {
                return new NLevelStrategy_DE30_v2(context);
            } 
            if (context.symbol == "JP225_USD" || context.symbol == "JP225")
            {
                return new NLevelStrategy_JP225_v2(context);
            } 
            
            return null;

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
            if (context.symbol.ToUpper().EndsWith("CAD"))
            {
                return new NLevelStrategy_AUDCAD_v2(context);
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