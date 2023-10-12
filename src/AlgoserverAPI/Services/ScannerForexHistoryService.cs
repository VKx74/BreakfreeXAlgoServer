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

        public override List<IInstrument> getInstruments()
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

            // return instruments.Take(1).ToList();
            return instruments;
        }

        public override List<IInstrument> getInstrumentsForLongHistory()
        {
            var instruments = new List<IInstrument>();
            var forexInstruments = _instrumentService.GetOandaInstruments();
            var allowedForex = InstrumentsHelper.ForexInstrumentList;
            var excludeInstruments =InstrumentsHelper.ExcludeListForLongHistory;

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

            // return instruments.Take(1).ToList();
            return instruments;
        }
    }
}