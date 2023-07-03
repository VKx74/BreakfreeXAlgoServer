using System;
using System.Collections.Generic;
using System.Linq;

namespace Algoserver.API.Helpers
{
    public static class InstrumentTypes
    {
        public static string Bonds = "Bonds";
        public static string Commodities = "Commodities";
        public static string Crypto = "Crypto";
        public static string Equities = "Equities";
        public static string ForexExotics = "ForexExotics";
        public static string Indices = "Indices";
        public static string MajorForex = "MajorForex";
        public static string Metals = "Metals";
        public static string ForexMinors = "ForexMinors";
        public static string Other = "Other";

    }

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
                    new InstrumentDescription { Symbol = "AAPL", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "AMD", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "AMZN", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "ATVI", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "BA", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "BAC", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "BIDU", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "C-J", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "FB", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "FCX", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "FSLR", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "GM", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "GOOG", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "GOOGL", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "GS", Exchange = "NYSE" },
                    // new InstrumentDescription { Symbol = "GWPH", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "JPM", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "MCD", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "MSFT", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "NFLX", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "NKE", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "NVDA", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "PFE", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "PGHH", Exchange = "NSE" },
                    new InstrumentDescription { Symbol = "PTON", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "QCOM", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "RACE", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "SNAP", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "TLRY", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "BYND", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "TSLA", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "TWTR", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "UBER", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "WMT", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "XOM", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "ZM", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "GE", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "AAL", Exchange = "NASDAQ" },
                    // new InstrumentDescription { Symbol = "DELTA", Exchange = "BSE" },
                    new InstrumentDescription { Symbol = "GPRO", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "ACB", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "CUK", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "CCL", Exchange = "BSE" },
                    new InstrumentDescription { Symbol = "NCLH", Exchange = "NYSE" },
                    // new InstrumentDescription { Symbol = "FIT", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "PLUG", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "UAL", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "WEED", Exchange = "TSX" },
                    new InstrumentDescription { Symbol = "CSCO", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "TSM", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "BABA", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "V", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "UNH", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "JNJ", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "HD", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "MA", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "ADBE", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "DIS", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "PYPL", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "NKE", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "ORCL", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "NVO", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "TM", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "KO", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "CMCSA", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "ABT", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "ACN", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "PEP", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "VZ", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "COST", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "INTC", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "AZN", Exchange = "NASDAQ" },
                    new InstrumentDescription { Symbol = "UPS", Exchange = "NYSE" },
                    new InstrumentDescription { Symbol = "SONY", Exchange = "NYSE" },
                    // new InstrumentDescription { Symbol = "LLOY", Exchange = "LSE" },
                    // new InstrumentDescription { Symbol = "BARC", Exchange = "LSE" },
                    // new InstrumentDescription { Symbol = "BIDS", Exchange = "LSE" },
                    // new InstrumentDescription { Symbol = "GST", Exchange = "LSE" },
                    // new InstrumentDescription { Symbol = "VOD", Exchange = "LSE" },
                    // new InstrumentDescription { Symbol = "ALBA", Exchange = "LSE" },
                    // new InstrumentDescription { Symbol = "KOD", Exchange = "LSE" },
                    // new InstrumentDescription { Symbol = "BP", Exchange = "LSE" },
                    // new InstrumentDescription { Symbol = "TSCO", Exchange = "LSE" },
                    // new InstrumentDescription { Symbol = "WTE", Exchange = "LSE" },
                    // new InstrumentDescription { Symbol = "PH0", Exchange = "SGX" },
                    // new InstrumentDescription { Symbol = "5HH", Exchange = "SGX" },
                    // new InstrumentDescription { Symbol = "579", Exchange = "SGX" },
                    // new InstrumentDescription { Symbol = "D05", Exchange = "SGX" },
                    // new InstrumentDescription { Symbol = "C8R", Exchange = "SGX" },
                    // new InstrumentDescription { Symbol = "IJ8", Exchange = "FSX" },
                    // new InstrumentDescription { Symbol = "SNH", Exchange = "FSX" },
                    // new InstrumentDescription { Symbol = "AYJ", Exchange = "FSX" },
                    // new InstrumentDescription { Symbol = "LHA", Exchange = "FSX" },
                    // new InstrumentDescription { Symbol = "VODI", Exchange = "FSX" },
                    // new InstrumentDescription { Symbol = "DTE", Exchange = "FSX" },
                    // new InstrumentDescription { Symbol = "TUI1", Exchange = "FSX" },
                    // new InstrumentDescription { Symbol = "CBK", Exchange = "FSX" },
                    // new InstrumentDescription { Symbol = "AXA", Exchange = "FSX" }
                };
            }
        }
        public static List<InstrumentDescription> CryptoInstrumentList
        {
            get
            {
                return new List<InstrumentDescription> {
                    // new InstrumentDescription { Symbol = "ethbtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "xrpbtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "xlmbtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "ltcbtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "linkbtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "yfibtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "adabtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "xembtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "xmrbtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "bchbtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "eosbtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "trxbtc", Exchange = "Binance" },
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
                    new InstrumentDescription { Symbol = "solusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "avaxusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "dotusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "dogeusdt", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "ftmusdt", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "omgusdt", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "fttusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "filusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "axsusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "bnbusdt", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "vetusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "thetausdt", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "icpusdt", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "xtzusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "etcusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "aaveusdt", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "ksmusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "iotausdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "maticusdt", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "cakeusdt", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "solbtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "avaxbtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "dotbtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "lunabtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "bnbbtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "dogebtc", Exchange = "Binance" },
                    // new InstrumentDescription { Symbol = "etcbtc", Exchange = "Binance" },
                    new InstrumentDescription { Symbol = "eosusdt", Exchange = "Binance" }
                };
            }
        }

        public static List<string> Metals
        {
            get
            {
                return new List<string> {
                    "XAU", "XAG", "XPD", "XCU"
                };
            }
        }

        public static List<string> ForexMetals
        {
            get
            {
                return new List<string> {
                        "XAG_EUR", "XAG_USD", "XAU_EUR", "XAU_USD", "XAU_XAG", "XPD_USD", "XPT_USD"
                    };
            }
        }

        public static List<string> ForexIndices
        {
            get
            {
                return new List<string> {
                    "AU200_AUD", "CN50_USD", "EU50_EUR", "FR40_EUR", "DE30_EUR", "HK33_HKD", "IN50_USD", "JP225_USD", "NL25_EUR",
                    "SG30_SGD", "TWIX_USD", "UK100_GBP", "NAS100_USD", "US2000_USD", "SPX500_USD", "US30_USD"
                };
            }
        }

        public static List<string> ForexBounds
        {
            get
            {
                return new List<string> {
                    "DE10YB_EUR", "UK100_GBP", "USB02Y_USD", "USB30Y_USD"
                };
            }
        }

        public static List<string> ForexCommodities
        {
            get
            {
                return new List<string> {
                    "BCO_USD", "CORN_USD", "NATGAS_USD", "SOYBN_USD", "WHEAT_USD", "WTICO_USD", "XCU_USD"
                };
            }
        }

        public static List<string> ForexExotic
        {
            get
            {
                return new List<string> {
                    "AUD_SGD", "EUR_DKK", "EUR_HKD", "EUR_NOK", "EUR_PLN", "EUR_SEK", "EUR_SGD", "EUR_TRY", "EUR_ZAR", "SGD_JPY", "USD_CNH", "NZD_SGD",
                    "GBP_SGD", "USD_CZK", "USD_DKK", "USD_HKD", "USD_MXN", "USD_NOK", "USD_PLN", "USD_RUB", "USD_SEK", "USD_THB", "USD_TRY", "USD_ZAR"
                };
            }
        }

        public static List<string> ForexMajor
        {
            get
            {
                return new List<string> {
                    "AUD_USD", "EUR_JPY", "EUR_USD", "GBP_JPY", "GBP_USD", "NZD_USD", "USD_CAD", "USD_CHF", "USD_JPY"
                };
            }
        }

        public static List<string> ForexMinors
        {
            get
            {
                return new List<string> {
                    "AUD_NZD", "AUD_CAD", "AUD_JPY", "CHF_JPY", "EUR_GBP", "EUR_AUD", "EUR_CHF", "EUR_NZD", "EUR_CAD", "GBP_CHF",
                    "CAD_CHF", "CAD_JPY", "GBP_AUD", "GBP_CAD", "GBP_NZD", "NZD_CAD", "NZD_CHF", "NZD_JPY", "USD_SGD"
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
                    "UK100_GBP",
                    "US2000_USD",
                    "US30_USD",
                    "USB02Y_USD",
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

        public static List<string> ForexInstrumentListShort
        {
            get
            {
                return new List<string> {
                   "GBP_USD", "EUR_USD", "USD_JPY", "AUD_USD", "USD_CAD", "USD_CHF','NZD_USD", "EUR_GBP", "EUR_JPY", "EUR_CHF", "EUR_AUD", "EUR_CAD", "EUR_NZD", "GBP_JPY", "GBP_CHF", "GBP_AUD", "GBP_CAD", "GBP_NZD", "AUD_JPY", "AUD_CHF", "AUD_CAD", "AUD_NZD", "CAD_JPY", "CAD_CHF", "NZD_JPY", "NZD_CHF", "CHF_JPY", "XAU_USD", "XAG_USD", "SPX500_USD", "NAS100_USD", "US30_USD", "BCO_USD", "WTICO_USD"
                };
            }
        }

        public static decimal GetContractSize(string symbol)
        {
            foreach (var i in InstrumentsHelper.ForexIndices)
            {
                if (symbol.Contains(i))
                {
                    return InstrumentsHelper.IndicesContractSize;
                }
            }

            foreach (var i in InstrumentsHelper.ForexBounds)
            {
                if (symbol.Contains(i))
                {
                    return InstrumentsHelper.BoundsContractSize;
                }
            }

            foreach (var i in InstrumentsHelper.ForexCommodities)
            {
                if (symbol.Contains(i))
                {
                    return InstrumentsHelper.CommoditiesContractSize;
                }
            }

            foreach (var i in InstrumentsHelper.Metals)
            {
                if (symbol.Contains(i))
                {
                    return InstrumentsHelper.MetalsContractSize;
                }
            }

            return InstrumentsHelper.ForexContractSize;
        }

        public static string NormalizeInstrument(string symbol)
        {
            return symbol.Replace("_", "").Replace("/", "").ToLower();
        }

        public static string GetInstrumentType(string symbol)
        {
            var normalizedInstrument = NormalizeInstrument(symbol);

            if (StockInstrumentList.Any(_ => String.Equals(NormalizeInstrument(_.Symbol), normalizedInstrument, StringComparison.InvariantCultureIgnoreCase)))
            {
                return InstrumentTypes.Equities;
            }

            if (CryptoInstrumentList.Any(_ => String.Equals(NormalizeInstrument(_.Symbol), normalizedInstrument, StringComparison.InvariantCultureIgnoreCase)))
            {
                return InstrumentTypes.Crypto;
            }

            if (ForexMetals.Any(_ => String.Equals(NormalizeInstrument(_), normalizedInstrument, StringComparison.InvariantCultureIgnoreCase)))
            {
                return InstrumentTypes.Metals;
            }

            if (ForexIndices.Any(_ => String.Equals(NormalizeInstrument(_), normalizedInstrument, StringComparison.InvariantCultureIgnoreCase)))
            {
                return InstrumentTypes.Indices;
            }

            if (ForexBounds.Any(_ => String.Equals(NormalizeInstrument(_), normalizedInstrument, StringComparison.InvariantCultureIgnoreCase)))
            {
                return InstrumentTypes.Bonds;
            }

            if (ForexCommodities.Any(_ => String.Equals(NormalizeInstrument(_), normalizedInstrument, StringComparison.InvariantCultureIgnoreCase)))
            {
                return InstrumentTypes.Commodities;
            }

            if (ForexExotic.Any(_ => String.Equals(NormalizeInstrument(_), normalizedInstrument, StringComparison.InvariantCultureIgnoreCase)))
            {
                return InstrumentTypes.ForexExotics;
            }

            if (ForexMajor.Any(_ => String.Equals(NormalizeInstrument(_), normalizedInstrument, StringComparison.InvariantCultureIgnoreCase)))
            {
                return InstrumentTypes.MajorForex;
            }

            if (ForexMinors.Any(_ => String.Equals(NormalizeInstrument(_), normalizedInstrument, StringComparison.InvariantCultureIgnoreCase)))
            {
                return InstrumentTypes.ForexMinors;
            }

            return InstrumentTypes.Other;
        }
    }
}
