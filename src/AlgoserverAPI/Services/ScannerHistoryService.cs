using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Microsoft.Extensions.Caching.Memory;

namespace Algoserver.API.Services
{
    public class ScannerHistoryService
    {
        private readonly HistoryService _historyService;
        private readonly InstrumentService _instrumentService;
        private readonly IMemoryCache _cache;
        public ScannerHistoryService(HistoryService historyService, InstrumentService instrumentService, IMemoryCache cache)
        {
            _historyService = historyService;
            _instrumentService = instrumentService;
            _cache = cache;
        }

        public async Task<string> Refresh()
        {
            var forexInstruments = _instrumentService.GetOandaInstruments();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var instrument in forexInstruments)
            {
                var Exchange = instrument.Datafeed.ToLowerInvariant();
                var Datafeed = instrument.Datafeed.ToLowerInvariant();
                var Type = instrument.Type.ToLowerInvariant();
                var Symbol = instrument.Symbol;
                
                var hour4 = _historyService.GetHistory(Symbol, TimeframeHelper.HOUR4_GRANULARITY, Datafeed, Exchange, Type);
                var hourly = _historyService.GetHistory(Symbol, TimeframeHelper.HOURLY_GRANULARITY, Datafeed, Exchange, Type);
                var min15 = _historyService.GetHistory(Symbol, TimeframeHelper.MIN15_GRANULARITY, Datafeed, Exchange, Type);
                var task = await Task.WhenAll<HistoryData>(new[] { hour4, hourly, min15 });
                var hour4PriceData = task[0];
                var hourlyPriceData = task[1];
                var min15PriceData = task[2];
                // Console.WriteLine(">>> Instruments loaded: " + Symbol);
            }

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            return elapsedTime;
        }
    }
}
