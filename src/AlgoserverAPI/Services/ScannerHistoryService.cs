using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Algoserver.API.Services.CacheServices;

namespace Algoserver.API.Services
{
    public class HistoryRequest
    {
        public string Symbol { get; set; }
        public string Datafeed { get; set; }
        public string Exchange { get; set; }
        public int Granularity { get; set; }
        public int Count { get; set; }
    }

    public abstract class ScannerHistoryService
    {
        private static int longMinHistoryCount = 21600;
        private string _cachePrefix = "HistoryCache_";
        private readonly ICacheService _cache;
        protected readonly HistoryService _historyService;
        protected readonly InstrumentService _instrumentService;
        protected readonly List<HistoryData> _1MinLongHistory = new List<HistoryData>();
        protected readonly List<HistoryData> _1MinHistory = new List<HistoryData>();
        protected readonly List<HistoryData> _5MinHistory = new List<HistoryData>();
        protected readonly List<HistoryData> _15MinHistory = new List<HistoryData>();
        protected readonly List<HistoryData> _30MinHistory = new List<HistoryData>();
        protected readonly List<HistoryData> _1hHistory = new List<HistoryData>();
        protected readonly List<HistoryData> _4hHistory = new List<HistoryData>();
        protected readonly List<HistoryData> _dailyHistory = new List<HistoryData>();

        public ScannerHistoryService(HistoryService historyService, InstrumentService instrumentService, ICacheService cache)
        {
            _historyService = historyService;
            _instrumentService = instrumentService;
            _cache = cache;
        }

