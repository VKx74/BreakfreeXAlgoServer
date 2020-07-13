using System;

namespace Twelvedata.Client.REST.Requests
{
    public class GetTimeSeriesRequest
    {   
        [QueryParameter("exchange")]
        public string Exchange { get; set; }

        [QueryParameter("symbol")]
        public string Symbol { get; set; }

        [QueryParameter("interval")]
        public string Interval { get; set; }

        [QueryParameter("start_date")]
        public DateTime? From { get; set; }

        [QueryParameter("end_date")]
        public DateTime? To { get; set; }

        [QueryParameter("timezone")]
        public string Timezone { get; set; } = "UTC";

    }
}
