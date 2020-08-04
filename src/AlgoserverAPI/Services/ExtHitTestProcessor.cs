using System;
using System.Collections.Generic;
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

            foreach (var signal in _signals)
            {
                foreach (var bar in _history)
                {
                    var high = bar.High;
                    var low = bar.Low;
                    var timestamp = bar.Timestamp;
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
                        if (signal.topext1hit || signal.topext2hit)
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
                        else if (signal.bottomext1hit || signal.bottomext2hit)
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

                    if (high >= topExt1 && !signal.topext1hit)
                    {
                        signal.topext1hit = true;
                    }
                    if (high >= topExt2 && !signal.topext2hit)
                    {
                        signal.topext2hit = true;
                    }

                    if (low <= bottomExt1 && !signal.bottomext1hit)
                    {
                        signal.bottomext1hit = true;
                    }
                    if (low <= bottomExt2 && !signal.bottomext2hit)
                    {
                        signal.bottomext2hit = true;
                    }
                }
            }
        }
    }
}
