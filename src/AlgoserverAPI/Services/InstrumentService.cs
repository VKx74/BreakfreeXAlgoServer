using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Algoserver.API.Services
{
    // Provide historical data from oanda or twelvedata datafeeds
    public class InstrumentService
    {

        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;
        private readonly ILogger<InstrumentService> _logger;
        private readonly List<OandaInstruments> _oandaInstruments = new List<OandaInstruments>();
        private readonly List<TwelvedatsInstruments> _twelvedatsInstruments = new List<TwelvedatsInstruments>();
        private readonly List<KaikoInstruments> _kaikoInstruments = new List<KaikoInstruments>();

        public InstrumentService(ILogger<InstrumentService> logger, IConfiguration configuration) {
            _logger = logger;
            _serverUrl = configuration["DatafeedEndpoint"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Init() {
            var oandaInstruments = LoadOandaInstruments().GetAwaiter().GetResult();
            var twelvedataInstruments = LoadTwelvedataInstruments().GetAwaiter().GetResult();
            var kaikoInstruments = LoadKaikoInstruments().GetAwaiter().GetResult();

            _oandaInstruments.AddRange(oandaInstruments.Data);
            _twelvedatsInstruments.AddRange(twelvedataInstruments.Data);
            _kaikoInstruments.AddRange(kaikoInstruments.Data);
        }

        public List<OandaInstruments> GetOandaInstruments() {
            return this._oandaInstruments.ToList();
        }

        public bool SymbolExist(string datafeed, string symbol) {
            if (datafeed.ToLowerInvariant() == "oanda") {
                return _oandaInstruments.Any(_ => _.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase));
            }
            
            if (datafeed.ToLowerInvariant() == "twelvedata") {
                return _twelvedatsInstruments.Any(_ => _.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase));
            }
            
            if (datafeed.ToLowerInvariant() == "kaiko") {
                return _kaikoInstruments.Any(_ => _.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase));
            }

            return false;
        }

        private async Task<OandaInstrumentsResponse> LoadOandaInstruments()
        {
            // TODO: Move out to config file
            var datafeed = "oanda";

            var uri = $"{_serverUrl}/{datafeed.ToLowerInvariant()}/instruments";

            var request = new HttpRequestMessage(HttpMethod.Get, uri);            
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error occured while sending http request to {request.RequestUri}, response string: {content}");
                response.EnsureSuccessStatusCode();
            }

            return JsonConvert.DeserializeObject<OandaInstrumentsResponse>(content);
        }
        
        private async Task<TwelvedatsInstrumentsResponse> LoadTwelvedataInstruments()
        {
            // TODO: Move out to config file
            var datafeed = "twelvedata";

            var uri = $"{_serverUrl}/{datafeed.ToLowerInvariant()}/instruments?Kind=Forex&Skip=0&Take=10000";

            var request = new HttpRequestMessage(HttpMethod.Get, uri);            
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error occured while sending http request to {request.RequestUri}, response string: {content}");
                response.EnsureSuccessStatusCode();
            }

            return JsonConvert.DeserializeObject<TwelvedatsInstrumentsResponse>(content);
        }
        
        private async Task<KaikoInstrumentsResponse> LoadKaikoInstruments()
        {
            // TODO: Move out to config file
            var datafeed = "kaiko";

            var uri = $"{_serverUrl}/{datafeed.ToLowerInvariant()}/instruments?Skip=0&Take=10000";

            var request = new HttpRequestMessage(HttpMethod.Get, uri);            
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error occured while sending http request to {request.RequestUri}, response string: {content}");
                response.EnsureSuccessStatusCode();
            }

            return JsonConvert.DeserializeObject<KaikoInstrumentsResponse>(content);
        }
    }
}
