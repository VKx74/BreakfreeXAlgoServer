using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Algoserver.API.Services
{
    // Provide historical data from oanda or twelvedata datafeeds
    public class HistoryService
    {
        private const int BARS_COUNT = 800;
        private const int ONE_DAY_TIME_SHIFT = 1000 * 60 * 60 * 24;

        private readonly HttpClient _httpClient;
        private readonly ILogger<HistoryService> _logger;

        public HistoryService(ILogger<HistoryService> logger) {
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HistoryResponse> GetHistory(string symbol, int granularity, string datafeed, string exchange = "")
        {
            long endDate = AlgoHelper.UnixTimeNow() + (1000 * 60 * 60 * 12); // + 12h to prevent TZ difference
            long startDate = endDate - (BARS_COUNT * granularity * 1000);
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

            return await LoadHistoricalData(datafeed, symbol, granularity, startDate / 1000, endDate / 1000, exchange);
        }

        private async Task<HistoryResponse> LoadHistoricalData(string datafeed, string symbol, int granularity, long startDate, long endDate, string exchange = "")
        {
            // TODO: Move out to config file
            var serverUrl = "https://datafeed.breakfreetrading.com";

            var uri = $"{serverUrl}/${datafeed.ToLowerInvariant()}/history?" +
                      $"kind=daterange&symbol=${symbol}&granularity=${granularity}&from=${startDate}&to=${endDate}";

            if (string.IsNullOrWhiteSpace(exchange))
            {
                uri = $"{uri}&exchange={exchange}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error occured while sending http request to {request.RequestUri}, response string: {content}");
                response.EnsureSuccessStatusCode();
            }

            return JsonConvert.DeserializeObject<HistoryResponse>(content);
        }
    }
}
