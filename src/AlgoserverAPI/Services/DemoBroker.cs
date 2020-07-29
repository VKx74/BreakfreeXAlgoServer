using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.Broker;

namespace Algoserver.API.Services
{
    public class DemoBroker
    {

        private decimal _price = decimal.Zero;
        private long _time;
        private long _idCounter = 100;
        private readonly List<Order> _orders = new List<Order>();

        public List<Order> Orders
        {
            get
            {
                return this._orders.ToList();
            }
        }

        public void UpdatePrice(decimal price, long timestamp)
        {
            _price = price;
            _time = timestamp;
            _updateOrders();
        }

        public Order PlaceOrder(PlaceOrderRequest request)
        {
            if (_price == decimal.Zero)
            {
                throw new ApplicationException("Price not exist");
            }

            _idCounter++;

            var order = new Order
            {
                id = _idCounter.ToString(),
                current_price = _price,
                open_timestamp = _time,
                qty = request.qty,
                side = request.side,
                type = request.type,
                sl_price = request.sl_price,
                tp_price = request.tp_price
            };

            if (request.type == OrderType.Market)
            {
                order.price = _price;
                _setFilledState(order);
            }
            else
            {
                if (!request.price.HasValue)
                {
                    throw new ApplicationException("Invalid order Price parameter");
                }
                order.price = request.price.Value;
                order.status = OrderStatus.Open;
            }

            order.tp_price = request.tp_price.Value;
            order.sl_price = request.sl_price.Value;

            _validateOrderParameters(order);

            lock (_orders)
            {
                _orders.Add(order);
            }

            return order;
        }

        public Order EditOrder(EditOrderRequest request)
        {

            Order order = null;
            lock (_orders)
            {
                order = _orders.FirstOrDefault(_ => _.id == request.id);
            }

            if (order == null)
            {
                throw new ApplicationException("Order not found");
            }

            if (order.status == OrderStatus.Canceled || order.status == OrderStatus.Closed)
            {
                throw new ApplicationException("Order can`t be modified");
            }

            if (order.status == OrderStatus.Filled && request.price.HasValue)
            {
                throw new ApplicationException("Can`t modify filled order price");
            }

            if (order.status == OrderStatus.Open && request.price.HasValue)
            {
                _validateEntryPrice(order.type, order.side, request.price.Value);
                order.price = request.price.Value;
            }

            if (request.sl_price.HasValue)
            {
                if (order.type == OrderType.Market)
                {
                    _validateSLPrice(order.type, order.side, order.sl_price.Value, order.current_price);
                }
                else
                {
                    _validateSLPrice(order.type, order.side, order.sl_price.Value, order.price);
                }
                order.sl_price = request.sl_price.Value;
            }
            else
            {
                order.sl_price = request.sl_price.Value;
            }

            if (request.tp_price.HasValue)
            {
                if (order.type == OrderType.Market)
                {
                    _validateTPPrice(order.type, order.side, order.tp_price.Value, order.current_price);
                }
                else
                {
                    _validateTPPrice(order.type, order.side, order.tp_price.Value, order.price);
                }
                order.tp_price = request.tp_price.Value;
            }
            else
            {
                order.tp_price = request.tp_price.Value;
            }

            order.update_timestamp = _time;

            return order;

        }

        public Order CloseOrder(string id)
        {
            Order order = null;
            lock (_orders)
            {
                order = _orders.FirstOrDefault(_ => _.id == id);
            }

            if (order == null)
            {
                throw new ApplicationException("Order not found");
            }

            if (order.type != OrderType.Market)
            {
                throw new ApplicationException("Order type not valid");
            }

            order.close_timestamp = _time;
            order.status = OrderStatus.Closed;

            if (order.side == OrderSide.Buy)
            {
                order.pl = order.current_price - order.price;
            }
            else 
            {
                order.pl = order.price - order.current_price;
            }

            return order;
        }

        public Order CancelOrder(string id)
        {
            Order order = null;
            lock (_orders)
            {
                order = _orders.FirstOrDefault(_ => _.id == id);
            }

            if (order == null)
            {
                throw new ApplicationException("Order not found");
            }

            if (order.type == OrderType.Market)
            {
                throw new ApplicationException("Order type not valid");
            }

            order.cancel_timestamp = _time;
            order.status = OrderStatus.Canceled;

            return order;
        }

        public List<Order> GetOrders(OrderStatus status)
        {
            lock (_orders)
            {
                return _orders.Where(_ => _.status == status).ToList();
            }
        }

