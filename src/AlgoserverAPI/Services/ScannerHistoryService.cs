using System;
using System.Collections.Generic;
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
            var tasks15min = new List<Task<HistoryData>>();
            var tasks1h = new List<Task<HistoryData>>();
            var tasks4h = new List<Task<HistoryData>>();

            foreach (var instrument in forexInstruments)
            {
                var Exchange = instrument.Datafeed.ToLowerInvariant();
                var Datafeed = instrument.Datafeed.ToLowerInvariant();
                var Symbol = instrument.Symbol;
                
                var hour4 = _historyService.GetHistory(Symbol, TimeframeHelper.HOUR4_GRANULARITY, Datafeed, Exchange, 300);
                var hourly = _historyService.GetHistory(Symbol, TimeframeHelper.HOURLY_GRANULARITY, Datafeed, Exchange, 300);
                var min15 = _historyService.GetHistory(Symbol, TimeframeHelper.MIN15_GRANULARITY, Datafeed, Exchange, 400);
                tasks15min.Add(min15);
                tasks1h.Add(hourly);
                tasks4h.Add(hour4);
            }
            
            stopWatch.Start();
            var min15task = await Task.WhenAll<HistoryData>(tasks15min);
            stopWatch.Stop();
            TimeSpan ts15 = stopWatch.Elapsed;

            stopWatch.Start();
            var hourlytask = await Task.WhenAll<HistoryData>(tasks1h);
            stopWatch.Stop();
            TimeSpan ts1h = stopWatch.Elapsed;

            stopWatch.Start();
            var hour4task = await Task.WhenAll<HistoryData>(tasks4h);
            stopWatch.Stop();
            TimeSpan ts4h = stopWatch.Elapsed;


            // string elapsedTime = String.Format("Total {0:00}:{1:00}", ts.Minutes, ts.Seconds);
            string elapsedTime15 = String.Format("15 min {0:00}:{1:00}", ts15.Minutes, ts15.Seconds);
            string elapsedTime1h = String.Format("1 h {0:00}:{1:00}", ts1h.Minutes, ts1h.Seconds);
            string elapsedTime4h = String.Format("4 h {0:00}:{1:00}", ts4h.Minutes, ts4h.Seconds);

            return elapsedTime15 + " - " + elapsedTime1h  + " - " + elapsedTime4h;
            
        }
    }
}
