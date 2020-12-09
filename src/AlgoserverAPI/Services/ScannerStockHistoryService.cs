using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public class ScannerStockHistoryService : ScannerHistoryService
    {
        public ScannerStockHistoryService(HistoryService historyService, InstrumentService instrumentService): base(historyService, instrumentService)
        {
        }

        protected override List<IInstrument> _getInstruments() 
        {
            var instruments = new List<IInstrument>();
            var stockInstruments = _instrumentService.GetTwelvedataInstruments();
            var allowedStocks = InstrumentsHelper.StockInstrumentList;

            foreach (var instrument in stockInstruments) {
                if (allowedStocks.Any(_ => String.Equals(_.Symbol, instrument.Symbol, StringComparison.InvariantCultureIgnoreCase) && String.Equals(_.Exchange, instrument.Exchange, StringComparison.InvariantCultureIgnoreCase))) {
                    if (!instruments.Any(_ => String.Equals(_.Symbol, instrument.Symbol, StringComparison.InvariantCultureIgnoreCase) && String.Equals(_.Exchange, instrument.Exchange, StringComparison.InvariantCultureIgnoreCase))) {
                        instruments.Add(instrument);
                    }
                }
            }
            
            return instruments;
        }
    }
}