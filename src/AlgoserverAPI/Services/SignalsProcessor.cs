using System;
using System.Collections.Generic;
using Algoserver.API.Models.Broker;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public class SignalsProcessor
    {
        private readonly IEnumerable<BarMessage> _history;
        private readonly IEnumerable<BacktestSignal> _signals;
        private readonly DemoBroker _demoBroker;

        public SignalsProcessor(IEnumerable<BarMessage> history, IEnumerable<BacktestSignal> signals)
        {
            _history = history;
            _signals = signals;
            _demoBroker = new DemoBroker();
        }

        public List<Order> Calculate()
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

            foreach (var bar in _history)
            {
                _demoBroker.UpdatePrice(bar.High, bar.Timestamp);
                _demoBroker.UpdatePrice(bar.Low, bar.Timestamp);
                _demoBroker.UpdatePrice(bar.Close, bar.Timestamp);

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

                var openOrders = _demoBroker.GetOrders(OrderStatus.Open);
                foreach (var openOrder in openOrders) 
                {
                    _demoBroker.CancelOrder(openOrder.id);
                }

                prevSignal = signal;

                var side = signal.data.AlgoEntry > signal.data.AlgoStop ? OrderSide.Buy : OrderSide.Sell;

                var placeOrderRequest1 = new PlaceOrderRequest {
                    qty = 1,
                    price = signal.data.AlgoEntryLow,
                    side = side,
                    type = OrderType.Limit,
                    sl_price = signal.data.AlgoStop,
                    tp_price = signal.data.AlgoTP1Low                    
                };
                
                var placeOrderRequest2 = new PlaceOrderRequest {
                    qty = 1,
                    price = signal.data.AlgoEntry,
                    side = side,
                    type = OrderType.Limit,
                    sl_price = signal.data.AlgoStop,
                    tp_price = signal.data.AlgoTP1High
                };
                
                var placeOrderRequest3 = new PlaceOrderRequest {
                    qty = 1,
                    price = signal.data.AlgoEntryHigh,
                    side = side,
                    type = OrderType.Limit,
                    sl_price = signal.data.AlgoStop,
                    tp_price = signal.data.AlgoTP2
                };

                try {
                    order1 = _demoBroker.PlaceOrder(placeOrderRequest1);
                    order2 = _demoBroker.PlaceOrder(placeOrderRequest2);
                    order3 = _demoBroker.PlaceOrder(placeOrderRequest3);
                } catch (Exception ex) {

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
