using System;
using System.Collections.Generic;
using Algoserver.API.Models.Broker;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services
{
    public abstract class SignalProcessorBase 
    {
        protected readonly IEnumerable<BarMessage> _history;
        protected readonly DemoBroker _demoBroker;

        public SignalProcessorBase(IEnumerable<BarMessage> history) 
        {
            _history = history;
            _demoBroker = new DemoBroker();
        }

        protected void _cancelOrders()
        {
            var openOrders = _demoBroker.GetOrders(OrderStatus.Open);
            foreach (var openOrder in openOrders)
            {
                _demoBroker.CancelOrder(openOrder.id);
            }
        }

        protected void _validateBreakevenCandles(IEnumerable<BarMessage> barsBack)
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
                            sl_price = filledOrder.sl_price
                        });
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        protected bool _validateBreakevenCandlesOrder(Order order, IEnumerable<BarMessage> barsBack)
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
