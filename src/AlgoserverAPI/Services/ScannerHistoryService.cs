using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
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
        private readonly List<HistoryData> _15MinHistory = new List<HistoryData>();
        private readonly List<HistoryData> _1hHistory = new List<HistoryData>();
        private readonly List<HistoryData> _4hHistory = new List<HistoryData>();
        private readonly List<HistoryData> _dailyHistory = new List<HistoryData>();

        public ScannerHistoryService(HistoryService historyService, InstrumentService instrumentService)
        {
            _historyService = historyService;
            _instrumentService = instrumentService;
        }

        public async Task<string> Refresh() {
            var instruments = this._getInstruments();
            var stopWatch = new Stopwatch();
            var tasks15min = new List<HistoryRequest>();
            foreach (var instrument in instruments)
            {
                var Exchange = instrument.Exchange;
                var Datafeed = instrument.Datafeed;
                var Symbol = instrument.Symbol;
                tasks15min.Add(new HistoryRequest {
                    Symbol = Symbol,
                    Datafeed = Datafeed,
                    Exchange = Exchange,
                    Count = 300,
                    Granularity = TimeframeHelper.MIN15_GRANULARITY
                });
            }

            stopWatch.Start();
            var min15history = await this._loadPack(tasks15min);
            stopWatch.Stop();
            TimeSpan ts15 = stopWatch.Elapsed;

            lock (_15MinHistory) {
                _15MinHistory.Clear();
                _15MinHistory.AddRange(min15history);
            }

            _updateHigherTimeframes();

            string elapsedTime15 = String.Format(" * 15 min {0:00}:{1:00} - data loaded " +  min15history.Count , ts15.Minutes, ts15.Seconds);
            Console.WriteLine(">>> " + elapsedTime15);
            return elapsedTime15;
        }

        public async Task<string> RefreshAll()
        {
            var instruments = this._getInstruments();
            var stopWatch = new Stopwatch();
            var tasks15min = new List<HistoryRequest>();
            var tasks1h = new List<HistoryRequest>();
            var tasks4h = new List<HistoryRequest>();
            var tasks1d = new List<HistoryRequest>();

            foreach (var instrument in instruments)
            {
                var Exchange = instrument.Exchange;
                var Datafeed = instrument.Datafeed;
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
                tasks1d.Add(new HistoryRequest {
                    Symbol = Symbol,
                    Datafeed = Datafeed,
                    Exchange = Exchange,
                    Count = 200,
                    Granularity = TimeframeHelper.DAILY_GRANULARITY
                });
            }
            
            stopWatch.Start();
            var min15history = await this._loadPack(tasks15min);
            stopWatch.Stop();
            TimeSpan ts15 = stopWatch.Elapsed;

            stopWatch.Reset();
            stopWatch.Start();
            var hourlyhistory = await this._loadPack(tasks1h);
            stopWatch.Stop();
            TimeSpan ts1h = stopWatch.Elapsed;

            stopWatch.Reset();
            stopWatch.Start();
            var hour4history = await this._loadPack(tasks4h);
            stopWatch.Stop();
            TimeSpan ts4h = stopWatch.Elapsed;
            
            stopWatch.Reset();
            stopWatch.Start();
            var dailyhistory = await this._loadPack(tasks1d);
            stopWatch.Stop();
            TimeSpan ts1d = stopWatch.Elapsed;

            lock (_15MinHistory) {
                _15MinHistory.Clear();
                _15MinHistory.AddRange(min15history);
            }

            lock (_1hHistory) {
                _1hHistory.Clear();
                _1hHistory.AddRange(hourlyhistory);
            }

            lock (_4hHistory) {
                _4hHistory.Clear();
                _4hHistory.AddRange(hour4history);
            }
            
            lock (_dailyHistory) {
                _dailyHistory.Clear();
                _dailyHistory.AddRange(dailyhistory);
            }

            // string elapsedTime = String.Format("Total {0:00}:{1:00}", ts.Minutes, ts.Seconds);
            string elapsedTime15 = String.Format(" * 15 min {0:00}:{1:00} - data loaded " +  min15history.Count , ts15.Minutes, ts15.Seconds);
            string elapsedTime1h = String.Format(" * 1 h {0:00}:{1:00} - data loaded " +  hourlyhistory.Count , ts1h.Minutes, ts1h.Seconds);
            string elapsedTime4h = String.Format(" * 4 h {0:00}:{1:00} - data loaded " +  hour4history.Count , ts4h.Minutes, ts4h.Seconds);
            string elapsedTime1d = String.Format(" * 1 d {0:00}:{1:00} - data loaded " +  dailyhistory.Count , ts1d.Minutes, ts1d.Seconds);
            Console.WriteLine(">>> " + elapsedTime15 + " - " + elapsedTime1h  + " - " + elapsedTime4h  + " - " + elapsedTime1d);
            return elapsedTime15 + " - " + elapsedTime1h  + " - " + elapsedTime4h  + " - " + elapsedTime1d;
            
        }

        public List<HistoryData> Get15MinData() {
            lock(_15MinHistory) {
                return _15MinHistory.ToList();
            }
        }   
        
        public List<HistoryData> Get1HData() {
            lock(_1hHistory) {
                return _1hHistory.ToList();
            }
        } 
        
        public List<HistoryData> Get4HData() {
            lock(_4hHistory) {
                return _4hHistory.ToList();
            }
        }  
        
        public Dictionary<string, HistoryData> Get15MinDataDictionary() {
            lock(_15MinHistory) {
                return _15MinHistory.ToDictionary(_ => GetKey(_));
            }
        }   
        
        public Dictionary<string, HistoryData> Get1HDataDictionary() {
            lock(_1hHistory) {
                return _1hHistory.ToDictionary(_ => GetKey(_));
            }
        } 
        
        public Dictionary<string, HistoryData> Get4HDataDictionary() {
            lock(_4hHistory) {
                return _4hHistory.ToDictionary(_ => GetKey(_));
            }
        }
        public Dictionary<string, HistoryData> Get1DDataDictionary() {
            lock(_dailyHistory) {
                return _dailyHistory.ToDictionary(_ => GetKey(_));
            }
        }

        public List<HistoryData> Get1DData() {
            lock(_dailyHistory) {
                return _dailyHistory.ToList();
            }
        }

        private async Task<List<HistoryData>> _loadPack(List<HistoryRequest> tasks) {
            var result = new List<HistoryData>();
            var count = 3;

            while(tasks.Count > 0) {
                var tasksToProcess = tasks.Take(Math.Min(count, tasks.Count));
                
                var tasksToWait = new List<Task<HistoryData>>();
                foreach (var t in tasksToProcess) {
                    try {
                        var task = _historyService.GetHistoryWithCount(t.Symbol, t.Granularity, t.Datafeed, t.Exchange, t.Count);
                        tasksToWait.Add(task);
                    } catch(Exception ex) {

                    }
                }
                try {
                    var res = await Task.WhenAll<HistoryData>(tasksToWait);
                    result.AddRange(res);
                } catch(Exception ex) {
                    
                }
                tasks.RemoveRange(0, Math.Min(count, tasks.Count));
            }

            return result;
        }

        private void _updateHigherTimeframes() {
            var _15Mins = Get15MinData();
            var _1Hour = Get1HDataDictionary();
            var _4Hour = Get4HDataDictionary();
            var _1Day = Get1DDataDictionary();

            foreach (var history15Min in _15Mins) {
                if (_1Hour.TryGetValue(GetKey(history15Min), out var history1H)) {
                    _updateLastBar(history15Min, history1H);
                }

                if (_4Hour.TryGetValue(GetKey(history15Min), out var history4H)) {
                    _updateLastBar(history15Min, history4H);
                }

                if (_1Day.TryGetValue(GetKey(history15Min), out var history1D)) {
                    _updateLastBar(history15Min, history1D);
                }
            }
        }

        public string GetKey(HistoryData data) {
            return data.Symbol + data.Exchange;
        }
        private void _updateLastBar(HistoryData hLow, HistoryData hHigh) {
            var length = hLow.Bars.Count();
            if (length < 2)  {
                return;
            }

            var lastL = hLow.Bars.LastOrDefault();
            var preL = hLow.Bars.ElementAt(length - 1);
            var lastH = hHigh.Bars.LastOrDefault();

            if (lastH == null || lastL == null) {
                return;
            }

            lastH.Close = lastL.Close;
            lastH.High = Math.Max(lastL.High, lastH.High);
            lastH.Low = Math.Min(lastL.Low, lastH.Low);

            if (lastH.Timestamp < preL.Timestamp) {
                lastH.High = Math.Max(preL.High, lastH.High);
                lastH.Low = Math.Min(preL.Low, lastH.Low);
            }
        }

        private bool _isSameInstrument(HistoryData h1, HistoryData h2) {
            return String.Equals(h1.Exchange, h2.Exchange) || String.Equals(h1.Symbol, h2.Symbol);
        }

        private List<IInstrument> _getInstruments() {
            var instruments = new List<IInstrument>();

            var forexInstruments = _instrumentService.GetOandaInstruments();
            var stockInstruments = _instrumentService.GetTwelvedataInstruments();
            var allowedStocks = InstrumentsHelper.StockInstrumentList;
            var allowedForex = InstrumentsHelper.ForexInstrumentList;

            foreach (var instrument in forexInstruments) {
                if (allowedForex.Any(_ => String.Equals(_, instrument.Symbol, StringComparison.InvariantCultureIgnoreCase))) {
                    if (!instruments.Any(_ => String.Equals(_.Symbol, instrument.Symbol, StringComparison.InvariantCultureIgnoreCase) && String.Equals(_.Exchange, instrument.Exchange, StringComparison.InvariantCultureIgnoreCase))) {
                        instruments.Add(instrument);
                    }
                }
            } 
            
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
