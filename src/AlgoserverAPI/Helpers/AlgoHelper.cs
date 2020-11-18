using System;
using System.Net;
using Algoserver.API.Exceptions;
using Algoserver.API.Models.Algo;
using Algoserver.API.Models;
using System.Collections.Generic;

namespace Algoserver.API.Helpers
{
    public class AlgoHelper
    {
        public static int ConvertTimeframeToCranularity(int interval, string period)
        {
            switch (period)
            {
                case Periodicity.MINUTE: return 60 * interval;
                case Periodicity.HOUR: return 3600 * interval;
                case Periodicity.DAY: return 86400;
                case Periodicity.WEEK: return 604800;
                case Periodicity.MONTH: return 2629746;
                default: throw new ApiException(HttpStatusCode.BadRequest, $"Period '{period}' is not supported.");
            }
        }

        public static long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }

        public static decimal CalculatePositionValue(string type, string symbol, decimal accountSize, decimal suggestedRisk, decimal entry, decimal sl) {
            if (type == "forex" || type == "metals")
            {
                var contractSize = InstrumentsHelper.GetContractSize(symbol);
                return (((accountSize * (suggestedRisk / 100)) / Math.Abs(entry - sl))) / contractSize;
            }

            return 0;
            // return (accountSize * (suggestedRisk / 100)) / Math.Abs(entry - sl) / 100;
        }
    }
}
