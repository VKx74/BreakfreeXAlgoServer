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
        public ScannerForexHistoryService(HistoryService historyService, InstrumentService instrumentService, ICacheService cache): base(historyService, instrumentService, cache)
        {
        }

        protected override List<IInstrument> _getInstruments() 
        {
            var instruments = new List<IInstrument>();
            var forexInstruments = _instrumentService.GetOandaInstruments();
            // var forexInstruments = _instrumentService.GetTwelvedataInstruments();
            var allowedForex = InstrumentsHelper.ForexInstrumentList;

            foreach (var instrument in forexInstruments) {
                if (allowedForex.Any(_ => String.Equals(_.Replace("_", "").Replace("/", ""), instrument.Symbol.Replace("_", "").Replace("/", ""), StringComparison.InvariantCultureIgnoreCase))) {
                    if (!instruments.Any(_ => String.Equals(_.Symbol, instrument.Symbol, StringComparison.InvariantCultureIgnoreCase) && String.Equals(_.Exchange, instrument.Exchange, StringComparison.InvariantCultureIgnoreCase))) {
                        instruments.Add(instrument);
                    }
                }
            } 
            // return instruments.Take(3).ToList();
            return instruments;
        }
    }
}