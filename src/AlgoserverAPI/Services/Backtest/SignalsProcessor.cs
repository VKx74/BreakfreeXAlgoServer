using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.Broker;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{

    public class SignalsProcessor: SignalProcessorBase
    {
        protected readonly IEnumerable<BacktestSignal> _signals;
        protected readonly int _breakevenCandles;

        public SignalsProcessor(IEnumerable<BarMessage> history, IEnumerable<BacktestSignal> signals, int breakevenCandles): base(history)
        {
            _signals = signals;
            _breakevenCandles = breakevenCandles;
        }

        public List<Order> Backtest(int entryCount = 3)
        {
            BacktestSignal prevSignal = null;
            Order order1 = null;
            Order order2 = null;
            Order order3 = null;
            var signals = new Queue<BacktestSignal>();

            foreach (var signal in _signals)
            {
                signals.Enqueue(signal);
            }

            var processedBars = new List<BarMessage>();

            foreach (var bar in _history)
            {
                _demoBroker.UpdatePrice(bar.High, bar.Timestamp);
                _demoBroker.UpdatePrice(bar.Low, bar.Timestamp);
                _demoBroker.UpdatePrice(bar.Close, bar.Timestamp);
                processedBars.Add(bar);

                if (processedBars.Count >= _breakevenCandles && _breakevenCandles > 1)
                {
                    var lastCandles = processedBars.TakeLast(_breakevenCandles);
                    _validateBreakevenCandles(lastCandles);
                }

                if (prevSignal != null && prevSignal.end_timestamp > 0 && prevSignal.end_timestamp <= bar.Timestamp)
                {
                    _cancelOrders();
                    prevSignal = null;
                }

                BacktestSignal signal;

                if (!signals.TryPeek(out signal))
                {
                    continue;
                }

                if (signal == null || signal.timestamp != bar.Timestamp)
                {
                    continue;
                }

                signals.Dequeue();

                _cancelOrders();

                prevSignal = signal;

                var side = signal.data.trade.AlgoEntry > signal.data.trade.AlgoStop ? OrderSide.Buy : OrderSide.Sell;

                var placeOrderRequest1 = new PlaceOrderRequest
                {
                    qty = 1,
                    price = signal.data.trade.AlgoEntryLow,
                    side = side,
                    type = OrderType.Limit,
                    sl_price = signal.data.trade.AlgoStop,
                    tp_price = signal.data.trade.AlgoTP1Low
                };

                var placeOrderRequest2 = new PlaceOrderRequest
                {
                    qty = 1,
                    price = signal.data.trade.AlgoEntry,
                    side = side,
                    type = OrderType.Limit,
                    sl_price = signal.data.trade.AlgoStop,
                    tp_price = signal.data.trade.AlgoTP1High
                };

                var placeOrderRequest3 = new PlaceOrderRequest
                {
                    qty = 1,
                    price = signal.data.trade.AlgoEntryHigh,
                    side = side,
                    type = OrderType.Limit,
                    sl_price = signal.data.trade.AlgoStop,
                    tp_price = signal.data.trade.AlgoTP2
                };

                try
                {
                    if (entryCount >= 1)
                    {
                        order1 = _demoBroker.PlaceOrder(placeOrderRequest1);
                    }
                    if (entryCount >= 2)
                    {
                        order2 = _demoBroker.PlaceOrder(placeOrderRequest2);
                    }
                    if (entryCount >= 3)
                    {
                        order3 = _demoBroker.PlaceOrder(placeOrderRequest3);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return _demoBroker.Orders;
        }

        private BacktestSignal _getSignalByDate(long timestamp)
        {
            foreach (var signal in this._signals)
            {
                if (signal.timestamp == timestamp)
                {
                    return signal;
                }
                if (signal.timestamp > timestamp)
                {
                    return null;
                }
            }

            return null;
        }
    }
}
