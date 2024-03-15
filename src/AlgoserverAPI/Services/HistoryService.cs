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
using Algoserver.API.Services.CacheServices;
using Algoserver.Auth.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Algoserver.API.Services
{
    // Provide historical data from oanda or twelvedata or kaiko or binance datafeeds
    public class HistoryService
    {
        private const int ONE_DAY_TIME_SHIFT = 60 * 60 * 24;
        private readonly HttpClient _httpClient;
        private readonly ILogger<HistoryService> _logger;
        private readonly AuthService _auth;
        private readonly string _serverUrl;
        private readonly IInMemoryCache _cache;
        private string _cachePrefix = "History_";
        private readonly Dictionary<string, Task<HistoryData>> _requestCache = new Dictionary<string, Task<HistoryData>>();

        public HistoryService(ILogger<HistoryService> logger, IConfiguration configuration, IInMemoryCache cache, AuthService auth)
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
        public async Task<HistoryData> GetHistory(string symbol, int granularity, string datafeed, string exchange, string type, int replayBack = 0, int minBarsCount = 0)
        {
            var hash = getHash(symbol, granularity, datafeed, exchange);
            var barsBack = Math.Max(InputDataContainer.MIN_BARS_COUNT, minBarsCount) + replayBack;

            try
            {
                if (_cache.TryGetValue<HistoryData>(_cachePrefix, hash, out var cachedResponse))
                {
                    if (cachedResponse.Bars != null && cachedResponse.Bars.Count() >= barsBack)
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

            var result = await LoadHistoricalData(datafeed, symbol, granularity, barsBack, exchange);

            try
            {
                if (result != null && result != null && result.Bars != null && result.Bars.Any())
                {
                    if (granularity > 60 * 15)
                    {
                        _cache.Set(_cachePrefix, hash, result, TimeSpan.FromMinutes(15));
                    }
                    else if (granularity > 60 * 5)
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

        public async Task<HistoryData> GetHistoryByDates(string datafeed, string symbol, int granularity, string exchange, long start, long end)
        {
            var hash = getHash(symbol, granularity, datafeed, exchange) + start + end;
            try
            {
                if (_cache.TryGetValue(_cachePrefix, hash, out HistoryData cachedResponse))
                {
                    return cachedResponse;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get cached response");
                _logger.LogError(e.Message);
            }

            var bars = await SendHistoricalRequest(datafeed, symbol, granularity, exchange, start, end);
            var result = new HistoryData
            {
                Bars = bars,
                Datafeed = datafeed,
                Exchange = exchange,
                Granularity = granularity,
                Symbol = symbol
            };

            // try
            // {
            //     if (result.Bars != null && result.Bars.Any())
            //     {
            //         _cache.Set(_ _cache.Set(_cachePrefix, hash, result, TimeSpan.FromMinutes(60));
            //     }
            // }
            // catch (Exception e)
            // {
            //     _logger.LogError("Failed to set cached response");
            //     _logger.LogError(e.Message);
            // }

            return result;
        }

        private async Task<List<BarMessage>> SendHistoricalRequest(string datafeed, string symbol, int granularity, string exchange, long start, long end)
        {
            var bearerdatafeed = datafeed.ToLowerInvariant();
            var uri = $"{_serverUrl}/{bearerdatafeed}/history?" +
                        $"kind=daterange&symbol={symbol}&granularity={granularity}&from={start}&to={end}";

            if (!string.IsNullOrWhiteSpace(exchange))
            {
                uri = $"{uri}&exchange={exchange}";
            }

            var token = await _auth.GetToken();
            var request = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Headers = {
                { HttpRequestHeader.Authorization.ToString(), token }
            }
            };

            // Console.WriteLine(">>> SendHistoryRequest: " + uri);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error occured while sending http request to {request.RequestUri}, response string: {content}");
                response.EnsureSuccessStatusCode();
            }

            if (bearerdatafeed == "oanda")
            {
                var oandaResponse = JsonConvert.DeserializeObject<OandaHistoryResponse>(content);
                return oandaResponse.Data.Bars.ToList();
            }
            else
            {
                var historyResponse = JsonConvert.DeserializeObject<HistoryData>(content);
                return historyResponse.Bars.ToList();
            }
        }

        private async Task<HistoryData> SendHistoricalRequest(string datafeed, string symbol, int granularity, long bars_count, string exchange, int repeatCount)
        {
            var result = new HistoryData
            {
                Bars = new List<BarMessage>(),
                Datafeed = datafeed,
                Exchange = exchange,
                Granularity = granularity,
                Symbol = symbol
            };

            var requestCount = 0;
            repeatCount = (int)(bars_count / 3000) + repeatCount;
            var endDate = AlgoHelper.UnixTimeNow();
            var emptyResponseRepeatCount = 0;

            do
            {
                var startDate = this.getStartDate(result, endDate, bars_count);

                var bars = await SendHistoricalRequest(datafeed, symbol, granularity, exchange, startDate, endDate);

                endDate = startDate + 1;
                var prevCount = result.Bars.Count();
                result.Bars = this.mergeBars(result.Bars, bars);
                var afterCount = result.Bars.Count();

                if (prevCount == afterCount)
                {
                    if (emptyResponseRepeatCount > 0)
                    {
                        return result;
                    }
                    emptyResponseRepeatCount++;
                    repeatCount++;
                }
                else
                {
                    emptyResponseRepeatCount = 0;
                }

                if (requestCount > 0)
                {
                    Console.WriteLine($"History request count {requestCount} - {symbol} - {granularity} - {bars_count} -> existing {afterCount}");
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
                Console.WriteLine(">>> HISTORY LOAD EXCEPTION: " + symbol + " - " + granularity);
                Console.WriteLine(ex.ToString());
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

        private List<BarMessage> mergeBars(IEnumerable<BarMessage> existingBars, IEnumerable<BarMessage> newBars)
        {
            var firstBar = existingBars.FirstOrDefault();

            if (firstBar == null)
            {
                return newBars.ToList();
            }

            var barsToAppend = newBars.Where(_ => _.Timestamp < firstBar.Timestamp).ToList();
            barsToAppend.AddRange(existingBars);
            return barsToAppend;
        }

        private long getEndDate(HistoryData data, long defaultEndDate)
        {
            var firstBar = data.Bars.FirstOrDefault();

            if (firstBar == null)
            {
                return defaultEndDate;
            }

            return firstBar.Timestamp + 1;
        }

        private long getStartDate(HistoryData data, long endDate, long bars_count)
        {
            var existing_count = data.Bars.Count();
            var mult = 2;
            var bars_difference = (bars_count - existing_count);
            if (bars_difference < 300)
            {
                bars_difference = 300;
            }
            long startDate = endDate - (bars_difference * data.Granularity * mult);

            if (string.Equals(data.Datafeed, "Twelvedata", StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.Granularity < TimeframeHelper.DAILY_GRANULARITY)
                {
                    var minutesInDay = 6 * 60;
                    var minutesRequested = data.Granularity * bars_count / 60m;
                    var daysRequested = Math.Floor((minutesRequested * 2) / minutesInDay);
                    startDate = endDate - ((long)daysRequested * TimeframeHelper.DAILY_GRANULARITY);
                }
            }


            if (startDate < 0)
            {
                startDate = 0;
            }
            else
            {
                var dateTime = AlgoHelper.UnixTimeStampToDateTime(startDate);
                var day = dateTime.DayOfWeek;

                // load more minute data
                if (data.Granularity < 3600)
                {
                    startDate = startDate - ONE_DAY_TIME_SHIFT;
                }

                if (day == DayOfWeek.Sunday)
                {
                    // Sunday
                    startDate = startDate - (ONE_DAY_TIME_SHIFT * 2);
                }
                else if (day == DayOfWeek.Saturday)
                {
                    // Saturday
                    startDate = startDate - ONE_DAY_TIME_SHIFT;
                }
            }

            if (string.Equals(data.Datafeed, "oanda", StringComparison.InvariantCultureIgnoreCase))
            {
                var count = (endDate - startDate) / data.Granularity;
                if (count > 5000)
                {
                    startDate = endDate - (data.Granularity * 5000);
                }
            }

            // May 13 2014
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
