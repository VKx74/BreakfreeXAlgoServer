using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Algoserver.API.Services
{
    // Provide historical data from oanda or twelvedata datafeeds
    public class HistoryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HistoryService> _logger;

        public HistoryService(ILogger<HistoryService> logger) {
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HistoryResponse> LoadHistoricalData(string datafeed, string symbol, string granularity, int startDate, int endDate)
        {
            // TODO: Move out to config file
            var serverUrl = "https://datafeed.breakfreetrading.com";

            var uri = $"{serverUrl}/${datafeed.ToLowerInvariant()}/history?" +
                      $"kind=daterange&symbol=${symbol}&granularity=${granularity}&from=${startDate}&to=${endDate}";

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
