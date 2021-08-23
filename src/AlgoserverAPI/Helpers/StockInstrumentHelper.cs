using System.Collections.Generic;

namespace Algoserver.API.Helpers
{
    public class InstrumentDescription
    {
        public string Symbol { get; set; }
        public string Exchange { get; set; }
    }
    public static class InstrumentsHelper
    {
        public static decimal ForexContractSize = 100000;

        public static decimal MetalsContractSize = 100;

        public static decimal IndicesContractSize = 10;

        public static decimal BoundsContractSize = 10;

        public static decimal CommoditiesContractSize = 10000;

        public static List<InstrumentDescription> StockInstrumentList
        {
            get
            {
                return new List<InstrumentDescription> {
                    new InstrumentDescription { Symbol = "AAPL", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "AMD", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "AMZN", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "ATVI", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "BA", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "BAC-A", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "BIDU", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "C-J", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "FB", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "FCX", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "FSLR", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "GM", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "GOOG", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "GOOGL", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "GS", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "GWPH", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "JPM", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "MCD", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "MSFT", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "NFLX", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "NKE", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "NVDA", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "PFE", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "PGHH", Exchange = "NSE" }, new InstrumentDescription { Symbol = "PTON", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "QCOM", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "RACE", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "SNAP", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "TLRY", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "BYND", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "TSLA", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "TWTR", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "UBER", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "WMT", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "XOM", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "ZM", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "GE", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "AAL", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "DELTA", Exchange = "BSE" }, new InstrumentDescription { Symbol = "GPRO", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "ACB", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "CUK", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "CCL", Exchange = "BSE" }, new InstrumentDescription { Symbol = "NCLH", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "FIT", Exchange = "NYSE" }, new InstrumentDescription { Symbol = "PLUG", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "UAL", Exchange = "NASDAQ" }, new InstrumentDescription { Symbol = "WEED", Exchange = "TSX" }, new InstrumentDescription { Symbol = "CSCO", Exchange = "NASDAQ" }
                };
            }
        }
        public static List<InstrumentDescription> CryptoInstrumentList
        {
            get
            {
                return new List<InstrumentDescription> {
                    new InstrumentDescription { Symbol = "ethbtc", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "xrpbtc", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "xlmbtc", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "ltcbtc", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "linkbtc", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "yfibtc", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "adabtc", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "xembtc", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "xmrbtc", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "bchbtc", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "eosbtc", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "trxbtc", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "ethusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "btcusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "bchusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "xrpusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "ltcusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "linkusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "adausdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "yfiusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "xmrusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "trxusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "eosusdt", Exchange = "Binance" }
                };
            }
        }

        public static List<string> ForexMetals {
            get {
                return new List<string> {
                    "XAU", "XAG", "XPD", "XCU"
                };
            }
        }

        public static List<string> ForexIndices {
            get {
                return new List<string> {
                    "AU200_AUD", "CN50_USD", "EU50_EUR", "FR40_EUR", "DE30_EUR", "HK33_HKD", "IN50_USD", "JP225_USD", "NL25_EUR",
                    "SG30_SGD", "TWIX_USD", "UK100_GBP", "NAS100_USD", "US2000_USD", "SPX500_USD", "US30_USD"
                };
            }
        }   
        
        public static List<string> ForexBounds {
            get {
                return new List<string> {
                    "DE10YB_EUR", "UK10YB_GBP", "UK100_GBP", "USB02Y_USD", "USB05Y_USD", "USB30Y_USD"
                };
            }
        }

        public static List<string> ForexCommodities {
            get {
                return new List<string> {
                    "BCO_USD", "CORN_USD", "NATGAS_USD", "SOYBN_USD", "SUGAR_USD", "WHEAT_USD", "WTICO_USD", "XCU_USD"
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

        public static decimal GetContractSize(string symbol) {
            foreach (var i in InstrumentsHelper.ForexIndices) {
                if (symbol.Contains(i)) {
                    return InstrumentsHelper.IndicesContractSize;
                }
            }

            foreach (var i in InstrumentsHelper.ForexBounds) {
                if (symbol.Contains(i)) {
                    return InstrumentsHelper.BoundsContractSize;
                }
            }

            foreach (var i in InstrumentsHelper.ForexCommodities) {
                if (symbol.Contains(i)) {
                    return InstrumentsHelper.CommoditiesContractSize;
                }
            }

            foreach (var i in InstrumentsHelper.ForexMetals) {
                if (symbol.Contains(i)) {
                    return InstrumentsHelper.MetalsContractSize;
                }
            }

            return InstrumentsHelper.ForexContractSize;
        }
    }
}
