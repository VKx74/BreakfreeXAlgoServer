using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.Broker;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public class SignalsProcessor
    {
        private readonly IEnumerable<BarMessage> _history;
        private readonly IEnumerable<BacktestSignal> _signals;
        private readonly int _breakevenCandles;
        private readonly DemoBroker _demoBroker;

        public SignalsProcessor(IEnumerable<BarMessage> history, IEnumerable<BacktestSignal> signals, int breakevenCandles)
        {
            _history = history;
            _signals = signals;
            _breakevenCandles = breakevenCandles;
            _demoBroker = new DemoBroker();
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
                    cancelOrders();
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

                cancelOrders();

                prevSignal = signal;

                var side = signal.data.AlgoEntry > signal.data.AlgoStop ? OrderSide.Buy : OrderSide.Sell;

                var placeOrderRequest1 = new PlaceOrderRequest
                {
                    qty = 1,
                    price = signal.data.AlgoEntryLow,
                    side = side,
                    type = OrderType.Limit,
                    sl_price = signal.data.AlgoStop,
                    tp_price = signal.data.AlgoTP1Low
                };

                var placeOrderRequest2 = new PlaceOrderRequest
                {
                    qty = 1,
                    price = signal.data.AlgoEntry,
                    side = side,
                    type = OrderType.Limit,
                    sl_price = signal.data.AlgoStop,
                    tp_price = signal.data.AlgoTP1High
                };

                var placeOrderRequest3 = new PlaceOrderRequest
                {
                    qty = 1,
                    price = signal.data.AlgoEntryHigh,
                    side = side,
                    type = OrderType.Limit,
                    sl_price = signal.data.AlgoStop,
                    tp_price = signal.data.AlgoTP2
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

        private void cancelOrders()
        {
            var openOrders = _demoBroker.GetOrders(OrderStatus.Open);
            foreach (var openOrder in openOrders)
            {
                _demoBroker.CancelOrder(openOrder.id);
            }
        }

        private void _validateBreakevenCandles(IEnumerable<BarMessage> barsBack)
        {
            var filledOrders = _demoBroker.GetOrders(OrderStatus.Filled);
            foreach (var filledOrder in filledOrders)
            {
                if (!_validateBreakevenCandlesOrder(filledOrder, barsBack)) {
                    _demoBroker.EditOrder(new EditOrderRequest {
                        id = filledOrder.id,
                        tp_price = filledOrder.price
                    });
                }
            }
        }

        private bool _validateBreakevenCandlesOrder(Order order, IEnumerable<BarMessage> barsBack)
        {
            var prices = new List<decimal>();
            foreach (var bar in barsBack)
            {
                if (order.side == OrderSide.Buy)
                {
                    prices.Add(bar.Low);
                }
                else
                {
                    prices.Add(bar.High);
                }
            }

            foreach (var price in prices)
            {
                if (order.side == OrderSide.Buy)
                {
                    if (price >= order.price) 
                    {
                        return true;
                    }
                }
                else
                {
                    if (price <= order.price) 
                    {
                        return true;
                    }
                }
            }

            return false;
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
