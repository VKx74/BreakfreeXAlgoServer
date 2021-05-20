using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.Broker;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public class SignalsV2Processor
    {
        private readonly IEnumerable<BarMessage> _history;
        private readonly IEnumerable<Strategy2BacktestSignal> _signals;
        private readonly int _breakevenCandles;
        private readonly DemoBroker _demoBroker;

        public SignalsV2Processor(IEnumerable<BarMessage> history, IEnumerable<Strategy2BacktestSignal> signals, int breakevenCandles)
        {
            _history = history;
            _signals = signals;
            _breakevenCandles = breakevenCandles;
            _demoBroker = new DemoBroker();
        }

        public List<Order> Backtest()
        {
            Strategy2BacktestSignal prevSignal = null;
            var signals = new Queue<Strategy2BacktestSignal>();

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

                Strategy2BacktestSignal signal;

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

                try
                {
                    if (signal.data.trade_sr != null)
                    {
                        var trade = signal.data.trade_sr;
                        var side1 = trade.is_buy ? OrderSide.Buy : OrderSide.Sell;
                        var placeOrderRequest = new PlaceOrderRequest
                        {
                            qty = 1,
                            price = trade.entry,
                            side = side1,
                            type = OrderType.Limit,
                            sl_price = trade.stop,
                            tp_price = trade.limit,
                            comment = "S/R order"
                        };
                        _demoBroker.PlaceOrder(placeOrderRequest);
                    }
                }
                catch (Exception ex)
                {

                }

                try
                {
                    if (signal.data.trade_ex1 != null)
                    {
                        var trade = signal.data.trade_ex1;
                        var side1 = trade.is_buy ? OrderSide.Buy : OrderSide.Sell;
                        var placeOrderRequest = new PlaceOrderRequest
                        {
                            qty = 1,
                            price = trade.entry,
                            side = side1,
                            type = OrderType.Limit,
                            sl_price = trade.stop,
                            tp_price = trade.limit,
                            comment = "Ext order"
                        };
                        _demoBroker.PlaceOrder(placeOrderRequest);
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
                if (!_validateBreakevenCandlesOrder(filledOrder, barsBack))
                {
                    try
                    {
                        _demoBroker.EditOrder(new EditOrderRequest
                        {
                            id = filledOrder.id,
                            tp_price = filledOrder.price,
                            sl_price = filledOrder.sl_price,
                            comment = "Breakeven candles condition hit"
                        });
                    }
                    catch (Exception ex)
                    {
                    }
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
    }
}
