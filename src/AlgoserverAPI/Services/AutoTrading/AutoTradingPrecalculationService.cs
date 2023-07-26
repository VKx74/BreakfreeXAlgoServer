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
            var res = new Dictionary<string, AutoTradingSymbolInfoResponse>();
            foreach (var instrument in instruments)
            {
                try
                {
                    var key = (instrument.Datafeed + "_" + instrument.Symbol).ToLower();
                    var info = await _algoService.CalculateAutoTradingInfoAsync(instrument.Symbol, instrument.Datafeed, instrument.Exchange, type);
                    if (info != null)
                    {
                        res.Add(key, info);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            try
            {
                Console.WriteLine("Calculated Auto Trading Precalculated data: " + res.Count);
                _cache.Set(_cachePrefix, type, res, TimeSpan.FromMinutes(15));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}