using System;

namespace Algoserver.Client.REST.Response
{
    public class TimeSeries
    {
        public DateTimeOffset Datetime { get; set; }

        public decimal Open { get; set; }

        public decimal High { get; set; }
        public decimal Low { get; set; }

        public decimal Close { get; set; }

        public decimal Volume { get; set; }
    }

    public class TimeSeriesResponse
    {
        public TimeSeries[] Values { get; set; }
    }
}
