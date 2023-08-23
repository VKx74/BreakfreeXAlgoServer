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
        private readonly AutoTradingUserInfoService _autoTradingUserInfoService;
        private readonly string _cachePrefix = "at_trading_info_";
        private readonly Dictionary<string, Dictionary<string, AutoTradingSymbolInfoResponse>> _data = new Dictionary<string, Dictionary<string, AutoTradingSymbolInfoResponse>>();

        public AutoTradingPreloaderService(ICacheService cache, AlgoService algoService, AutoTradingUserInfoService autoTradingUserInfoService)
        {
            _cache = cache;
            _algoService = algoService;
            _autoTradingUserInfoService = autoTradingUserInfoService;
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

        public async Task<List<AutoTradingInstrumentsResponse>> GetAutoTradeInstruments(string account)
        {
            var result = new List<AutoTradingInstrumentsResponse>();
            var symbols = new Dictionary<string, AutoTradingSymbolInfoResponse>();
            var userSettings = _autoTradingUserInfoService.GetUserInfo(account);

            lock (_data)
            {
                foreach (var types in _data)
                {
                    foreach (var symbol in types.Value)
                    {
                        var name = symbol.Key.Split("_");
                        name = name.TakeLast(name.Length - 1).ToArray();
                        var instrument = String.Join("_", name).ToUpper();
                        if (symbol.Value.TrendState == 3)
                        {
                            symbols.Add(instrument, symbol.Value);
                        }
                        else if (userSettings != null && userSettings.useManualTrading && userSettings.markets != null)
                        {
                            var s = getNormalizedInstrument(instrument);
                            var marketConfig = userSettings.markets.FirstOrDefault((_) => !string.IsNullOrEmpty(_.symbol) && string.Equals(getNormalizedInstrument(_.symbol), s, StringComparison.InvariantCultureIgnoreCase));
                            if (marketConfig != null)
                            {
                                var tradingConfig = symbol.Value;
                                var minStrength = marketConfig.minStrength;
                                var minStrength1h = marketConfig.minStrength1H;
                                var minStrength4h = marketConfig.minStrength4H;
                                var minStrength1d = marketConfig.minStrength1D;

                                if (Math.Abs(tradingConfig.TotalStrength * 100) < minStrength)
                                {
                                    continue;
                                }

                                if (tradingConfig.TrendDirection == 1)
                                {
                                    if ((tradingConfig.Strength1H * 100 < minStrength1h) || (tradingConfig.Strength4H * 100 < minStrength4h) || (tradingConfig.Strength1D * 100 < minStrength1d))
                                    {
                                        continue;
                                    }
                                }
                                else if (tradingConfig.TrendDirection == -1)
                                {
                                    if ((tradingConfig.Strength1H * 100 > minStrength1h * -1) || (tradingConfig.Strength4H * 100 > minStrength4h * -1) || (tradingConfig.Strength1D * 100 > minStrength1d * -1))
                                    {
                                        continue;
                                    }
                                }

                                symbols.Add(instrument, tradingConfig);
                            }
                        }
                    }
                }
            }

            var totalCount = 0m;
            foreach (var symbol in symbols)
            {
                var cnt = 1m / relatedSymbolsCount(symbol.Key, symbols);
                totalCount += cnt;
                result.Add(new AutoTradingInstrumentsResponse
                {
                    Symbol = symbol.Key,
                    Risk = cnt
                });
            }

            foreach (var r in result)
            {
                if (string.Equals(r.Symbol, "BTC_USD", StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(r.Symbol, "BTCUSD", StringComparison.InvariantCultureIgnoreCase))
                {
                    r.Symbol = "BTC_USDT";
                }
                if (string.Equals(r.Symbol, "ETH_USD", StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(r.Symbol, "ETHUSD", StringComparison.InvariantCultureIgnoreCase))
                {
                    r.Symbol = "ETH_USDT";
                }
            }

            if (totalCount <= 0)
            {
                foreach (var r in result)
                {
                    r.Risk = 1m / symbols.Count;
                }
                return result;
            }

            var weight = 1m / totalCount;

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
            if (InstrumentsHelper.ForexCommodities.Any((_) => string.Equals(_, symbol, StringComparison.InvariantCultureIgnoreCase)))
            {
                return 1;
            }
            if (InstrumentsHelper.ForexBounds.Any((_) => string.Equals(_, symbol, StringComparison.InvariantCultureIgnoreCase)))
            {
                return 2;
            }
            if (InstrumentsHelper.ForexIndices.Any((_) => string.Equals(_, symbol, StringComparison.InvariantCultureIgnoreCase)))
            {
                return 3;
            }
            if (InstrumentsHelper.ForexMetals.Any((_) => string.Equals(_, symbol, StringComparison.InvariantCultureIgnoreCase)))
            {
                return 4;
            }
            return 0;
        }

        private string getNormalizedInstrument(string instrument)
        {
            instrument = InstrumentsHelper.NormalizeInstrument(instrument);
            var btcusd = new List<string> { "BTCUSDT", "BTCUSD" };
            if (btcusd.Any(_ => _.Equals(instrument, StringComparison.InvariantCultureIgnoreCase)))
            {
                return "btcusdt";
            } 
            var ethusdt = new List<string> { "ETHUSDT", "ETHUSD" };
            if (ethusdt.Any(_ => _.Equals(instrument, StringComparison.InvariantCultureIgnoreCase)))
            {
                return "ethusdt";
            }

            return instrument;
        }

    }
}