using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;
using Algoserver.API.Services.CacheServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Algoserver.API.Services
{
    class PredictionRequest
    {
        public decimal[,] ohlcv { get; set; }
        public decimal[,] xmode_channels { get; set; }
    }

    class PredictionOhlcData
    {
        public string Time { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }

    class PredictionTrendRequest
    {
        public string market { get; set; }
        public string instrument { get; set; }
        public string timeframe { get; set; }
        public List<PredictionOhlcData> ohlcData { get; set; }
    }

    class PredictionLgbmRequest
    {
        public string market { get; set; }
        public string instrument { get; set; }
        public string timeframe { get; set; }
        public List<PredictionOhlcData> ohlcData { get; set; }
    }

    public class LevelsPredictionResponse
    {
        public decimal support { get; set; }
        public decimal resistance { get; set; }
        public decimal support_ext { get; set; }
        public decimal resistance_ext { get; set; }
    }

    [Serializable]
    public class TrendPredictionResponse
    {
        public List<decimal> mama { get; set; }
        public List<decimal> fama { get; set; }
    }

    [Serializable]
    public class LevelsPredictionLgbmResponse
    {
        public decimal upper_1_step_1 { get; set; }
        public decimal upper_1_step_2 { get; set; }
        public decimal upper_1_step_3 { get; set; }
        public decimal upper_1_step_4 { get; set; }
        public decimal upper_1_step_5 { get; set; }
        public decimal upper_1_step_6 { get; set; }
        public decimal upper_1_step_7 { get; set; }
        public decimal upper_1_step_8 { get; set; }
        public decimal upper_1_step_9 { get; set; }
        public decimal upper_1_step_10 { get; set; }
        public decimal upper_1_step_11 { get; set; }
        public decimal upper_1_step_12 { get; set; }
        public decimal upper_1_step_13 { get; set; }
        public decimal upper_1_step_14 { get; set; }
        public decimal upper_1_step_15 { get; set; }
        public decimal upper_1_step_16 { get; set; }
        public decimal upper_1_step_17 { get; set; }
        public decimal upper_1_step_18 { get; set; }
        public decimal upper_1_step_19 { get; set; }
        public decimal upper_1_step_20 { get; set; }
        public decimal upper_1_step_21 { get; set; }
        public decimal upper_1_step_22 { get; set; }
        public decimal upper_1_step_23 { get; set; }
        public decimal upper_1_step_24 { get; set; }
        public decimal upper_1_step_25 { get; set; }
        public decimal upper_1_step_26 { get; set; }
        public decimal upper_1_step_27 { get; set; }
        public decimal upper_1_step_28 { get; set; }
        public decimal upper_1_step_29 { get; set; }
        public decimal upper_1_step_30 { get; set; }
        public decimal lower_1_step_1 { get; set; }
        public decimal lower_1_step_2 { get; set; }
        public decimal lower_1_step_3 { get; set; }
        public decimal lower_1_step_4 { get; set; }
        public decimal lower_1_step_5 { get; set; }
        public decimal lower_1_step_6 { get; set; }
        public decimal lower_1_step_7 { get; set; }
        public decimal lower_1_step_8 { get; set; }
        public decimal lower_1_step_9 { get; set; }
        public decimal lower_1_step_10 { get; set; }
        public decimal lower_1_step_11 { get; set; }
        public decimal lower_1_step_12 { get; set; }
        public decimal lower_1_step_13 { get; set; }
        public decimal lower_1_step_14 { get; set; }
        public decimal lower_1_step_15 { get; set; }
        public decimal lower_1_step_16 { get; set; }
        public decimal lower_1_step_17 { get; set; }
        public decimal lower_1_step_18 { get; set; }
        public decimal lower_1_step_19 { get; set; }
        public decimal lower_1_step_20 { get; set; }
        public decimal lower_1_step_21 { get; set; }
        public decimal lower_1_step_22 { get; set; }
        public decimal lower_1_step_23 { get; set; }
        public decimal lower_1_step_24 { get; set; }
        public decimal lower_1_step_25 { get; set; }
        public decimal lower_1_step_26 { get; set; }
        public decimal lower_1_step_27 { get; set; }
        public decimal lower_1_step_28 { get; set; }
        public decimal lower_1_step_29 { get; set; }
        public decimal lower_1_step_30 { get; set; }
        public decimal upper_2_step_1 { get; set; }
        public decimal upper_2_step_2 { get; set; }
        public decimal upper_2_step_3 { get; set; }
        public decimal upper_2_step_4 { get; set; }
        public decimal upper_2_step_5 { get; set; }
        public decimal upper_2_step_6 { get; set; }
        public decimal upper_2_step_7 { get; set; }
        public decimal upper_2_step_8 { get; set; }
        public decimal upper_2_step_9 { get; set; }
        public decimal upper_2_step_10 { get; set; }
        public decimal upper_2_step_11 { get; set; }
        public decimal upper_2_step_12 { get; set; }
        public decimal upper_2_step_13 { get; set; }
        public decimal upper_2_step_14 { get; set; }
        public decimal upper_2_step_15 { get; set; }
        public decimal upper_2_step_16 { get; set; }
        public decimal upper_2_step_17 { get; set; }
        public decimal upper_2_step_18 { get; set; }
        public decimal upper_2_step_19 { get; set; }
        public decimal upper_2_step_20 { get; set; }
        public decimal upper_2_step_21 { get; set; }
        public decimal upper_2_step_22 { get; set; }
        public decimal upper_2_step_23 { get; set; }
        public decimal upper_2_step_24 { get; set; }
        public decimal upper_2_step_25 { get; set; }
        public decimal upper_2_step_26 { get; set; }
        public decimal upper_2_step_27 { get; set; }
        public decimal upper_2_step_28 { get; set; }
        public decimal upper_2_step_29 { get; set; }
        public decimal upper_2_step_30 { get; set; }
        public decimal lower_2_step_1 { get; set; }
        public decimal lower_2_step_2 { get; set; }
        public decimal lower_2_step_3 { get; set; }
        public decimal lower_2_step_4 { get; set; }
        public decimal lower_2_step_5 { get; set; }
        public decimal lower_2_step_6 { get; set; }
        public decimal lower_2_step_7 { get; set; }
        public decimal lower_2_step_8 { get; set; }
        public decimal lower_2_step_9 { get; set; }
        public decimal lower_2_step_10 { get; set; }
        public decimal lower_2_step_11 { get; set; }
        public decimal lower_2_step_12 { get; set; }
        public decimal lower_2_step_13 { get; set; }
        public decimal lower_2_step_14 { get; set; }
        public decimal lower_2_step_15 { get; set; }
        public decimal lower_2_step_16 { get; set; }
        public decimal lower_2_step_17 { get; set; }
        public decimal lower_2_step_18 { get; set; }
        public decimal lower_2_step_19 { get; set; }
        public decimal lower_2_step_20 { get; set; }
        public decimal lower_2_step_21 { get; set; }
        public decimal lower_2_step_22 { get; set; }
        public decimal lower_2_step_23 { get; set; }
        public decimal lower_2_step_24 { get; set; }
        public decimal lower_2_step_25 { get; set; }
        public decimal lower_2_step_26 { get; set; }
        public decimal lower_2_step_27 { get; set; }
        public decimal lower_2_step_28 { get; set; }
        public decimal lower_2_step_29 { get; set; }
        public decimal lower_2_step_30 { get; set; }
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
        private readonly string _serverLgbmUrl;
        private readonly string _serverTrendPredictionUrl;
        private readonly ILogger<LevelsPredictionService> _logger;
        private readonly ICacheService _cache;
        private string _cachePrefix = "Predictions_";

        public LevelsPredictionService(ILogger<LevelsPredictionService> logger, IConfiguration configuration, ICacheService cache)
        {
            _logger = logger;
            _cache = cache;
            _serverLgbmUrl = configuration["LevelsLgbmPrediction"];
            _serverTrendPredictionUrl = configuration["TrendPrediction"];

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            _httpClient = new HttpClient(clientHandler);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<TrendPredictionResponse> PredictTrend(ScanningHistory historyData, string symbol, int granularity)
        {
            Console.WriteLine($"Prediction Trend requested");

            var cachedResponse = tryGetTrendPredictionFromCache(symbol, granularity);

            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var length = 300;
            var open = historyData.Open.TakeLast(length).ToList();
            var high = historyData.High.TakeLast(length).ToList();
            var low = historyData.Low.TakeLast(length).ToList();
            var close = historyData.Close.TakeLast(length).ToList();
            var time = historyData.Time.TakeLast(length).ToList();

            var requestData = new PredictionTrendRequest();
            requestData.ohlcData = new List<PredictionOhlcData>();
            requestData.instrument = symbol;
            requestData.market = getMarketType(symbol);
            requestData.timeframe = getTimeframe(granularity);

            if (string.IsNullOrEmpty(requestData.market))
            {
                return null;
            }

            for (var i = 0; i < open.Count; i++)
            {
                requestData.ohlcData.Add(new PredictionOhlcData
                {
                    Open = open[i],
                    High = high[i],
                    Low = low[i],
                    Close = close[i],
                    Volume = 0,
                    Time = AlgoHelper.UnixTimeStampToDateTime(time[i]).ToString(),
                });
            }

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
            var data = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_serverTrendPredictionUrl, data);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error occured while sending http request to {_serverTrendPredictionUrl}, response string: {content}");
                response.EnsureSuccessStatusCode();
            }

            var deserialized = JsonConvert.DeserializeObject<TrendPredictionResponse>(content);

            tryAddTrendPredictionInCache(symbol, granularity, deserialized);

            return deserialized;
        }

        public async Task<LevelsPredictionLgbmResponse> PredictLgbm(ScanningHistory historyData, string symbol, int granularity)
        {
            Console.WriteLine($"Prediction Lgbm requested");

            var cachedResponse = tryGetLevelsPredictionLgbmFromCache(symbol, granularity);

            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var length = 100;
            var open = historyData.Open.TakeLast(length).ToList();
            var high = historyData.High.TakeLast(length).ToList();
            var low = historyData.Low.TakeLast(length).ToList();
            var close = historyData.Close.TakeLast(length).ToList();
            var time = historyData.Time.TakeLast(length).ToList();

            var requestData = new PredictionLgbmRequest();
            requestData.ohlcData = new List<PredictionOhlcData>();
            requestData.instrument = symbol;
            requestData.market = getMarketType(symbol);
            requestData.timeframe = getTimeframe(granularity);

            if (string.IsNullOrEmpty(requestData.market))
            {
                return null;
            }

            for (var i = 0; i < open.Count; i++)
            {
                requestData.ohlcData.Add(new PredictionOhlcData
                {
                    Open = open[i],
                    High = high[i],
                    Low = low[i],
                    Close = close[i],
                    Volume = 0,
                    Time = AlgoHelper.UnixTimeStampToDateTime(time[i]).ToString(),
                });
            }

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
            var data = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_serverLgbmUrl, data);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error occured while sending http request to {_serverLgbmUrl}, response string: {content}");
                response.EnsureSuccessStatusCode();
            }

            var deserialized = JsonConvert.DeserializeObject<LevelsPredictionLgbmResponse>(content);

            tryAddLevelsPredictionLgbmInCache(symbol, granularity, deserialized);

            return deserialized;
        }

        private string getMarketType(string symbol)
        {
            var forex = "Forex";
            var commodities = "Commodities";
            var indices = "Indices";
            
            if (symbol.Equals("SPX500_USD", StringComparison.InvariantCultureIgnoreCase)) return indices;
            if (symbol.Equals("NAS100_USD", StringComparison.InvariantCultureIgnoreCase)) return indices;
            if (symbol.Equals("JP225_USD", StringComparison.InvariantCultureIgnoreCase)) return indices;
            if (symbol.Equals("DE30_EUR", StringComparison.InvariantCultureIgnoreCase)) return indices;


            if (symbol.Equals("USD_JPY", StringComparison.InvariantCultureIgnoreCase)) return forex;
            if (symbol.Equals("USD_CHF", StringComparison.InvariantCultureIgnoreCase)) return forex;
            if (symbol.Equals("USD_CAD", StringComparison.InvariantCultureIgnoreCase)) return forex;
            if (symbol.Equals("NZD_USD", StringComparison.InvariantCultureIgnoreCase)) return forex;
            if (symbol.Equals("GBP_USD", StringComparison.InvariantCultureIgnoreCase)) return forex;
            if (symbol.Equals("GBP_JPY", StringComparison.InvariantCultureIgnoreCase)) return forex;
            if (symbol.Equals("EUR_USD", StringComparison.InvariantCultureIgnoreCase)) return forex;
            if (symbol.Equals("EUR_JPY", StringComparison.InvariantCultureIgnoreCase)) return forex;
            if (symbol.Equals("EUR_GBP", StringComparison.InvariantCultureIgnoreCase)) return forex;
            if (symbol.Equals("EUR_CHF", StringComparison.InvariantCultureIgnoreCase)) return forex;
            if (symbol.Equals("EUR_CAD", StringComparison.InvariantCultureIgnoreCase)) return forex;
            if (symbol.Equals("EUR_AUD", StringComparison.InvariantCultureIgnoreCase)) return forex;
            if (symbol.Equals("AUD_USD", StringComparison.InvariantCultureIgnoreCase)) return forex;

            if (symbol.Equals("XAU_USD", StringComparison.InvariantCultureIgnoreCase)) return commodities;
            if (symbol.Equals("XAU_EUR", StringComparison.InvariantCultureIgnoreCase)) return commodities;
            if (symbol.Equals("XAG_USD", StringComparison.InvariantCultureIgnoreCase)) return commodities;
            if (symbol.Equals("XAG_EUR", StringComparison.InvariantCultureIgnoreCase)) return commodities;
            if (symbol.Equals("WTICO_USD", StringComparison.InvariantCultureIgnoreCase)) return commodities;
            if (symbol.Equals("NATGAS_USD", StringComparison.InvariantCultureIgnoreCase)) return commodities;
            if (symbol.Equals("BCO_USD", StringComparison.InvariantCultureIgnoreCase)) return commodities;
            return String.Empty;
        }

        private string getMarketTypeRestr(string symbol)
        {
            var forex = "Forex";
            var commodities = "Commodities";
            var indices = "Indices";
            if (symbol.Equals("GBP_USD", StringComparison.InvariantCultureIgnoreCase)) return forex;
            return String.Empty;
        }

        private string getTimeframe(int granularity)
        {
            if (granularity == TimeframeHelper.MIN1_GRANULARITY) return "M1";
            if (granularity == TimeframeHelper.MIN5_GRANULARITY) return "M5";
            if (granularity == TimeframeHelper.MIN15_GRANULARITY) return "M15";
            if (granularity == TimeframeHelper.MIN30_GRANULARITY) return "M30";
            if (granularity == TimeframeHelper.HOURLY_GRANULARITY) return "H1";
            if (granularity == TimeframeHelper.HOUR4_GRANULARITY) return "H4";
            if (granularity == TimeframeHelper.DAILY_GRANULARITY) return "D";
            return "";
        }

        private LevelsPredictionLgbmResponse tryGetLevelsPredictionLgbmFromCache(string symbol, int granularity)
        {
            var hash = symbol + granularity.ToString() + "_LevelsPredictionLgbm";
            try
            {
                if (_cache.TryGetValue(_cachePrefix, hash, out LevelsPredictionLgbmResponse cachedResponse))
                {
                    return cachedResponse;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get cached response for LevelsPredictionLgbm");
                _logger.LogError(e.Message);
            }

            return null;
        }

        private TrendPredictionResponse tryGetTrendPredictionFromCache(string symbol, int granularity)
        {
            var hash = symbol + granularity.ToString() + "_TrendPrediction";
            try
            {
                if (_cache.TryGetValue(_cachePrefix, hash, out TrendPredictionResponse cachedResponse))
                {
                    return cachedResponse;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get cached response for TrendPrediction");
                _logger.LogError(e.Message);
            }

            return null;
        }

        private void tryAddLevelsPredictionLgbmInCache(string symbol, int granularity, LevelsPredictionLgbmResponse data)
        {
            var hash = symbol + granularity.ToString() + "_LevelsPredictionLgbm";
            try
            {
                _cache.Set(_cachePrefix, hash, data, TimeSpan.FromSeconds(Math.Min(granularity, 60 * 15)));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to add cached response for LevelsPredictionLgbm");
                _logger.LogError(e.Message);
            }
        }

        private void tryAddTrendPredictionInCache(string symbol, int granularity, TrendPredictionResponse data)
        {
            var hash = symbol + granularity.ToString() + "_TrendPrediction";
            try
            {
                _cache.Set(_cachePrefix, hash, data, TimeSpan.FromSeconds(Math.Min(granularity, 60 * 15)));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to add cached response for TrendPrediction");
                _logger.LogError(e.Message);
            }
        }
    }
}
