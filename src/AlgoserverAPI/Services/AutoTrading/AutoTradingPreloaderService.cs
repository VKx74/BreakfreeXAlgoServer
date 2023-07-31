using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
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

            var totalCount = 0m;
            foreach (var symbol in symbols)
            {
                var cnt = 1 / relatedSymbolsCount(symbol.Key, symbols);
                totalCount += cnt;
                result.Add(new AutoTradingInstrumentsResponse
                {
                    Symbol = symbol.Key,
                    Risk = cnt
                });
            }

            if (totalCount <= 0)
            {
                foreach (var r in result)
                {
                    r.Risk = 1m / symbols.Count;
                }
                return result;
            }

            var weight = 1 / totalCount;

            foreach (var r in result)
            {
                r.Risk = weight * r.Risk;
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

        private int relatedSymbolsCount(string symbol, Dictionary<string, AutoTradingSymbolInfoResponse> symbols)
        {
            var symbolType = getSymbolsType(symbol);
            var curencies = symbol.Split("_");

            if (curencies.Length != 2)
            {
                return 1;
            }
            var curency1 = curencies[0];
            var curency2 = curencies[1];


            var count = 1;
            foreach (var s in symbols)
            {
                var c = s.Key.Split("_");
                if (c.Length != 2 || s.Key == symbol)
                {
                    continue;
                }

                var sT = getSymbolsType(s.Key);
                if (symbolType != sT)
                {
                    continue;
                }

                var c1 = c[0];
                var c2 = c[1];

                if (c1 == curency1)
                {
                    count++;
                }
                if (c2 == curency2)
                {
                    count++;
                }
            }

            return count;
        }

        private int getSymbolsType(string symbol)
        {
            if (InstrumentsHelper.ForexCommodities.All((_) => string.Equals(_, symbol, StringComparison.InvariantCultureIgnoreCase)))
            {
                return 1;
            }
            if (InstrumentsHelper.ForexBounds.All((_) => string.Equals(_, symbol, StringComparison.InvariantCultureIgnoreCase)))
            {
                return 2;
            }
            if (InstrumentsHelper.ForexIndices.All((_) => string.Equals(_, symbol, StringComparison.InvariantCultureIgnoreCase)))
            {
                return 3;
            }
            if (InstrumentsHelper.ForexMetals.All((_) => string.Equals(_, symbol, StringComparison.InvariantCultureIgnoreCase)))
            {
                return 4;
            }
            return 0;
        }

    }
}