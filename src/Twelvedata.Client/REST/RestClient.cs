using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Twelvedata.Client.REST.Requests;
using Twelvedata.Client.REST.Response;

namespace Twelvedata.Client.REST
{
    public class RestClient
    {
        private readonly string _baseAddress;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<RestClient> _logger;

        public RestClient(string baseUrl, string apiKey, ILogger<RestClient> logger)
        {
            _baseAddress = baseUrl;
            _apiKey = apiKey;
            _logger = logger;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseAddress),
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<TimeSeriesResponse> GetHistoryAsync(GetTimeSeriesRequest request)
        {
            var response = await SendAndGetResponseAsync(request, TwelvedataRoutes.TimeSeries);
            var result = JsonConvert.DeserializeObject<TimeSeriesResponse>(response);

            return result;
        }

        public async Task<InstrumentResponse<IndicesInstrument>> GetIndicesInstrumentsAsync()
        {
            var response = await SendAndGetResponseAsync(null, TwelvedataRoutes.IndicesList);
            var result = JsonConvert.DeserializeObject<InstrumentResponse<IndicesInstrument>>(response);

            return result;
        }

        public async Task<InstrumentResponse<CryptoInstrument>> GetCryptoInstrumentsAsync()
        {
            var response = await SendAndGetResponseAsync(null, TwelvedataRoutes.CryptocurrenciesList);
            var result = JsonConvert.DeserializeObject<InstrumentResponse<CryptoInstrument>>(response);

            return result;
        }

        public async Task<InstrumentResponse<StockInstrument>> GetStockInstrumentsAsync()
        {
            var response = await SendAndGetResponseAsync(null, TwelvedataRoutes.StocksList);
            var result = JsonConvert.DeserializeObject<InstrumentResponse<StockInstrument>>(response);

            return result;
        }

        public async Task<InstrumentResponse<ForexInstrument>> GetForexInstrumentsAsync() 
        {
            var response = await SendAndGetResponseAsync(null, TwelvedataRoutes.ForexList);
            var result = JsonConvert.DeserializeObject<InstrumentResponse<ForexInstrument>>(response);

            return result;
        }

        public async Task<TResult> ExecuteAsync<TResult>(object request, string route)
        {
            var response = await SendAndGetResponseAsync(request, route);
            var result = JsonConvert.DeserializeObject<TResult>(response);

            return result;
        }

        private async Task<string> SendAndGetResponseAsync(object request, string uriTemplate)
        {
            var uriTemplateWithApiKey = $"{uriTemplate}?apikey={_apiKey}";
            var uri = request == null ? uriTemplateWithApiKey : TwelvedataUriBuilder.GetUri(uriTemplateWithApiKey, request);
            var httpRquest = new HttpRequestMessage(HttpMethod.Get, uri);

            return await SendAndGetResponseAsync(httpRquest);
        }

        private async Task<string> SendAndGetResponseAsync(HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error occured while sending http request to {request.RequestUri}, response string: {responseString}");
                response.EnsureSuccessStatusCode();
            }

            return responseString;
        }
    }
}
