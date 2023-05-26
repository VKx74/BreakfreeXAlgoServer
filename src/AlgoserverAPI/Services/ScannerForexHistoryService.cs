using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;
using Algoserver.API.Services.CacheServices;

namespace Algoserver.API.Services
{
    public class ScannerForexHistoryService : ScannerHistoryService
    {
        public ScannerForexHistoryService(HistoryService historyService, InstrumentService instrumentService, ICacheService cache) : base(historyService, instrumentService, cache)
        {
        }

        protected override List<IInstrument> _getInstruments()
        {
            var instruments = new List<IInstrument>();
            var forexInstruments = _instrumentService.GetOandaInstruments();
            // var forexInstruments = _instrumentService.GetTwelvedataInstruments();
            var allowedForex = InstrumentsHelper.ForexInstrumentList;

            foreach (var instrument in forexInstruments)
            {
                if (allowedForex.Any(_ => String.Equals(_.Replace("_", "").Replace("/", ""), instrument.Symbol.Replace("_", "").Replace("/", ""), StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (!instruments.Any(_ => String.Equals(_.Symbol, instrument.Symbol, StringComparison.InvariantCultureIgnoreCase) && String.Equals(_.Exchange, instrument.Exchange, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        instruments.Add(instrument);
                    }
                }
            }

            instruments.Add(new OandaInstruments
            {
                Datafeed = "Oanda",
                Exchange = "Oanda",
                Symbol = "BTC_USD",
                Type = "Crypto",
                PricePrecision = 0.00000001m
            });
            instruments.Add(new OandaInstruments
            {
                Datafeed = "Oanda",
                Exchange = "Oanda",
                Symbol = "ETH_USD",
                Type = "Crypto",
                PricePrecision = 0.00000001m
            });

            // return instruments.Take(5).ToList();
            return instruments;
        }

        protected override List<IInstrument> _getInstrumentsForLongHistory()
        {
            var instruments = new List<IInstrument>();
            var forexInstruments = _instrumentService.GetOandaInstruments();
            var allowedForex = InstrumentsHelper.ForexInstrumentList;
            var excludeInstruments = new List<string>{"EUR_SEK", "USD_SEK", "USD_NOK", "USD_HUF", "CAD_SGD", "EUR_NOK", "USD_MXN", "SGD_CHF", "NZD_SGD", "AU200_AUD", "DE10YB_EUR", "CORN_USD", "CN50_USD", "DE30_EUR", "EU50_EUR", "FR40_EUR", "HK33_HKD", "IN50_USD", "NL25_EUR", "SG30_SGD", "SUGAR_USD", "UK100_GBP", "UK10YB_GBP", "USB02Y_USD", "USB05Y_USD", "USB10Y_USD", "USB30Y_USD"};

            foreach (var instrument in forexInstruments)
            {
                if (excludeInstruments.Any(_ => String.Equals(_, instrument.Symbol, StringComparison.InvariantCultureIgnoreCase))) {
                    continue;
                }
                if (allowedForex.Any(_ => String.Equals(_.Replace("_", "").Replace("/", ""), instrument.Symbol.Replace("_", "").Replace("/", ""), StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (!instruments.Any(_ => String.Equals(_.Symbol, instrument.Symbol, StringComparison.InvariantCultureIgnoreCase) && String.Equals(_.Exchange, instrument.Exchange, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        instruments.Add(instrument);
                    }
                }
            }

            instruments.Add(new OandaInstruments
            {
                Datafeed = "Oanda",
                Exchange = "Oanda",
                Symbol = "BTC_USD",
                Type = "Crypto",
                PricePrecision = 0.00000001m
            });
            instruments.Add(new OandaInstruments
            {
                Datafeed = "Oanda",
                Exchange = "Oanda",
                Symbol = "ETH_USD",
                Type = "Crypto",
                PricePrecision = 0.00000001m
            });

            // return instruments.Take(5).ToList();
            return instruments;
        }
    }
}