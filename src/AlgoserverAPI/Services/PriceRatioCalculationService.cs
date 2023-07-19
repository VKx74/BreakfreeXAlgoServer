using System;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Services.CacheServices;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.Services
{
    public class PriceRatioCalculationService
    {
        private const int DAILY_GRANULARITY = 86400;

        private readonly ILogger<PriceRatioCalculationService> _logger;
        private readonly HistoryService _historyService;
        private readonly InstrumentService _instrumentService;
        private readonly IInMemoryCache _cache;
        private string _cachePrefix = "PriceRatio";

        public PriceRatioCalculationService(ILogger<PriceRatioCalculationService> logger, HistoryService historyService, InstrumentService instrumentService, IInMemoryCache cache)
        {
            _cache = cache;
            _logger = logger;
            _historyService = historyService;
            _instrumentService = instrumentService;
        }

        public async Task<decimal> GetSymbolRatio(string symbol, string account_currency, string datafeed, string type, string exchange)
        {
            var act = string.IsNullOrWhiteSpace(account_currency) ? "USD" : account_currency;
            var key = $"{symbol}-{act}";

            try
            {
                if (_cache.TryGetValue(_cachePrefix, key, out decimal cachedResponse))
                {
                    return cachedResponse;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get cached response");
                _logger.LogError(e.Message);
            }

            var usdRatio = await GetUSDRatio(symbol, datafeed, type, exchange);
            if (string.IsNullOrWhiteSpace(account_currency) || account_currency.ToUpperInvariant() == "USD")
            {
                return usdRatio;
            }

            var instrumentSeparator = "/";
            if (datafeed == "oanda")
            {
                instrumentSeparator = "_";
            }

            var acctCurrency = account_currency.ToUpperInvariant();
            var crossSymbol = $"USD{instrumentSeparator}{acctCurrency}";
            var direct = true;

            if (!_instrumentService.SymbolExist(datafeed, crossSymbol))
            {
                crossSymbol = $"{acctCurrency}{instrumentSeparator}USD";
                direct = false;
                if (!_instrumentService.SymbolExist(datafeed, crossSymbol))
                {
                    return 1;
                }
            }

            var acctCurrencyPrice = 1m;

            try
            {
                var dailyPriceData = await _historyService.GetHistory(crossSymbol, DAILY_GRANULARITY, datafeed, type, exchange);
                if (dailyPriceData != null && dailyPriceData.Bars != null && dailyPriceData.Bars.Any())
                {
                    var price = dailyPriceData.Bars.Last().Close;
                    acctCurrencyPrice = direct ? price : 1 / price;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get price ratio history. {crossSymbol}, {datafeed}, {exchange}");
            }

            var result = usdRatio / acctCurrencyPrice;

            try
            {
                _cache.Set(_cachePrefix, key, result, TimeSpan.FromHours(6));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to set cached response");
                _logger.LogError(e.Message);
            }

            return result;
        }

        public async Task<decimal> GetUSDRatio(string symbol, string datafeed, string type, string exchange)
        {
            try
            {
                if (_cache.TryGetValue(_cachePrefix, symbol, out decimal cachedResponse))
                {
                    return cachedResponse;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get cached response");
                _logger.LogError(e.Message);
            }

            var instrumentSeparator = "/";
            if (datafeed == "oanda")
            {
                instrumentSeparator = "_";
            }

            var currencies = symbol.Split(instrumentSeparator);

            if (currencies.Length != 2)
            {
                return 1;
            }

            if (currencies[1].ToLowerInvariant() == "usd")
            {
                return 1; // ***/USD -> USD/USD ratio = 1
            }

            var crossSymbol = $"USD{instrumentSeparator}{currencies[1]}";
            var direct = true;

            if (!_instrumentService.SymbolExist(datafeed, crossSymbol))
            {
                crossSymbol = $"{currencies[1]}{instrumentSeparator}USD";
                direct = false;

                if (!_instrumentService.SymbolExist(datafeed, crossSymbol))
                {
                    return 1;
                }
            }

            var result = 1m;

            try
            {
                var dailyPriceData = await _historyService.GetHistory(crossSymbol, DAILY_GRANULARITY, datafeed, type, exchange);
                if (dailyPriceData != null && dailyPriceData.Bars != null && dailyPriceData.Bars.Any())
                {
                    var price = dailyPriceData.Bars.Last().Close;
                    result = direct ? price : 1 / price;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get price ratio history. {crossSymbol}, {datafeed}, {exchange}");
            }

            try
            {
                _cache.Set(_cachePrefix, symbol, result, TimeSpan.FromHours(6));
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to set cached response");
                _logger.LogError(e.Message);
            }

            return result;
        }
    }
}
