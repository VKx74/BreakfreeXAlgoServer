using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Algoserver.API.Services
{
    class PredictionRequest {
        public decimal[,] ohlcv { get; set; }
        public decimal[,] xmode_channels { get; set; }
    }

    public class LevelsPredictionResponse
    {
        public decimal support { get; set; }
        public decimal resistance { get; set; }
        public decimal support_ext { get; set; }
        public decimal resistance_ext { get; set; }
    }

    class LevelsPredictionResponseDTO
    {
        public decimal lower_1 { get; set; }
        public decimal lower_2 { get; set; }
        public decimal upper_1 { get; set; }
        public decimal upper_2 { get; set; }
    }

    // Provide historical data from oanda or twelvedata datafeeds
    public class LevelsPredictionService
    {

        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;
        private readonly ILogger<LevelsPredictionService> _logger;

        public LevelsPredictionService(ILogger<LevelsPredictionService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _serverUrl = configuration["LevelsPrediction"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<LevelsPredictionResponse> Predict(ScanningHistory historyData, List<LookBackResult> channelsData)
        {
            var length = 75;
            var open = historyData.Open.TakeLast(length).ToList();
            var high = historyData.High.TakeLast(length).ToList();
            var low = historyData.Low.TakeLast(length).ToList();
            var close = historyData.Close.TakeLast(length).ToList();
            var channels = channelsData.TakeLast(length).ToList();

            var requestData = new PredictionRequest();
            requestData.ohlcv = new decimal[75, 4];
            requestData.xmode_channels = new decimal[75, 4];

            for (var i = 0; i < length; i++)
            {
                requestData.ohlcv[i, 0] = open[i];
                requestData.ohlcv[i, 1] = high[i];
                requestData.ohlcv[i, 2] = low[i];
                requestData.ohlcv[i, 3] = close[i];

                requestData.xmode_channels[i, 0] = channels[i].EightEight;
                requestData.xmode_channels[i, 1] = channels[i].ZeroEight;
                requestData.xmode_channels[i, 2] = channels[i].Plus28;
                requestData.xmode_channels[i, 3] = channels[i].Minus28;
            }

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
            var data = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_serverUrl, data);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error occured while sending http request to {_serverUrl}, response string: {content}");
                response.EnsureSuccessStatusCode();
            }

            var deserialized = JsonConvert.DeserializeObject<LevelsPredictionResponseDTO>(content);
            return new LevelsPredictionResponse {
                support = deserialized.lower_1,
                support_ext = deserialized.lower_2,
                resistance = deserialized.upper_1,
                resistance_ext = deserialized.upper_2
            };
        }
    }
}
