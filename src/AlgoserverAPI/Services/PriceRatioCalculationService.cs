using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Algoserver.API.Services
{
    public class PriceRatioCalculationService
    {
        private const int DAILYG_RANULARITY = 86400;

        private readonly ILogger<PriceRatioCalculationService> _logger;
        private readonly HistoryService _historyService;

        public PriceRatioCalculationService(ILogger<PriceRatioCalculationService> logger, HistoryService historyService)
        {
            _logger = logger;
            _historyService = historyService;
        }

        public async Task<decimal> GetUSDRatio(string symbol, string datafeed, string exchange)
        {
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

            var crossSymbol = $"USD{instrumentSeparator}{currencies[0]}";

            try {
                var dailyPriceData = await _historyService.GetHistory(crossSymbol, DAILYG_RANULARITY, datafeed, exchange);
                if (dailyPriceData != null && dailyPriceData.Bars != null && dailyPriceData.Bars.Any()) {
                    return dailyPriceData.Bars.Last().Close;
                }
            } catch (Exception ex) {
                _logger.LogError($"Failed to get price ratio history. {crossSymbol}, {datafeed}, {exchange}");
            }

            return 1;
        }
        
    }
}
