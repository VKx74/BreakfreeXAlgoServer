using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Algoserver.Auth.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Algoserver.API.Services
{
    class HistoryRequest {
        public string Symbol { get; set; }
        public string Datafeed { get; set; }
        public string Exchange { get; set; }
        public int Granularity { get; set; }
        public int Count { get; set; }
    }

    public class ScannerHistoryService
    {
        private readonly HistoryService _historyService;
        private readonly InstrumentService _instrumentService;
        private readonly IMemoryCache _cache;
        private readonly AuthService _auth;

        public ScannerHistoryService(HistoryService historyService, InstrumentService instrumentService, IMemoryCache cache, AuthService auth)
        {
            _historyService = historyService;
            _instrumentService = instrumentService;
            _cache = cache;
            _auth = auth;
        }

        public async Task<string> Refresh()
        {
            var token = await _auth.GetAuth();
            var forexInstruments = _instrumentService.GetOandaInstruments();
            var stopWatch = new Stopwatch();
            var tasks15min = new List<HistoryRequest>();
            var tasks1h = new List<HistoryRequest>();
            var tasks4h = new List<HistoryRequest>();

            foreach (var instrument in forexInstruments)
            {
                var Exchange = instrument.Datafeed.ToLowerInvariant();
                var Datafeed = instrument.Datafeed.ToLowerInvariant();
                var Symbol = instrument.Symbol;
                tasks15min.Add(new HistoryRequest {
                    Symbol = Symbol,
                    Datafeed = Datafeed,
                    Exchange = Exchange,
                    Count = 300,
                    Granularity = TimeframeHelper.MIN15_GRANULARITY
                });
                tasks1h.Add(new HistoryRequest {
                    Symbol = Symbol,
                    Datafeed = Datafeed,
                    Exchange = Exchange,
                    Count = 300,
                    Granularity = TimeframeHelper.HOURLY_GRANULARITY
                });
                tasks4h.Add(new HistoryRequest {
                    Symbol = Symbol,
                    Datafeed = Datafeed,
                    Exchange = Exchange,
                    Count = 300,
                    Granularity = TimeframeHelper.HOUR4_GRANULARITY
                });
            }
            
            stopWatch.Start();
            var min15history = await this._loadPack(tasks15min, token);
            stopWatch.Stop();
            TimeSpan ts15 = stopWatch.Elapsed;

            stopWatch.Reset();
            stopWatch.Start();
            var hourlyhistory = await this._loadPack(tasks1h, token);
            stopWatch.Stop();
            TimeSpan ts1h = stopWatch.Elapsed;

            stopWatch.Reset();
            stopWatch.Start();
            var hour4history = await this._loadPack(tasks4h, token);
            stopWatch.Stop();
            TimeSpan ts4h = stopWatch.Elapsed;


            // string elapsedTime = String.Format("Total {0:00}:{1:00}", ts.Minutes, ts.Seconds);
            string elapsedTime15 = String.Format(" * 15 min {0:00}:{1:00} - data loaded " +  min15history.Count , ts15.Minutes, ts15.Seconds);
            string elapsedTime1h = String.Format(" * 1 h {0:00}:{1:00} - data loaded " +  hourlyhistory.Count , ts1h.Minutes, ts1h.Seconds);
            string elapsedTime4h = String.Format(" * 4 h {0:00}:{1:00} - data loaded " +  hour4history.Count , ts4h.Minutes, ts4h.Seconds);

            return elapsedTime15 + " - " + elapsedTime1h  + " - " + elapsedTime4h;
            
        }

        private async Task<List<HistoryData>> _loadPack(List<HistoryRequest> tasks, string token) {
            var result = new List<HistoryData>();
            var count = 5;

            while(tasks.Count > 0) {
                var tasksToProcess = tasks.Take(Math.Min(count, tasks.Count));
                
                var tasksToWait = new List<Task<HistoryData>>();
                foreach (var t in tasksToProcess) {
                    var task = _historyService.GetHistoryWithCount(t.Symbol, t.Granularity, t.Datafeed, t.Exchange, token, t.Count);
                    tasksToWait.Add(task);
                }
                var res = await Task.WhenAll<HistoryData>(tasksToWait);
                result.AddRange(res);
                tasks.RemoveRange(0, Math.Min(count, tasks.Count));
            }

            return result;
        }
    }
}
