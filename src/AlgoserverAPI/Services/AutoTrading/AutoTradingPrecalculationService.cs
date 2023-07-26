using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services.CacheServices
{
    public class AutoTradingPrecalculationService
    {
        private readonly ICacheService _cache;
        private readonly AlgoService _algoService;
        private readonly string _cachePrefix = "at_trading_info_";

        public AutoTradingPrecalculationService(ICacheService cache, AlgoService algoService)
        {
            _cache = cache;
            _algoService = algoService;
        }

        public async Task CalculateInstruments(List<IInstrument> instruments, string type)
        {
            type = type.ToLower();
            var tasksToWait = new List<Task<KeyValuePair<string, AutoTradingSymbolInfoResponse>>>();
            foreach (var instrument in instruments)
            {
                tasksToWait.Add(Task.Run(() =>
                {
                    return calculateInstruments(instrument, type);
                }));
            }

            try
            {
                var results = await Task.WhenAll<KeyValuePair<string, AutoTradingSymbolInfoResponse>>(tasksToWait);
                var resultsToSave = new Dictionary<string, AutoTradingSymbolInfoResponse>();
                foreach (var result in results)
                {
                    if (result.Key != null && result.Value != null)
                    {
                        resultsToSave.Add(result.Key, result.Value);
                    }
                }
                Console.WriteLine("Calculated Auto Trading Precalculated data: " + resultsToSave.Count);
                _cache.Set(_cachePrefix, type, resultsToSave, TimeSpan.FromMinutes(15));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task<KeyValuePair<string, AutoTradingSymbolInfoResponse>> calculateInstruments(IInstrument instrument, string type)
        {
            var key = (instrument.Datafeed + "_" + instrument.Symbol).ToLower();
            try
            {
                var info = await _algoService.CalculateAutoTradingInfoAsync(instrument.Symbol, instrument.Datafeed, instrument.Exchange, type);
                return new KeyValuePair<string, AutoTradingSymbolInfoResponse>(key, info);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return new KeyValuePair<string, AutoTradingSymbolInfoResponse>(key, null);
        }
    }
}