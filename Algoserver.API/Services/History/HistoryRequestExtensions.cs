using System.Collections.Generic;
using System.Net;
using Algoserver.API.Exceptions;

namespace Algoserver.API.Services.History
{
    public static class HistoryRequestExtensions
    {
        private static Dictionary<long, string> _granularityToInterval = new Dictionary<long, string>
        {
            { 60, "1min" },
            { 300, "5min" },
            { 900, "15min" },
            { 1800, "30min"},
            { 2700, "45min" },
            { 3600, "1h" },
            { 7200, "2h" },
            { 14400, "4h" },
            { 86400, "1day" },
            { 604800, "1week" },
            { 2629746, "1month" }
        };

        public static string GetInterval(this long granularity)
        {
            if (_granularityToInterval.TryGetValue(granularity, out string interval))
                return interval;
            else
                throw new RestException(HttpStatusCode.BadRequest, $"Can't create timeframe from granularity {granularity}");
        }
    }
}