        public async Task<string> Refresh()
        {
            var instruments = this.getInstrumentsForLongHistory();
            var stopWatch = new Stopwatch();
            var tasks1min = new List<HistoryRequest>();
            foreach (var instrument in instruments)
            {
                var Exchange = instrument.Exchange;
                var Datafeed = instrument.Datafeed;
                var Symbol = instrument.Symbol;
                tasks1min.Add(new HistoryRequest
                {
                    Symbol = Symbol,
                    Datafeed = Datafeed,
                    Exchange = Exchange,
                    Count = 300,
                    Granularity = TimeframeHelper.MIN1_GRANULARITY
                });
            }

            stopWatch.Start();
            var min1history = await this._loadPack(tasks1min, 10);
            stopWatch.Stop();
            TimeSpan ts1 = stopWatch.Elapsed;

            lock (_1MinHistory)
            {
                _1MinHistory.Clear();
                _1MinHistory.AddRange(min1history);
            }

            try
            {
                _updateHigherTimeframes();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                _updateMinuteLongHistory();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            string elapsedTime1 = String.Format(" * 1 min {0:00}:{1:00} - data loaded " + min1history.Count, ts1.Minutes, ts1.Seconds);
            Console.WriteLine(">>> " + elapsedTime1);
            return elapsedTime1;
        }

        public async Task<string> Refresh1MinLongHistory()
        {
            var instruments = this.getInstrumentsForLongHistory();
            var stopWatch = new Stopwatch();
            var tasks1min = new List<HistoryRequest>();
            foreach (var instrument in instruments)
            {
                var Exchange = instrument.Exchange;
                var Datafeed = instrument.Datafeed;
                var Symbol = instrument.Symbol;
                tasks1min.Add(new HistoryRequest
                {
                    Symbol = Symbol,
                    Datafeed = Datafeed,
                    Exchange = Exchange,
                    Count = 50000,
                    Granularity = TimeframeHelper.MIN1_GRANULARITY
                });
            }

            stopWatch.Start();
            var min1history = await this._loadPack(tasks1min, 1);
            stopWatch.Stop();
            TimeSpan ts1 = stopWatch.Elapsed;

            lock (_1MinLongHistory)
            {
                _1MinLongHistory.Clear();
                _1MinLongHistory.AddRange(min1history);
            }

            string elapsedTime1 = String.Format(" * 1 min long history {0:00}:{1:00} - data loaded " + min1history.Count, ts1.Minutes, ts1.Seconds);
            Console.WriteLine(">>> " + elapsedTime1);
            return elapsedTime1;
        }

        public async Task<string> RefreshAll()
        {
            var instruments = this.getInstrumentsForLongHistory();
            var stopWatch = new Stopwatch();
            var tasks1min = new List<HistoryRequest>();
            var tasks5min = new List<HistoryRequest>();
            var tasks15min = new List<HistoryRequest>();
            var tasks30min = new List<HistoryRequest>();
            var tasks1h = new List<HistoryRequest>();
            var tasks4h = new List<HistoryRequest>();
            var tasks1d = new List<HistoryRequest>();

            foreach (var instrument in instruments)
            {
                var Exchange = instrument.Exchange;
                var Datafeed = instrument.Datafeed;
                var Symbol = instrument.Symbol;
                tasks1min.Add(new HistoryRequest
                {
                    Symbol = Symbol,
                    Datafeed = Datafeed,
                    Exchange = Exchange,
                    Count = 1000,
                    Granularity = TimeframeHelper.MIN1_GRANULARITY
                });
                // tasks5min.Add(new HistoryRequest {
                //     Symbol = Symbol,
                //     Datafeed = Datafeed,
                //     Exchange = Exchange,
                //     Count = 300,
                //     Granularity = TimeframeHelper.MIN5_GRANULARITY
                // });
                tasks15min.Add(new HistoryRequest
                {
                    Symbol = Symbol,
                    Datafeed = Datafeed,
                    Exchange = Exchange,
                    Count = 400,
                    Granularity = TimeframeHelper.MIN15_GRANULARITY
                });
                // tasks30min.Add(new HistoryRequest {
                //     Symbol = Symbol,
                //     Datafeed = Datafeed,
                //     Exchange = Exchange,
                //     Count = 300,
                //     Granularity = TimeframeHelper.MIN30_GRANULARITY
                // });
                tasks1h.Add(new HistoryRequest
                {
                    Symbol = Symbol,
                    Datafeed = Datafeed,
                    Exchange = Exchange,
                    Count = 3000,
                    Granularity = TimeframeHelper.HOURLY_GRANULARITY
                });
                tasks4h.Add(new HistoryRequest
                {
                    Symbol = Symbol,
                    Datafeed = Datafeed,
                    Exchange = Exchange,
                    Count = 200,
                    Granularity = TimeframeHelper.HOUR4_GRANULARITY
                });
                tasks1d.Add(new HistoryRequest
                {
                    Symbol = Symbol,
                    Datafeed = Datafeed,
                    Exchange = Exchange,
                    Count = 3000,
                    Granularity = TimeframeHelper.DAILY_GRANULARITY
                });
            }

            stopWatch.Start();
            var min1history = await this._loadPack(tasks1min);
            stopWatch.Stop();
            TimeSpan ts1 = stopWatch.Elapsed;

            var min5history = new List<HistoryData>();
            foreach (var history in min1history)
            {
                var combinedData = CombineHistory(history.Bars, 5);
                min5history.Add(new HistoryData
                {
                    Datafeed = history.Datafeed,
                    Exchange = history.Exchange,
                    Granularity = history.Granularity,
                    Symbol = history.Symbol,
                    Bars = combinedData
                });
            }

            stopWatch.Start();
            var min15history = await this._loadPack(tasks15min);
            stopWatch.Stop();
            TimeSpan ts15 = stopWatch.Elapsed;

            var min30history = new List<HistoryData>();
            foreach (var history in min15history)
            {
                var combinedData = CombineHistory(history.Bars, 30);
                min30history.Add(new HistoryData
                {
                    Datafeed = history.Datafeed,
                    Exchange = history.Exchange,
                    Granularity = history.Granularity,
                    Symbol = history.Symbol,
                    Bars = combinedData
                });
            }
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

            lock (_1MinHistory)
            {
                _1MinHistory.Clear();
                _1MinHistory.AddRange(min1history);
            }

            lock (_5MinHistory)
            {
                _5MinHistory.Clear();
                _5MinHistory.AddRange(min5history);
            }

            lock (_15MinHistory)
            {
                _15MinHistory.Clear();
                _15MinHistory.AddRange(min15history);
            }

            lock (_30MinHistory)
            {
                _30MinHistory.Clear();
                _30MinHistory.AddRange(min30history);
            }

            lock (_1hHistory)
            {
                _1hHistory.Clear();
                _1hHistory.AddRange(hourlyhistory);
            }

            lock (_4hHistory)
            {
                _4hHistory.Clear();
                _4hHistory.AddRange(hour4history);
            }

            lock (_dailyHistory)
            {
                _dailyHistory.Clear();
                _dailyHistory.AddRange(dailyhistory);
            }

            try
            {
                _updateMinuteLongHistory();
            }
            catch (Exception ex) { }

            // string elapsedTime = String.Format("Total {0:00}:{1:00}", ts.Minutes, ts.Seconds);
            string elapsedTime1 = String.Format(" * 1 min {0:00}:{1:00} - data loaded " + min1history.Count, ts1.Minutes, ts1.Seconds);
            string elapsedTime15 = String.Format(" * 15 min {0:00}:{1:00} - data loaded " + min15history.Count, ts15.Minutes, ts15.Seconds);
            string elapsedTime1h = String.Format(" * 1 h {0:00}:{1:00} - data loaded " + hourlyhistory.Count, ts1h.Minutes, ts1h.Seconds);
            string elapsedTime4h = String.Format(" * 4 h {0:00}:{1:00} - data loaded " + hour4history.Count, ts4h.Minutes, ts4h.Seconds);
            string elapsedTime1d = String.Format(" * 1 d {0:00}:{1:00} - data loaded " + dailyhistory.Count, ts1d.Minutes, ts1d.Seconds);
            Console.WriteLine(">>> " + elapsedTime1 + " - " + elapsedTime15 + " - " + elapsedTime1h + " - " + elapsedTime4h + " - " + elapsedTime1d);
            return elapsedTime1 + " - " + elapsedTime15 + " - " + elapsedTime1h + " - " + elapsedTime4h + " - " + elapsedTime1d;

        }

        public List<HistoryData> Get1MinData()
        {
            lock (_1MinHistory)
            {
                return _1MinHistory.ToList();
            }
        }

        public List<HistoryData> Get1MinLongData()
        {
            lock (_1MinLongHistory)
            {
                return _1MinLongHistory.ToList();
            }
        }

        public List<HistoryData> Get5MinData()
        {
            lock (_5MinHistory)
            {
                return _5MinHistory.ToList();
            }
        }

        public List<HistoryData> Get15MinData()
        {
            lock (_15MinHistory)
            {
                return _15MinHistory.ToList();
            }
        }

        public List<HistoryData> Get30MinData()
        {
            lock (_30MinHistory)
            {
                return _30MinHistory.ToList();
            }
        }

        public List<HistoryData> Get1HData()
        {
            lock (_1hHistory)
            {
                return _1hHistory.ToList();
            }
        }

        public List<HistoryData> Get4HData()
        {
            lock (_4hHistory)
            {
                return _4hHistory.ToList();
            }
        }

        public Dictionary<string, HistoryData> Get1MinDataDictionary()
        {
            lock (_1MinHistory)
            {
                return _1MinHistory.ToDictionary(_ => GetKey(_));
            }
        }

        public Dictionary<string, HistoryData> Get1MinLongDataDictionary()
        {
            lock (_1MinLongHistory)
            {
                return _1MinLongHistory.ToDictionary(_ => GetKey(_));
            }
        }

        public Dictionary<string, HistoryData> Get5MinDataDictionary()
        {
            lock (_5MinHistory)
            {
                return _5MinHistory.ToDictionary(_ => GetKey(_));
            }
        }

        public Dictionary<string, HistoryData> Get15MinDataDictionary()
        {
            lock (_15MinHistory)
            {
                return _15MinHistory.ToDictionary(_ => GetKey(_));
            }
        }

        public Dictionary<string, HistoryData> Get30MinDataDictionary()
        {
            lock (_30MinHistory)
            {
                return _30MinHistory.ToDictionary(_ => GetKey(_));
            }
        }

        public Dictionary<string, HistoryData> Get1HDataDictionary()
        {
            lock (_1hHistory)
            {
                return _1hHistory.ToDictionary(_ => GetKey(_));
            }
        }

        public Dictionary<string, HistoryData> Get4HDataDictionary()
        {
            lock (_4hHistory)
            {
                return _4hHistory.ToDictionary(_ => GetKey(_));
            }
        }
        public Dictionary<string, HistoryData> Get1DDataDictionary()
        {
            lock (_dailyHistory)
            {
                return _dailyHistory.ToDictionary(_ => GetKey(_));
            }
        }

        public List<HistoryData> Get1DData()
        {
            lock (_dailyHistory)
            {
                return _dailyHistory.ToList();
            }
        }

        protected async Task<List<HistoryData>> _loadPack(List<HistoryRequest> tasks, int defaultPackCount = 1)
        {
            var result = new List<HistoryData>();
            var count = defaultPackCount;

            while (tasks.Count > 0)
            {
                var tasksToProcess = tasks.Take(Math.Min(count, tasks.Count));

                var tasksToWait = new List<Task<HistoryData>>();
                foreach (var t in tasksToProcess)
                {
                    try
                    {
                        var task = _historyService.GetHistoryWithCount(t.Symbol, t.Granularity, t.Datafeed, t.Exchange, t.Count);
                        tasksToWait.Add(task);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(">>> HISTORY LOAD EXCEPTION: " + t.Symbol + " - " + t.Granularity);
                        Console.WriteLine(ex.ToString());
                    }
                }
                try
                {
                    var res = await Task.WhenAll<HistoryData>(tasksToWait);
                    foreach (var historyItem in res)
                    {
                        if (historyItem != null)
                        {
                            result.Add(historyItem);
                        }
                        else
                        {
                            Console.WriteLine(">>> HISTORY NOT LOADED");
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                tasks.RemoveRange(0, Math.Min(count, tasks.Count));
            }

            return result;
        }

        protected void _updateMinuteLongHistory()
        {
            var _1Mins = Get1MinData();
            var _1MinLongHistory = Get1MinLongDataDictionary();
            foreach (var history1Min in _1Mins)
            {
                var key = GetKey(history1Min);
                if (_1MinLongHistory.TryGetValue(key, out var history1MinLongData))
                {
                    if (history1MinLongData == null)
                    {
                        continue;
                    }

                    var firstBar = history1Min.Bars.FirstOrDefault();
                    if (firstBar == null)
                    {
                        continue;
                    }

                    var indexOfStart = history1MinLongData.Bars.FindIndex((_) => _.Timestamp >= firstBar.Timestamp);
                    if (indexOfStart < 0)
                    {
                        continue;
                    }

                    history1MinLongData.Bars.RemoveRange(indexOfStart, history1MinLongData.Bars.Count - indexOfStart);
                    history1MinLongData.Bars.AddRange(history1Min.Bars);

                    // tryAddHistoryInCache(history1MinLongData);
                }
            }
        }

        protected void _updateHigherTimeframes()
        {
            var _1Mins = Get1MinData();
            var _5Mins = Get5MinDataDictionary();
            var _15Mins = Get15MinDataDictionary();
            var _30Mins = Get30MinDataDictionary();
            var _1Hour = Get1HDataDictionary();
            var _4Hour = Get4HDataDictionary();
            var _1Day = Get1DDataDictionary();

            foreach (var history1Min in _1Mins)
            {
                var key = GetKey(history1Min);

                if (_5Mins.TryGetValue(key, out var history5Min))
                {
                    _updateLastBar(history1Min, history5Min, TimeframeHelper.MIN5_GRANULARITY);
                }

                if (_15Mins.TryGetValue(key, out var history15Min))
                {
                    _updateLastBar(history1Min, history15Min, TimeframeHelper.MIN15_GRANULARITY);
                }

                if (_30Mins.TryGetValue(key, out var history30Min))
                {
                    _updateLastBar(history1Min, history30Min, TimeframeHelper.MIN30_GRANULARITY);
                }

                if (_1Hour.TryGetValue(key, out var history1H))
                {
                    _updateLastBar(history1Min, history1H, TimeframeHelper.HOURLY_GRANULARITY);
                }

                if (_4Hour.TryGetValue(key, out var history4H))
                {
                    _updateLastBar(history1Min, history4H, TimeframeHelper.HOUR4_GRANULARITY);
                }

                if (_1Day.TryGetValue(key, out var history1D))
                {
                    _updateLastBar(history1Min, history1D, TimeframeHelper.DAILY_GRANULARITY);
                }
            }
        }

        public string GetKey(HistoryData data)
        {
            if (data == null) return string.Empty;

            return data.Symbol + data.Exchange;
        }

        protected void _updateLastBar(HistoryData hLow, HistoryData hHigh, int granularity)
        {
            var lastHBar = hHigh.Bars.LastOrDefault();
            if (lastHBar == null)
            {
                return;
            }
            var bars = hLow.Bars.Where((_) => _.Timestamp > lastHBar.Timestamp);
            var count = bars.Count();
            foreach (var bar in bars)
            {
                if (bar.Timestamp >= lastHBar.Timestamp + granularity)
                {
                    hHigh.Bars.Add(new BarMessage
                    {
                        Open = bar.Open,
                        High = bar.High,
                        Low = bar.Low,
                        Close = bar.Close,
                        Timestamp = bar.Timestamp,
                        Volume = bar.Volume
                    });
                    lastHBar = hHigh.Bars.LastOrDefault();
                }
                else
                {
                    lastHBar.High = Math.Max(lastHBar.High, bar.High);
                    lastHBar.Low = Math.Min(lastHBar.Low, bar.Low);
                    lastHBar.Close = bar.Close;
                }
            }
            // var length = hLow.Bars.Count();
            // if (length < 2)
            // {
            //     return;
            // }

            // var lastL = hLow.Bars.LastOrDefault();
            // var preL = hLow.Bars.ElementAt(length - 1);
            // var lastH = hHigh.Bars.LastOrDefault();

            // if (lastH == null || lastL == null)
            // {
            //     return;
            // }

            // lastH.Close = lastL.Close;
            // lastH.High = Math.Max(lastL.High, lastH.High);
            // lastH.Low = Math.Min(lastL.Low, lastH.Low);

            // if (lastH.Timestamp < preL.Timestamp)
            // {
            //     lastH.High = Math.Max(preL.High, lastH.High);
            //     lastH.Low = Math.Min(preL.Low, lastH.Low);
            // }
        }

        protected bool _isSameInstrument(HistoryData h1, HistoryData h2)
        {
            return String.Equals(h1.Exchange, h2.Exchange) || String.Equals(h1.Symbol, h2.Symbol);
        }

        protected List<BarMessage> CombineHistory(IEnumerable<BarMessage> bars, int mins)
        {
            var res = new List<BarMessage>();
            foreach (var bar in bars)
            {
                if (bar.Timestamp % (mins * 60) == 0 || !res.Any())
                {
                    res.Add(new BarMessage
                    {
                        Open = bar.Open,
                        High = bar.High,
                        Low = bar.Low,
                        Close = bar.Close,
                        Volume = bar.Volume,
                        Timestamp = bar.Timestamp
                    });
                }
                else
                {
                    var last = res.LastOrDefault();
                    last.Close = bar.Close;
                    last.Low = Math.Min(bar.Low, last.Low);
                    last.High = Math.Max(bar.High, last.High);
                    last.Volume += bar.Volume;
                }
            }
            return res;
        }

        protected void tryAddHistoryInCache(HistoryData history1MinLongData)
        {
            var hash = history1MinLongData.Datafeed + "_" + history1MinLongData.Symbol + "_" + history1MinLongData.Granularity.ToString();
            try
            {
                _cache.Set(_cachePrefix, hash.ToLower(), history1MinLongData.Bars.TakeLast(longMinHistoryCount).ToList(), TimeSpan.FromDays(2));
            }
            catch (Exception e)
            {
            }
        }

        public abstract List<IInstrument> getInstruments();
        public abstract List<IInstrument> getInstrumentsForLongHistory();
    }
}
