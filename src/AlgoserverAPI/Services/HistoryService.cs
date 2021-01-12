using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.Algo;
using Algoserver.API.Models.REST;
using Algoserver.Auth.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Algoserver.API.Services
{
    // Provide historical data from oanda or twelvedata or kaiko datafeeds
    public class HistoryService
    {
        private const int BARS_COUNT = 500;
        private const int ONE_DAY_TIME_SHIFT = 60 * 60 * 24;
        private readonly HttpClient _httpClient;
        private readonly ILogger<HistoryService> _logger;
        private readonly AuthService _auth;
        private readonly string _serverUrl;
        private readonly ICacheService _cache;
        private string _cachePrefix = "History_";
        private readonly Dictionary<string, Task<HistoryData>> _requestCache = new Dictionary<string, Task<HistoryData>>();

        public HistoryService(ILogger<HistoryService> logger, IConfiguration configuration, ICacheService cache, AuthService auth)
        {
            _logger = logger;
            _cache = cache;
            _auth = auth;
            _serverUrl = configuration["DatafeedEndpoint"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<HistoryData> GetHistoryWithCount(string symbol, int granularity, string datafeed, string exchange, int count, int repeatCount = 2)
        {
            var result = await LoadHistoricalData(datafeed, symbol, granularity, count, exchange, repeatCount);
            return result;
        }
        public async Task<HistoryData> GetHistory(string symbol, int granularity, string datafeed, string exchange, string type, int replayBack = 0)
        {
            var hash = getHash(symbol, granularity, datafeed, exchange);

            try
            {
                if (_cache.TryGetValue(_cachePrefix, hash, out HistoryData cachedResponse))
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

            // var exists = _contextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader);
            // var bearerString = authHeader.ToString();

            var result = await LoadHistoricalData(datafeed, symbol, granularity, barsBack, exchange);

            try
            {
                if (result != null && result != null && result.Bars != null && result.Bars.Any())
                {
                    if (granularity > 60 * 15)
                    {
                        _cache.Set(_cachePrefix, hash, result, TimeSpan.FromMinutes(5));
                    }
                    else if (granularity > 60)
                    {
                        _cache.Set(_cachePrefix, hash, result, TimeSpan.FromMinutes(3));
                    }
                    else
                    {
                        _cache.Set(_cachePrefix, hash, result, TimeSpan.FromMinutes(1));
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

        private async Task<HistoryData> SendHistoricalRequest(string datafeed, string symbol, int granularity, long bars_count, string exchange, int repeatCount)
        {
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

                if (!string.IsNullOrWhiteSpace(exchange))
                {
                    uri = $"{uri}&exchange={exchange}";
                }

                Console.WriteLine(uri);

                var token = await _auth.GetToken();
                var request = new HttpRequestMessage(HttpMethod.Get, uri)
                {
                    Headers = {
                    { HttpRequestHeader.Authorization.ToString(), token }
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

                if (prevCount == afterCount)
                {
                    return result;
                }

                if (requestCount > 0) {
                    Console.WriteLine($"History request count {requestCount} - {uri}");
                }

                if (requestCount++ > repeatCount)
                {
                    return result;
                }

            } while (result.Bars.Count() < bars_count);

            return result;
        }

        private async Task<HistoryData> LoadHistoricalData(string datafeed, string symbol, int granularity, long bars_count, string exchange, int repeatCount = 5)
        {
            var hash = GetHistoricalRequestHash(datafeed, symbol, granularity, bars_count, exchange, repeatCount);
            HistoryData returnResult = null;
            try
            {
                Task<HistoryData> res;
                lock (_requestCache)
                {
                    if (!_requestCache.TryGetValue(hash, out res))
                    {
                        res = SendHistoricalRequest(datafeed, symbol, granularity, bars_count, exchange, repeatCount);
                        _requestCache.TryAdd(hash, res);
                    }
                }
                returnResult = await res;
            }
            catch (Exception ex)
            {
            }
            finally
            {
                lock (_requestCache)
                {
                    _requestCache.Remove(hash);
                }
            }

            return returnResult;
        }

        private string GetHistoricalRequestHash(string datafeed, string symbol, int granularity, long bars_count, string exchange, int repeatCount)
        {
            return datafeed + symbol + granularity.ToString() + bars_count + exchange + repeatCount.ToString();
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
            var mult = 3;

            if (data.Granularity < 86400 && data.Datafeed == "Twelvedata")
            {
                mult = 10;
            }

            long startDate = endDate - ((bars_count - existing_count) * data.Granularity * mult);

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
            if (!String.IsNullOrEmpty(symbol))
            {
                symbol = symbol.ToLowerInvariant();
            }
            if (!String.IsNullOrEmpty(datafeed))
            {
                datafeed = datafeed.ToLowerInvariant();
            }
            if (!String.IsNullOrEmpty(exchange))
            {
                exchange = exchange.ToLowerInvariant();
            }
            return $"{symbol}{granularity}{datafeed}{exchange}";
        }
    }
}
