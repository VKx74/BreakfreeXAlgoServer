using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services.CacheServices
{
    public class AutoTradingPreloaderService
    {
        private readonly ICacheService _cache;
        private readonly AlgoService _algoService;
        private readonly string _cachePrefix = "at_trading_info_";
        private readonly Dictionary<string, Dictionary<string, AutoTradingSymbolInfoResponse>> _data = new Dictionary<string, Dictionary<string, AutoTradingSymbolInfoResponse>>();

        public AutoTradingPreloaderService(ICacheService cache, AlgoService algoService)
        {
            _cache = cache;
            _algoService = algoService;
        }

        public async Task LoadInstruments(string type)
        {
            type = type.ToLower();
            try
            {
                if (_cache.TryGetValue<Dictionary<string, AutoTradingSymbolInfoResponse>>(_cachePrefix, type, out var res))
                {
                    lock (_data)
                    {
                        if (!_data.ContainsKey(type))
                        {
                            _data.Add(type, new Dictionary<string, AutoTradingSymbolInfoResponse>());
                        }
                        _data[type] = res;
                        Console.WriteLine("Loaded Auto Trading Precalculated data: " + res.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<List<AutoTradingInstrumentsResponse>> GetAutoTradeInstruments()
        {
            var result = new List<AutoTradingInstrumentsResponse>();
            var symbols = new Dictionary<string, AutoTradingSymbolInfoResponse>();

            lock (_data)
            {
                foreach (var types in _data)
                {
                    foreach (var symbol in types.Value)
                    {
                        if (symbol.Value.TrendState == 3)
                        {
                            var name = symbol.Key.Split("_");
                            name = name.TakeLast(name.Length - 1).ToArray();
                            var instrument = String.Join("_", name).ToUpper();
                            symbols.Add(instrument, symbol.Value);
                        }
                    }
                }
            }

            foreach (var symbol in symbols)
            {
                result.Add(new AutoTradingInstrumentsResponse
                {
                    Symbol = symbol.Key,
                    Risk = 1m / symbols.Count
                });
            }

            return result;
        }

        public async Task<AutoTradingSymbolInfoResponse> GetAutoTradingSymbolInfo(string symbol, string datafeed, string exchange, string type)
        {
            type = type.ToLower();
            try
            {
                lock (_data)
                {
                    if (_data.ContainsKey(type))
                    {
                        var key = (datafeed + "_" + symbol).ToLower();
                        if (_data[type].ContainsKey(key))
                        {
                            return _data[type][key];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            var info = await _algoService.CalculateAutoTradingInfoAsync(symbol, datafeed, exchange, type);
            return info;
        }
    }
}