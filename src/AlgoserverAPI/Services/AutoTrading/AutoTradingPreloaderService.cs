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
        private readonly AutoTradingAccountsService _autoTradingAccountsService;
        private readonly string _cachePrefix = "at_trading_info_";
        private readonly Dictionary<string, Dictionary<string, AutoTradingSymbolInfoResponse>> _data = new Dictionary<string, Dictionary<string, AutoTradingSymbolInfoResponse>>();

        public AutoTradingPreloaderService(ICacheService cache, AlgoService algoService, AutoTradingUserInfoService autoTradingUserInfoService, AutoTradingAccountsService autoTradingAccountsService)
        {
            _cache = cache;
            _algoService = algoService;
            _autoTradingUserInfoService = autoTradingUserInfoService;
            _autoTradingAccountsService = autoTradingAccountsService;
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

        public List<string> GetAutoTradeAllInstruments()
        {
            var result = new List<string>();
            lock (_data)
            {
                foreach (var types in _data)
                {
                    foreach (var symbol in types.Value)
                    {
                        var name = symbol.Key.Split("_");
                        name = name.TakeLast(name.Length - 1).ToArray();
                        var s = String.Join("_", name).ToUpper();

                        if (string.Equals(s, "BTC_USD", StringComparison.InvariantCultureIgnoreCase) ||
                            string.Equals(s, "BTCUSD", StringComparison.InvariantCultureIgnoreCase))
                        {
                            s = "BTC_USDT";
                        }
                        if (string.Equals(s, "ETH_USD", StringComparison.InvariantCultureIgnoreCase) ||
                            string.Equals(s, "ETHUSD", StringComparison.InvariantCultureIgnoreCase))
                        {
                            s = "ETH_USDT";
                        }

                        result.Add(s.ToUpper());
                    }
                }
            }
            return result;
        }

        [Obsolete]
        public async Task<List<AutoTradingInstrumentsResponse>> GetAutoTradeInstruments(string account)
        {
            var result = new List<AutoTradingInstrumentsResponse>();
            var symbols = new Dictionary<string, AutoTradingSymbolInfoResponse>();
            var userSettings = _autoTradingUserInfoService.GetUserInfo(account);
            var maxAmount = _autoTradingAccountsService.GetMaxTradingInstrumentsCount(account);
            var isHITLOverride = maxAmount != int.MaxValue;

            lock (_data)
            {
                foreach (var types in _data)
                {
                    foreach (var symbol in types.Value)
                    {
                        if (symbol.Value.TradingState == 0)
                        {
                            continue;
                        }

                        var name = symbol.Key.Split("_");
                        name = name.TakeLast(name.Length - 1).ToArray();
                        var instrument = String.Join("_", name).ToUpper();
                        var canAutoTrade = symbol.Value.TradingState == 2;
                        if (canAutoTrade && !isHITLOverride)
                        {
                            symbols.Add(instrument, symbol.Value);
                        }
                        else if (userSettings != null && userSettings.useManualTrading && userSettings.markets != null)
                        {
                            var canTradeInHITLMode = symbol.Value.TradingState == 1;
                            if (!canTradeInHITLMode)
                            {
                                continue;
                            }
                            var s = getNormalizedInstrument(instrument);
                            var marketConfig = userSettings.markets.FirstOrDefault((_) => !string.IsNullOrEmpty(_.symbol) && string.Equals(getNormalizedInstrument(_.symbol), s, StringComparison.InvariantCultureIgnoreCase));
                            if (marketConfig != null)
                            {
                                symbols.Add(instrument, symbol.Value);
                            }
                        }
                    }
                }
            }

            symbols = symbols.Take(maxAmount).ToDictionary((_) => _.Key, (_) => _.Value);

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

        public async Task<AutoTradingInstrumentsDedicationResponse> GetAutoTradingInstrumentsDedication(string account)
        {
            var instruments = new List<AutoTradingInstrumentsResponse>();
            var symbols = new Dictionary<string, AutoTradingSymbolInfoResponse>();
            var userSettings = _autoTradingUserInfoService.GetUserInfo(account);
            var maxAmount = _autoTradingAccountsService.GetMaxTradingInstrumentsCount(account);
            var isHITLOverride = maxAmount != int.MaxValue;

            lock (_data)
            {
                foreach (var types in _data)
                {
                    foreach (var symbol in types.Value)
                    {
                        if (symbol.Value.TradingState == 0)
                        {
                            continue;
                        }

                        var name = symbol.Key.Split("_");
                        name = name.TakeLast(name.Length - 1).ToArray();
                        var instrument = String.Join("_", name).ToUpper();
                        var canAutoTrade = symbol.Value.TradingState == 2;
                        if (canAutoTrade && !isHITLOverride)
                        {
                            symbols.Add(instrument, symbol.Value);
                        }
                        else if (userSettings != null && userSettings.useManualTrading && userSettings.markets != null)
                        {
                            var canTradeInHITLMode = symbol.Value.TradingState == 1;
                            if (!canTradeInHITLMode)
                            {
                                continue;
                            }
                            var s = getNormalizedInstrument(instrument);
                            var marketConfig = userSettings.markets.FirstOrDefault((_) => !string.IsNullOrEmpty(_.symbol) && string.Equals(getNormalizedInstrument(_.symbol), s, StringComparison.InvariantCultureIgnoreCase));
                            if (marketConfig != null)
                            {
                                symbols.Add(instrument, symbol.Value);
                            }
                        }
                    }
                }
            }

            symbols = symbols.Take(maxAmount).ToDictionary((_) => _.Key, (_) => _.Value);

            var totalCount = 0m;
            foreach (var symbol in symbols)
            {
                var cnt = 1m / relatedSymbolsCount(symbol.Key, symbols);
                totalCount += cnt;
                instruments.Add(new AutoTradingInstrumentsResponse
                {
                    Symbol = symbol.Key,
                    Risk = cnt
                });
            }

            foreach (var r in instruments)
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
                foreach (var r in instruments)
                {
                    r.Risk = 1m / symbols.Count;
                }
            }
            else
            {
                var weight = 1m / totalCount;

                foreach (var r in instruments)
                {
                    r.Risk = weight * r.Risk;
                }
            }

            return new AutoTradingInstrumentsDedicationResponse
            {
                Instruments = instruments,
                Risks = userSettings.risksPerMarket != null ? userSettings.risksPerMarket : new Dictionary<string, int>(),
                AccountRisk = userSettings.accountRisk,
                UseManualTrading = userSettings.useManualTrading,
                DefaultMarketRisk = userSettings.defaultMarketRisk,
            };
        }

        public async Task<AutoTradingSymbolInfoResponse> GetAutoTradingSymbolInfo(string symbol, string datafeed, string exchange, string type)
        {
            var existing = GetAutoTradingSymbolInfoFromCache(symbol, datafeed, type);
            if (existing != null)
            {
                return existing;
            }

            var info = await _algoService.CalculateAutoTradingInfoAsync(symbol, datafeed, exchange, type);
            return info;
        }

        public AutoTradingSymbolInfoResponse GetAutoTradingSymbolInfoFromCache(string symbol, string datafeed, string type)
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

            return null;
        }

        public AutoTradingSymbolInfoResponse GetAutoTradingSymbolInfoFromCache(string symbol, string datafeed)
        {
            try
            {
                lock (_data)
                {
                    foreach (var type in _data)
                    {
                        var key = (datafeed + "_" + symbol).ToLower();
                        if (type.Value.ContainsKey(key))
                        {
                            return type.Value[key];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
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
            instrument = InstrumentsHelper.NormalizedInstrumentWithCrypto(instrument);
            return instrument;
        }
    }
}