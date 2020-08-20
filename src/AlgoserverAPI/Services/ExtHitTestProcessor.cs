using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.Algo;
using Algoserver.API.Models.Broker;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public class ExtHitTestProcessor
    {
        private readonly IEnumerable<BarMessage> _history;
        private readonly IEnumerable<ExtHitTestSignal> _signals;
        private readonly int _breakevenCandles;
        private readonly int _entryTargetBox;
        private readonly int _stoplossRR;

        public ExtHitTestProcessor(IEnumerable<BarMessage> history, IEnumerable<ExtHitTestSignal> signals, int breakevenCandles, int entryTargetBox, int stoplossRR)
        {
            _history = history;
            _signals = signals;

            _breakevenCandles = breakevenCandles;
            _entryTargetBox = entryTargetBox;
            _stoplossRR = stoplossRR;
        }

        public void HitTest()
        {
            var signals = new Queue<ExtHitTestSignal>();
            foreach (var signal in _signals)
            {
                signals.Enqueue(signal);
            }

            var signalsToProcess = new List<ExtHitTestSignal>();
            var processedBars = new List<BarMessage>();

            foreach (var bar in _history)
            {
                var high = bar.High;
                var low = bar.Low;
                var timestamp = bar.Timestamp;
                IEnumerable<BarMessage> lastCandles = null;
                processedBars.Add(bar);

                if (processedBars.Count >= _breakevenCandles && _breakevenCandles > 1)
                {
                    lastCandles = processedBars.TakeLast(_breakevenCandles);
                }

                foreach (var signal in signalsToProcess)
                {
                    if (signal.wentout || signal.backhit || timestamp < signal.timestamp || signal.breakeven)
                    {
                        continue;
                    }

                    var topExt1 = signal.data.P18;
                    var topExt2 = signal.data.P28;
                    var resistance = signal.data.EE;

                    if (signal.top_sl == 0)
                    {
                        var shift = Math.Abs(topExt2 - topExt1) * (_stoplossRR / 100m);
                        signal.top_sl = topExt2 + shift;
                    }

                    if (signal.top_entry == 0) 
                    {
                        var shift = Math.Abs(topExt2 - topExt1) * (_entryTargetBox / 100m);
                        signal.top_entry = topExt1 - shift;
                    }

                    var support = signal.data.ZE;
                    var bottomExt1 = signal.data.M18;
                    var bottomExt2 = signal.data.M28;

                    if (signal.bottom_sl == 0)
                    {
                        var shift = Math.Abs(bottomExt1 - bottomExt2) * (_stoplossRR / 100m);
                        signal.bottom_sl = bottomExt2 - shift;
                    }

                    if (signal.bottom_entry == 0) 
                    {
                        var shift = Math.Abs(bottomExt1 - bottomExt2) * (_entryTargetBox / 100m);
                        signal.bottom_entry = bottomExt1 + shift;
                    }

                    if (!signal.breakeven && lastCandles != null && lastCandles.Any())
                    {
                        if (signal.bottomext1hit || signal.topext1hit) {
                            if (!_validateSignal(signal, lastCandles)) 
                            {
                                signal.breakeven = true;
                                continue;
                            }
                        }
                    }

                    if (!signal.backhit)
                    {
                        if (signal.topext1hit)
                        {
                            if (low <= resistance)
                            {
                                signal.backhit = true;
                            }
                            else if (high > signal.top_sl)
                            {
                                signal.wentout = true;
                            }
                        }
                        else if (signal.bottomext1hit)
                        {
                            if (high >= support)
                            {
                                signal.backhit = true;
                            }
                            else if (low < signal.bottom_sl)
                            {
                                signal.wentout = true;
                            }
                        }
                    }

                    if (signal.end_timestamp > timestamp)
                    {
                        if (high >= signal.top_entry && !signal.topext1hit && signal.trend == Trend.Down)
                        {
                            signal.topext1hit = true;
                        }

                        if (low <= signal.bottom_entry && !signal.bottomext1hit && signal.trend == Trend.Up)
                        {
                            signal.bottomext1hit = true;
                        }
                    }
                }

                ExtHitTestSignal nextSignal;
                if (signals.TryPeek(out nextSignal))
                {
                    if (nextSignal != null && nextSignal.timestamp == bar.Timestamp)
                    {
                        signals.Dequeue();

                        foreach (var signal in signalsToProcess.ToList())
                        {
                            if (signal.wentout || signal.backhit)
                            {
                                signalsToProcess.Remove(signal);
                            }
                            else if (!signal.topext1hit && !signal.bottomext1hit)
                            {
                                signalsToProcess.Remove(signal);
                            }
                        }

                        signalsToProcess.Add(nextSignal);
                    }

                }
            }
        }

        private bool _validateSignal(ExtHitTestSignal signal, IEnumerable<BarMessage> barsBack)
        {
            var prices = new List<decimal>();
            var bottomExt1 = signal.data.M18;
            var topExt1 = signal.data.P18;

            foreach (var bar in barsBack)
            {
                if (signal.bottomext1hit)
                {
                    prices.Add(bar.Low);
                }
                else if (signal.topext1hit)
                {
                    prices.Add(bar.High);
                }
            }

            foreach (var price in prices)
            {
                if (signal.bottomext1hit)
                {
                    if (price >= bottomExt1) 
                    {
                        return true;
                    }
                }
                else
                {
                    if (price <= topExt1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
