using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.Broker;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public class ExtHitTestProcessor
    {
        private readonly IEnumerable<BarMessage> _history;
        private readonly IEnumerable<ExtHitTestSignal> _signals;

        public ExtHitTestProcessor(IEnumerable<BarMessage> history, IEnumerable<ExtHitTestSignal> signals)
        {
            _history = history;
            _signals = signals;
        }

        public void HitTest()
        {
            var signals = new Queue<ExtHitTestSignal>();
            foreach (var signal in _signals)
            {
                signals.Enqueue(signal);
            }

            var signalsToProcess = new List<ExtHitTestSignal>();
            foreach (var bar in _history)
            {
                var high = bar.High;
                var low = bar.Low;
                var timestamp = bar.Timestamp;

                foreach (var signal in signalsToProcess)
                {
                    if (signal.wentout || signal.backhit || timestamp < signal.timestamp)
                    {
                        continue;
                    }

                    var topExt2 = signal.data.P28;
                    var topExt1 = signal.data.P18;
                    var resistance = signal.data.EE;
                    var topShiftPossible = Math.Abs(topExt2 - topExt1) * 0.25m;

                    var support = signal.data.ZE;
                    var bottomExt1 = signal.data.M18;
                    var bottomExt2 = signal.data.M28;
                    var bottomShiftPossible = Math.Abs(bottomExt1 - bottomExt2) * 0.25m;

                    if (!signal.backhit)
                    {
                        if (signal.topext1hit)
                        {
                            if (low <= resistance)
                            {
                                signal.backhit = true;
                            }
                            else if (high > topExt2 + topShiftPossible)
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
                            else if (low < bottomExt2 - bottomShiftPossible)
                            {
                                signal.wentout = true;
                            }
                        }
                    }

                    if (high >= topExt1 && !signal.topext1hit && !signal.is_up_tending)
                    {
                        signal.topext1hit = true;
                    }

                    if (low <= bottomExt1 && !signal.bottomext1hit && signal.is_up_tending)
                    {
                        signal.bottomext1hit = true;
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
                            if (signal.wentout || signal.backhit) {
                                signalsToProcess.Remove(signal);
                            } else if (!signal.topext1hit && !signal.bottomext1hit) {
                                signalsToProcess.Remove(signal);
                            }
                        }
                        
                        signalsToProcess.Add(nextSignal);
                    }

                }
            }
        }
    }
}
