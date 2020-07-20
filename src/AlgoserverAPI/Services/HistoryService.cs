using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
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
        private const int BARS_COUNT = 800;
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

        public async Task<HistoryResponse> GetHistory(string symbol, int granularity, string datafeed, string exchange = "")
        {   
            var hash = getHash(symbol, granularity, datafeed, exchange);

            try
            {
                if (_cache.TryGetValue(hash, out HistoryResponse cachedResponse))
                {
                    return cachedResponse;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get cached response");
                _logger.LogError(e.Message);
            }

            long endDate = AlgoHelper.UnixTimeNow() + (60 * 60 * 12); // + 12h to prevent TZ difference
            long startDate = endDate - (BARS_COUNT * granularity);
            int day = new DateTime(startDate).Day;

            // load more minute data
            if (granularity < 3600)
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

            var result = await LoadHistoricalData(datafeed, symbol, granularity, startDate, endDate, exchange);

            try
            {
                if (result != null && result.Data != null && result.Data.Bars != null && result.Data.Bars.Any())
                {
                    _cache.Set(hash, result, TimeSpan.FromMinutes(1));
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to set cached response");
                _logger.LogError(e.Message);
            }

            return result;
        }

        private async Task<HistoryResponse> LoadHistoricalData(string datafeed, string symbol, int granularity, long startDate, long endDate, string exchange = "")
        {
            var exists = _contextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader);
            var bearerString = authHeader.ToString();

            if (!exists || string.IsNullOrEmpty(bearerString)) {

            }

            var uri = $"{_serverUrl}/{datafeed.ToLowerInvariant()}/history?" +
                      $"kind=daterange&symbol={symbol}&granularity={granularity}&from={startDate}&to={endDate}";

            if (string.IsNullOrWhiteSpace(exchange))
            {
                uri = $"{uri}&exchange={exchange}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, uri) {
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

            return JsonConvert.DeserializeObject<HistoryResponse>(content);
        }

        private string getHash(string symbol, int granularity, string datafeed, string exchange = "")
        {
            return $"{symbol}{granularity}{datafeed}{exchange}";
        }
    }
}
