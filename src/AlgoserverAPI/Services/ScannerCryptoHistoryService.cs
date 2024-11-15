using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;
using Algoserver.API.Services.CacheServices;

namespace Algoserver.API.Services
{
    public class ScannerCryptoHistoryService : ScannerHistoryService
    {
        public ScannerCryptoHistoryService(HistoryService historyService, InstrumentService instrumentService, IInMemoryCache cache): base(historyService, instrumentService, cache)
        {
        }

        public override List<IInstrument> getInstruments() 
        {
            var instruments = new List<IInstrument>();
            var cryptoInstruments = _instrumentService.GetBinanceInstruments();
            var allowedCrypto = InstrumentsHelper.CryptoInstrumentList;

            foreach (var instrument in cryptoInstruments) {
                var symbol = instrument.Symbol.Replace("-", "");
                if (allowedCrypto.Any(_ => String.Equals(_.Symbol, symbol, StringComparison.InvariantCultureIgnoreCase) && String.Equals(_.Exchange, instrument.Exchange, StringComparison.InvariantCultureIgnoreCase))) {
                    if (!instruments.Any(_ => String.Equals(_.Symbol, instrument.Symbol, StringComparison.InvariantCultureIgnoreCase) && String.Equals(_.Exchange, instrument.Exchange, StringComparison.InvariantCultureIgnoreCase))) {
                        instruments.Add(instrument);
                    }
                }
            }

            return instruments;
        }

        public override List<IInstrument> getInstrumentsForLongHistory() 
        {
            var instruments = new List<IInstrument>();
            return instruments;
        }
    }
}