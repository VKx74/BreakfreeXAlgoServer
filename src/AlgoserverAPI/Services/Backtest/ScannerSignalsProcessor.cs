using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.Broker;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    internal class PlacedOrderData
    {
        public Order Order { get; set; }
        public int Index { get; set; }
    }

    public class ScannerSignalsProcessor : SignalProcessorBase
    {
        protected readonly IEnumerable<ScannerBacktestSignal> _signals;
        protected int _breakevenCandles;
        protected int _canceleationCandles;
        protected bool _singlePosition;

        public ScannerSignalsProcessor(IEnumerable<BarMessage> history, IEnumerable<ScannerBacktestSignal> signals) : base(history)
        {
            _signals = signals;
        }

        public List<Order> Backtest(int breakevenCandles, int canceleationCandles, bool singlePosition)
        {
            _breakevenCandles = breakevenCandles;
            _canceleationCandles = canceleationCandles;
            _singlePosition = singlePosition;

            var placedOrders = new List<PlacedOrderData>();

            ScannerBacktestSignal prevSignal = null;
            var signals = new Queue<ScannerBacktestSignal>();

            foreach (var signal in _signals)
            {
                signals.Enqueue(signal);
            }

            var processedBars = new List<BarMessage>();
            var index = 0;

            foreach (var bar in _history)
            {
                index++;

                _demoBroker.UpdatePrice(bar.High, bar.Timestamp);
                _demoBroker.UpdatePrice(bar.Low, bar.Timestamp);
                _demoBroker.UpdatePrice(bar.Close, bar.Timestamp);
                processedBars.Add(bar);

                if (processedBars.Count >= _breakevenCandles && _breakevenCandles > 1)
                {
                    var lastCandles = processedBars.TakeLast(_breakevenCandles);
                    _validateBreakevenCandles(lastCandles);
                }

                if (canceleationCandles > 1)
                {
                    var ordersToCancel = placedOrders.Where(_ => index - _.Index > canceleationCandles);
                    foreach (var orderToCancel in ordersToCancel)
                    {
                        if (orderToCancel.Order.status == OrderStatus.Open)
                        {
                            _demoBroker.CancelOrder(orderToCancel.Order.id);
                        }
                    }
                    placedOrders.RemoveAll(_ => index - _.Index > canceleationCandles);
                }
                
                ScannerBacktestSignal signal;

                if (!signals.TryPeek(out signal))
                {
                    continue;
                }

                if (signal == null || signal.timestamp != bar.Timestamp)
                {
                    continue;
                }

                if (this._isTradeEquals(signal, prevSignal))
                {
                    continue;
                }

                prevSignal = signal;

                signals.Dequeue();

                var filledOrders = _demoBroker.GetOrders(OrderStatus.Filled);
                if (_singlePosition && filledOrders.Any())
                {
                    continue;
                }

                _cancelOrders();

                var side = signal.data.trade.entry > signal.data.trade.stop ? OrderSide.Buy : OrderSide.Sell;

                var placeOrderRequest1 = new PlaceOrderRequest
                {
                    qty = 1,
                    price = signal.data.trade.entry,
                    side = side,
                    type = OrderType.Limit,
                    sl_price = signal.data.trade.stop,
                    tp_price = signal.data.trade.take_profit
                };

                var placeOrderRequest2 = new PlaceOrderRequest
                {
                    qty = 1,
                    price = signal.data.trade.entry_h,
                    side = side,
                    type = OrderType.Limit,
                    sl_price = signal.data.trade.stop,
                    tp_price = signal.data.trade.take_profit_h
                };

                var placeOrderRequest3 = new PlaceOrderRequest
                {
                    qty = 1,
                    price = signal.data.trade.entry_l,
                    side = side,
                    type = OrderType.Limit,
                    sl_price = signal.data.trade.stop,
                    tp_price = signal.data.trade.take_profit_l
                };

                try
                {
                    var order1 = _demoBroker.PlaceOrder(placeOrderRequest1);
                    var order2 = _demoBroker.PlaceOrder(placeOrderRequest2);
                    var order3 = _demoBroker.PlaceOrder(placeOrderRequest3);

                    placedOrders.Add(new PlacedOrderData
                    {
                        Order = order1,
                        Index = index
                    });
                    placedOrders.Add(new PlacedOrderData
                    {
                        Order = order2,
                        Index = index
                    });
                    placedOrders.Add(new PlacedOrderData
                    {
                        Order = order3,
                        Index = index
                    });
                }
                catch (Exception ex)
                {
                }
            }

            return _demoBroker.Orders;
        }

        private bool _isTradeEquals(ScannerBacktestSignal signal1, ScannerBacktestSignal signal2)
        {
            if (signal1 == null || signal2 == null)
            {
                return false;
            }

            var trade1 = signal1.data.trade;
            var trade2 = signal2.data.trade;

            return trade1.entry == trade2.entry && trade1.stop == trade2.stop;
        }

        private void _closeExpiredPandingOrders()
        {
            // var openOrders = _demoBroker.GetOrders(OrderStatus.Open);
            // openOrders
        }
    }
}