        private void _validateSLPrice(OrderType type, OrderSide side, decimal sl_price, decimal price)
        {
            if (side == OrderSide.Buy)
            {
                if (sl_price >= price)
                {
                    throw new ApplicationException("Invalid order SL parameter");
                }
            }
            else
            {
                if (sl_price <= price)
                {
                    throw new ApplicationException("Invalid order SL parameter");
                }
            }
        }

        private void _validateTPPrice(OrderType type, OrderSide side, decimal tp_price, decimal price)
        {
            if (side == OrderSide.Buy)
            {
                if (tp_price <= price)
                {
                    throw new ApplicationException("Invalid order TP parameter");
                }
            }
            else
            {
                if (tp_price >= price)
                {
                    throw new ApplicationException("Invalid order TP parameter");
                }
            }
        }

        private void _validateEntryPrice(OrderType type, OrderSide side, decimal price)
        {
            if (type == OrderType.Limit)
            {
                if (side == OrderSide.Buy && price >= _price)
                {
                    throw new ApplicationException("Invalid order Price parameter");
                }
                if (side == OrderSide.Sell && price <= _price)
                {
                    throw new ApplicationException("Invalid order Price parameter");
                }
            }

            if (type == OrderType.Stop)
            {
                if (side == OrderSide.Buy && price <= _price)
                {
                    throw new ApplicationException("Invalid order Price parameter");
                }
                if (side == OrderSide.Sell && price >= _price)
                {
                    throw new ApplicationException("Invalid order Price parameter");
                }
            }
        }
        private void _validateOrderParameters(Order order)
        {
            _validateEntryPrice(order.type, order.side, order.price);

            if (order.sl_price.HasValue)
            {
                if (order.type == OrderType.Market)
                {
                    _validateSLPrice(order.type, order.side, order.sl_price.Value, order.current_price);
                }
                else
                {
                    _validateSLPrice(order.type, order.side, order.sl_price.Value, order.price);
                }
            }

            if (order.tp_price.HasValue)
            {
                if (order.type == OrderType.Market)
                {
                    _validateTPPrice(order.type, order.side, order.tp_price.Value, order.current_price);
                }
                else
                {
                    _validateTPPrice(order.type, order.side, order.tp_price.Value, order.price);
                }
            }
        }

        private void _updateOrders()
        {
            foreach (var order in Orders)
            {
                if (order.status != OrderStatus.Filled && order.status != OrderStatus.Open) 
                {
                    continue;
                }
                order.current_price = _price;

                if (order.type != OrderType.Market)
                {
                    _updateLimitOrder(order);

                    // limit become market
                    if (order.type == OrderType.Market)
                    {
                        _updatemarketOrder(order);
                    }
                }
                else
                {
                    _updatemarketOrder(order);
                }
            }
        }

        private void _updatemarketOrder(Order order)
        {
            if (order.type == OrderType.Market)
            {
                if (order.side == OrderSide.Buy)
                {
                    order.pl = order.current_price - order.price;
                    if (order.tp_price.HasValue && order.current_price >= order.tp_price)
                    {
                        order.current_price = order.tp_price.Value;
                        CloseOrder(order.id);
                    }

                    if (order.sl_price.HasValue && order.current_price <= order.sl_price)
                    {
                        order.current_price = order.sl_price.Value;
                        CloseOrder(order.id);
                    }
                }
                else
                {
                    order.pl = order.price - order.current_price;
                    if (order.tp_price.HasValue && order.current_price <= order.tp_price)
                    {
                        order.current_price = order.tp_price.Value;
                        CloseOrder(order.id);
                    }

                    if (order.sl_price.HasValue && order.current_price >= order.sl_price)
                    {
                        order.current_price = order.sl_price.Value;
                        CloseOrder(order.id);
                    }
                }
            }
        }

        private void _setFilledState(Order order) 
        {
            order.type = OrderType.Market;
            order.status = OrderStatus.Filled;
            order.fill_timestamp = _time;
        }
        private void _updateLimitOrder(Order order)
        {
            if (order.type == OrderType.Limit)
            {
                if (order.side == OrderSide.Buy)
                {
                    if (order.current_price <= order.price)
                    {
                        _setFilledState(order);
                    }
                }
                else
                {
                    if (order.current_price >= order.price)
                    {
                        _setFilledState(order);
                    }
                }
            }

            if (order.type == OrderType.Stop)
            {
                if (order.side == OrderSide.Buy)
                {
                    if (order.current_price >= order.price)
                    {
                        _setFilledState(order);
                    }
                }
                else
                {
                    if (order.current_price <= order.price)
                    {
                        _setFilledState(order);
                    }
                }
            }
        }
    }
}
