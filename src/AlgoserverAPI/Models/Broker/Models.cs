using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Models.REST;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Algoserver.API.Models.Broker
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderType
    {
        Market,
        Limit,
        Stop
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderStatus
    {
        Open,
        Filled,
        Closed,
        Canceled
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderSide
    {
        Buy,
        Sell
    }

    public class Order
    {
        public string id { get; set; }
        public long open_timestamp { get; set; }
        public long? fill_timestamp { get; set; }
        public long? cancel_timestamp { get; set; }
        public long? close_timestamp { get; set; }
        public long? update_timestamp { get; set; }
        public OrderType type { get; set; }
        public OrderSide side { get; set; }
        public OrderStatus status { get; set; }
        public decimal qty { get; set; }
        public decimal current_price { get; set; }
        public decimal price { get; set; }
        public decimal? tp_price { get; set; }
        public decimal? sl_price { get; set; }
        public decimal? pl { get; set; }
    }

    public class PlaceOrderRequest
    {
        public OrderType type { get; set; }
        public OrderSide side { get; set; }
        public decimal qty { get; set; }
        public decimal? price { get; set; } // for stop and limit orders
        public decimal? tp_price { get; set; }
        public decimal? sl_price { get; set; }
    }

    public class EditOrderRequest
    {
        public string id { get; set; }
        public decimal? price { get; set; } // for stop and limit orders
        public decimal? tp_price { get; set; }
        public decimal? sl_price { get; set; }
    }
}