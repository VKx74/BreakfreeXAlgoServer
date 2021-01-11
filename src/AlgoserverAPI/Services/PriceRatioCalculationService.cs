using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.Services
{
    public class PriceRatioCalculationService
    {
        private const int DAILYG_RANULARITY = 86400;

        private readonly ILogger<PriceRatioCalculationService> _logger;
        private readonly HistoryService _historyService;
        private readonly InstrumentService _instrumentService;
        private readonly ICacheService _cache;
        private string _cachePrefix = "PriceRatio";

        public PriceRatioCalculationService(ILogger<PriceRatioCalculationService> logger, HistoryService historyService, InstrumentService instrumentService, ICacheService cache)
        {
            _cache = cache;
            _logger = logger;
            _historyService = historyService;
            _instrumentService = instrumentService;
        }

        public async Task<decimal> GetUSDRatio(string symbol, string datafeed, string type, string exchange)
        {
            try {
                if (_cache.TryGetValue(_cachePrefix, symbol, out decimal cachedResponse)) {
                    return cachedResponse;
                }
            } catch(Exception e) {
                _logger.LogError("Failed to get cached response");
                _logger.LogError(e.Message);
            }

            var instrumentSeparator = "/";
            if (datafeed == "oanda") {
                instrumentSeparator = "_";
            }

            var currencies = symbol.Split(instrumentSeparator);

            if (currencies.Length != 2) {
                return 1;
            }

            if (currencies[1].ToLowerInvariant() == "usd") {
                return 1; // ***/USD -> USD/USD ratio = 1
            }

            var crossSymbol = $"USD{instrumentSeparator}{currencies[1]}";
            var direct = true;

            if (!_instrumentService.SymbolExist(datafeed, crossSymbol)) {
                crossSymbol = $"{currencies[1]}{instrumentSeparator}USD";
                direct = false;

                if (!_instrumentService.SymbolExist(datafeed, crossSymbol)) {
                    return 1;
                }
            }

            var result = 1m;

            try {
                var dailyPriceData = await _historyService.GetHistory(crossSymbol, DAILYG_RANULARITY, datafeed, type, exchange);
                if (dailyPriceData != null && dailyPriceData.Bars != null && dailyPriceData.Bars.Any()) {
                    var price = dailyPriceData.Bars.Last().Close;
                    result = direct ? price : 1 / price;
                }
            } catch (Exception ex) {
                _logger.LogError($"Failed to get price ratio history. {crossSymbol}, {datafeed}, {exchange}");
            }

            try {
                _cache.Set(_cachePrefix, symbol, result, TimeSpan.FromHours(1));
            } catch(Exception e) {
                _logger.LogError("Failed to set cached response");
                _logger.LogError(e.Message);
            }

            return result;
        }
        
    }
}
