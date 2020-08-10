﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.Algo;
using Algoserver.API.Models.REST;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Algoserver.API.Services
{
    // Provide historical data from oanda or twelvedata datafeeds
    public class HistoryService
    {
        private const int BARS_COUNT = 600;
        private const int ONE_DAY_TIME_SHIFT = 60 * 60 * 24;

        private readonly HttpClient _httpClient;
        private readonly ILogger<HistoryService> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string _serverUrl;
        private readonly IMemoryCache _cache;

        public HistoryService(ILogger<HistoryService> logger, IConfiguration configuration, IMemoryCache cache, IHttpContextAccessor contextAccessor)
        {
            _logger = logger;
            _cache = cache;
            _contextAccessor = contextAccessor;
            _serverUrl = configuration["DatafeedEndpoint"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HistoryData> GetHistory(string symbol, int granularity, string datafeed, string exchange, string type, int replayBack = 0)
        {
            var hash = getHash(symbol, granularity, datafeed, exchange);

            try
            {
                if (_cache.TryGetValue(hash, out HistoryData cachedResponse))
                {
                    if (cachedResponse.Bars != null && cachedResponse.Bars.Count() - replayBack >= InputDataContainer.MIN_BARS_COUNT)
                    {
                        return cachedResponse;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get cached response");
                _logger.LogError(e.Message);
            }

            var barsBack = BARS_COUNT + replayBack;

            var result = await LoadHistoricalData(datafeed, symbol, granularity, barsBack, exchange);

            try
            {
                if (result != null && result != null && result.Bars != null && result.Bars.Any())
                {
                    if (granularity > 60)
                    {
                        _cache.Set(hash, result, TimeSpan.FromMinutes(3));
                    }
                    else
                    {
                        _cache.Set(hash, result, TimeSpan.FromMinutes(1));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to set cached response");
                _logger.LogError(e.Message);
            }

            return result;
        }

        private async Task<HistoryData> LoadHistoricalData(string datafeed, string symbol, int granularity, long bars_count, string exchange = "")
        {
            var exists = _contextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader);
            var bearerString = authHeader.ToString();
            var bearerdatafeed = datafeed.ToLowerInvariant();

            var result = new HistoryData
            {
                Bars = new List<BarMessage>(),
                Datafeed = datafeed,
                Exchange = exchange,
                Granularity = granularity,
                Symbol = symbol
            };

            var requestCount = 0;

            do
            {
                var endDate = this.getEndDate(result);
                var startDate = this.getStartDate(result, bars_count);

                var uri = $"{_serverUrl}/{bearerdatafeed}/history?" +
                          $"kind=daterange&symbol={symbol}&granularity={granularity}&from={startDate}&to={endDate}";

                if (string.IsNullOrWhiteSpace(exchange))
                {
                    uri = $"{uri}&exchange={exchange}";
                }

                var request = new HttpRequestMessage(HttpMethod.Get, uri)
                {
                    Headers = {
                    { HttpRequestHeader.Authorization.ToString(), bearerString }
                }
                };

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error occured while sending http request to {request.RequestUri}, response string: {content}");
                    response.EnsureSuccessStatusCode();
                }

                IEnumerable<BarMessage> bars = null;
                if (bearerdatafeed == "oanda")
                {
                    var oandaResponse = JsonConvert.DeserializeObject<OandaHistoryResponse>(content);
                    bars = oandaResponse.Data.Bars;
                }
                else
                {
                    var historyResponse = JsonConvert.DeserializeObject<HistoryData>(content);
                    bars = historyResponse.Bars;
                }

                var prevCount = result.Bars.Count();
                result.Bars = this.mergeBars(result.Bars, bars);
                var afterCount = result.Bars.Count();

                if (prevCount == afterCount) {
                    return result;
                }

                if (requestCount++ > 10)
                {
                    return result;
                }

            } while (result.Bars.Count() < bars_count);

            return result;
        }

        private IEnumerable<BarMessage> mergeBars(IEnumerable<BarMessage> existingBars, IEnumerable<BarMessage> newBars)
        {
            var firstBar = existingBars.FirstOrDefault();

            if (firstBar == null)
            {
                return newBars;
            }

            var barsToAppend = newBars.Where(_ => _.Timestamp < firstBar.Timestamp).ToList();
            barsToAppend.AddRange(existingBars);
            return barsToAppend;
        }

        private long getEndDate(HistoryData data)
        {
            var firstBar = data.Bars.FirstOrDefault();

            if (firstBar == null)
            {
                return AlgoHelper.UnixTimeNow();
            }

            return firstBar.Timestamp + 1;
        }

        private long getStartDate(HistoryData data, long bars_count)
        {
            var existing_count = data.Bars.Count();
            var endDate = this.getEndDate(data);
            long startDate = endDate - ((bars_count - existing_count) * data.Granularity * 3);

            if (startDate < 0)
            {
                startDate = 0;
            }
            else
            {
                int day = new DateTime(startDate).Day;

                // load more minute data
                if (data.Granularity < 3600)
                {
                    startDate = startDate - ONE_DAY_TIME_SHIFT;
                }

                if (day == 0)
                {
                    // Sunday
                    startDate = startDate - (ONE_DAY_TIME_SHIFT * 2);
                }
                else if (day == 6)
                {
                    // Saturday
                    startDate = startDate - ONE_DAY_TIME_SHIFT;
                }
            }

            var count = (endDate - startDate) / data.Granularity;
            if (count > 5000)
            {
                startDate = endDate - (data.Granularity * 5000);
            }

            if (startDate < 0)
            {
                startDate = 0;
            }

            return startDate;
        }

        private string getHash(string symbol, int granularity, string datafeed, string exchange = "")
        {
            return $"{symbol}{granularity}{datafeed}{exchange}";
        }
    }
}
