using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algoserver.API.Helpers;
using Algoserver.API.Models.REST;

namespace Algoserver.API.Services.CacheServices
{
    public enum EStrategyType
    {
        SR = 0,
        N = 2,
    }

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
                        if (string.Equals(s, "SOL_USD", StringComparison.InvariantCultureIgnoreCase) ||
                            string.Equals(s, "SOLUSD", StringComparison.InvariantCultureIgnoreCase))
                        {
                            s = "SOL_USDT";
                        }
                        if (string.Equals(s, "LTC_USD", StringComparison.InvariantCultureIgnoreCase) ||
                            string.Equals(s, "LTCUSD", StringComparison.InvariantCultureIgnoreCase))
                        {
                            s = "LTC_USDT";
                        }

                        result.Add(s.ToUpper());
                    }
                }
            }
            return result;
        }

        public async Task<AutoTradingInstrumentsDedicationResponse> GetAutoTradingInstrumentsDedication(string account, EStrategyType strategyType)
        {
            var instruments = new List<AutoTradingInstrumentsResponse>();
            var autoSymbols = new Dictionary<string, AutoTradingSymbolInfoResponse>();
            var hitlSymbols = new Dictionary<string, AutoTradingSymbolInfoResponse>();
            var userSettings = _autoTradingUserInfoService.GetUserInfo(account);
            var maxAmount = _autoTradingAccountsService.GetMaxTradingInstrumentsCount(account);
            var disabledMarkets = userSettings.disabledMarkets != null ? userSettings.disabledMarkets : new List<string>();
            var isHITLEnabled = userSettings != null && userSettings.useManualTrading;
            // var isHITLOverride = maxAmount != int.MaxValue && isHITLEnabled;

            lock (_data)
            {
                foreach (var types in _data)
                {
                    foreach (var symbol in types.Value)
                    {
                        // if (symbol.Value.TradingState == 0)
                        // {
                        //     continue;
                        // }

                        var name = symbol.Key.Split("_");
                        name = name.TakeLast(name.Length - 1).ToArray();
                        var instrument = string.Join("_", name).ToUpper();
                        var canAutoTrade = strategyType == EStrategyType.N ? symbol.Value.TradingStateN == 2 : symbol.Value.TradingStateSR == 2;
                        if (canAutoTrade)
                        {
                            autoSymbols.Add(instrument, symbol.Value);
                        }
                        else if (isHITLEnabled)
                        {
                            var canTradeInHITLMode = strategyType == EStrategyType.N ? symbol.Value.TradingStateN == 1 : symbol.Value.TradingStateSR == 1;
                            if (!canTradeInHITLMode)
                            {
                                continue;
                            }
                            if (userSettings != null && userSettings.markets != null)
                            {
                                var s = getNormalizedInstrument(instrument);
                                var marketConfig = userSettings.markets.FirstOrDefault((_) => !string.IsNullOrEmpty(_.symbol) && string.Equals(getNormalizedInstrument(_.symbol), s, StringComparison.InvariantCultureIgnoreCase));
                                if (marketConfig != null)
                                {
                                    hitlSymbols.Add(instrument, symbol.Value);
                                }
                            }
                        }
                    }
                }
            }

            var symbols = new Dictionary<string, AutoTradingSymbolInfoResponse>();

            foreach (var i in hitlSymbols)
            {
                symbols.Add(i.Key, i.Value);
            }

            foreach (var i in autoSymbols)
            {
                symbols.Add(i.Key, i.Value);
            }

            if (maxAmount != int.MaxValue)
            {
                var filteredSymbols = symbols.Where((_) => !disabledMarkets.Any((disabledMarket) => string.Equals(getNormalizedInstrument(_.Key), getNormalizedInstrument(disabledMarket), StringComparison.InvariantCultureIgnoreCase)));
                symbols = filteredSymbols.Take(maxAmount).ToDictionary((_) => _.Key, (_) => _.Value);
            }

            foreach (var symbol in symbols)
            {
                var isDisabled = false;
                if (userSettings.botShutDown || disabledMarkets.Any((_) => string.Equals(getNormalizedInstrument(_), getNormalizedInstrument(symbol.Key), StringComparison.InvariantCultureIgnoreCase)))
                {
                    isDisabled = true;
                }

                var cnt = relatedSymbolsCount(symbol.Key, symbols);
                var group = InstrumentsHelper.GetInstrumentGroup(symbol.Key);
                var groupRisk = userSettings.risksPerGroup != null && userSettings.risksPerGroup.ContainsKey(group) ? userSettings.risksPerGroup[group] : -1;
                if (groupRisk < 0)
                {
                    groupRisk = userSettings.defaultGroupRisk;
                    if (groupRisk <= 0)
                    {
                        groupRisk = 30;
                    }
                }

                var risk = groupRisk / cnt;

                instruments.Add(new AutoTradingInstrumentsResponse
                {
                    Symbol = symbol.Key,
                    Risk = isDisabled ? 0 : risk,
                    IsDisabled = isDisabled,
                    TradingStateSR = symbol.Value.TradingStateSR,
                    TradingStateN = symbol.Value.TradingStateN,
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
                if (string.Equals(r.Symbol, "SOL_USD", StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(r.Symbol, "SOLUSD", StringComparison.InvariantCultureIgnoreCase))
                {
                    r.Symbol = "SOL_USDT";
                }
                if (string.Equals(r.Symbol, "LTC_USD", StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(r.Symbol, "LTCUSD", StringComparison.InvariantCultureIgnoreCase))
                {
                    r.Symbol = "LTC_USDT";
                }
            }

            // if (totalCount <= 0)
            // {
            //     foreach (var r in instruments)
            //     {
            //         r.Risk = 1m / symbols.Count;
            //     }
            // }
            // else
            // {
            //     var weight = 1m / totalCount;

            //     foreach (var r in instruments)
            //     {
            //         r.Risk = weight * r.Risk;
            //     }
            // }

            return new AutoTradingInstrumentsDedicationResponse
            {
                Instruments = instruments,
                Risks = userSettings.risksPerMarket != null ? userSettings.risksPerMarket : new Dictionary<string, int>(),
                GroupRisks = userSettings.risksPerGroup != null ? userSettings.risksPerGroup : new Dictionary<string, int>(),
                AccountRisk = userSettings.accountRisk,
                UseManualTrading = userSettings.useManualTrading,
                BotShutDown = userSettings.botShutDown,
                DefaultMarketRisk = userSettings.defaultMarketRisk,
                DefaultGroupRisk = userSettings.defaultGroupRisk,
                DisabledInstruments = disabledMarkets
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
            if (string.Equals(symbol, "ETH_USD", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(symbol, "ETH_USDT", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(symbol, "ETHUSD", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(symbol, "ETHUSDT", StringComparison.InvariantCultureIgnoreCase))
            {
                symbol = "ETH_USD";
                datafeed = "OANDA";
            }

            if (string.Equals(symbol, "BTC_USD", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(symbol, "BTC_USDT", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(symbol, "BTCUSD", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(symbol, "BTCUSDT", StringComparison.InvariantCultureIgnoreCase))
            {
                symbol = "BTC_USD";
                datafeed = "OANDA";
            }

            if (string.Equals(symbol, "SOL_USD", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(symbol, "SOL_USDT", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(symbol, "SOLUSD", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(symbol, "SOLUSDT", StringComparison.InvariantCultureIgnoreCase))
            {
                symbol = "SOL_USD";
                datafeed = "OANDA";
            }

            if (string.Equals(symbol, "LTC_USD", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(symbol, "LTC_USDT", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(symbol, "LTCUSD", StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(symbol, "LTCUSDT", StringComparison.InvariantCultureIgnoreCase))
            {
                symbol = "LTC_USD";
                datafeed = "OANDA";
            }

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
            var symbol1Type = InstrumentsHelper.GetInstrumentGroup(symbol);
            var count = 0;
            foreach (var s in symbols)
            {
                var symbol2Type = InstrumentsHelper.GetInstrumentGroup(s.Key);
                if (symbol1Type == symbol2Type)
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