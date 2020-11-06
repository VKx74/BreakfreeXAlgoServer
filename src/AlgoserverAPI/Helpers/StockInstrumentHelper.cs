using System.Collections.Generic;

namespace Algoserver.API.Helpers
{
    public class StockInstrument
    {
        public string Symbol { get; set; }
        public string Exchange { get; set; }
    }
    public static class StockInstrumentHelper
    {
        public static List<StockInstrument> StockInstrumentList
        {
            get
            {
                return new List<StockInstrument> {
                    new StockInstrument { Symbol = "AAPL", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "AMD", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "AMZN", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "ATVI", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "BA", Exchange = "NYSE" }, new StockInstrument { Symbol = "BAC-A", Exchange = "NYSE" }, new StockInstrument { Symbol = "BIDU", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "C-J", Exchange = "NYSE" }, new StockInstrument { Symbol = "FB", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "FCX", Exchange = "NYSE" }, new StockInstrument { Symbol = "FSLR", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "GM", Exchange = "NYSE" }, new StockInstrument { Symbol = "GOOG", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "GOOGL", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "GS", Exchange = "NYSE" }, new StockInstrument { Symbol = "GWPH", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "JPM", Exchange = "NYSE" }, new StockInstrument { Symbol = "MCD", Exchange = "NYSE" }, new StockInstrument { Symbol = "MSFT", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "NFLX", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "NKE", Exchange = "NYSE" }, new StockInstrument { Symbol = "NVDA", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "PFE", Exchange = "NYSE" }, new StockInstrument { Symbol = "PGHH", Exchange = "NSE" }, new StockInstrument { Symbol = "PTON", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "QCOM", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "RACE", Exchange = "NYSE" }, new StockInstrument { Symbol = "SNAP", Exchange = "NYSE" }, new StockInstrument { Symbol = "TLRY", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "BYND", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "TSLA", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "TWTR", Exchange = "NYSE" }, new StockInstrument { Symbol = "UBER", Exchange = "NYSE" }, new StockInstrument { Symbol = "WMT", Exchange = "NYSE" }, new StockInstrument { Symbol = "XOM", Exchange = "NYSE" }, new StockInstrument { Symbol = "ZM", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "GE", Exchange = "NYSE" }, new StockInstrument { Symbol = "AAL", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "DELTA", Exchange = "BSE" }, new StockInstrument { Symbol = "GPRO", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "ACB", Exchange = "TSX" }, new StockInstrument { Symbol = "CUK", Exchange = "NYSE" }, new StockInstrument { Symbol = "CCL", Exchange = "BSE" }, new StockInstrument { Symbol = "NCLH", Exchange = "NYSE" }, new StockInstrument { Symbol = "FIT", Exchange = "NYSE" }, new StockInstrument { Symbol = "PLUG", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "UAL", Exchange = "NASDAQ" }, new StockInstrument { Symbol = "WEED", Exchange = "TSX" }, new StockInstrument { Symbol = "CSCO", Exchange = "NASDAQ" }
                };
            }
        }

        public static List<string> ForexInstrumentList
        {
            get
            {
                return new List<string> {
                    "AU200_AUD",
                    "AUD_CAD",
                    "AUD_CHF",
                    "AUD_JPY",
                    "AUD_NZD",
                    "AUD_SGD",
                    "AUD_USD",
                    "BCO_USD",
                    "CAD_CHF",
                    "CAD_JPY",
                    "CAD_SGD",
                    "CHF_JPY",
                    "CN50_USD",
                    "CORN_USD",
                    "DE10YB_EUR",
                    "DE30_EUR",
                    "EU50_EUR",
                    "EUR_AUD",
                    "EUR_CAD",
                    "EUR_CHF",
                    "EUR_GBP",
                    "EUR_JPY",
                    "EUR_NOK",
                    "EUR_NZD",
                    "EUR_SEK",
                    "EUR_SGD",
                    "EUR_USD",
                    "FR40_EUR",
                    "GBP_AUD",
                    "GBP_CAD",
                    "GBP_CHF",
                    "GBP_JPY",
                    "GBP_SGD",
                    "GBP_USD",
                    "HK33_HKD",
                    "IN50_USD",
                    "JP225_USD",
                    "NAS100_USD",
                    "NATGAS_USD",
                    "NL25_EUR",
                    "NZD_CAD",
                    "NZD_CHF",
                    "NZD_JPY",
                    "NZD_SGD",
                    "NZD_USD",
                    "SG30_SGD",
                    "SGD_CHF",
                    "SGD_JPY",
                    "SOYBN_USD",
                    "SPX500_USD",
                    "SUGAR_USD",
                    "UK100_GBP",
                    "UK10YB_GBP",
                    "US2000_USD",
                    "US30_USD",
                    "USB02Y_USD",
                    "USB05Y_USD",
                    "USB10Y_USD",
                    "USB30Y_USD",
                    "USD_CAD",
                    "USD_CHF",
                    "USD_HUF",
                    "USD_JPY",
                    "USD_MXN",
                    "USD_NOK",
                    "USD_SEK",
                    "USD_SGD",
                    "USD_ZAR",
                    "WHEAT_USD",
                    "WTICO_USD",
                    "XAG_EUR",
                    "XAG_USD",
                    "XAU_EUR",
                    "XAU_USD",
                    "XAU_XAG",
                    "XCU_USD"
                };
            }
        }
    }
}
