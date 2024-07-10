using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Helpers;
using Algoserver.API.Models;
using Algoserver.API.Services.CacheServices;

namespace Algoserver.API.Services
{
    public class AutoTradingUserInfoService
    {
        private readonly ICacheService _cache;
        private readonly string _cachePrefix = "user_defined_symbol_";

        public AutoTradingUserInfoService(ICacheService cache)
        {
            _cache = cache;
        }

        public UserInfoData GetDefaultUserInfo(string account)
        {
            return new UserInfoData();
        }

        public UserInfoData GetUserInfo(string account)
        {
            try
            {
                if (_cache.TryGetValue<UserInfoData>(_cachePrefix, account, out var res))
                {
                    if (res != null)
                    {
                        res.Validate();
                        return res;
                    }

                    return new UserInfoData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return new UserInfoData();
        }

        public void UpdateUserInfo(string account, UserInfoData info)
        {
            try
            {
                _cache.Set(_cachePrefix, account, info, TimeSpan.FromDays(100));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public UserInfoData ChangeMarketRisk(string account, string market, int risk)
        {
            var info = GetUserInfo(account);
            market = InstrumentsHelper.NormalizedInstrumentWithCrypto(market);

            if (risk > 100)
            {
                return info;
            }

            if (info.risksPerMarket == null)
            {
                info.risksPerMarket = new Dictionary<string, int>();
            }

            if (info.risksPerMarket.ContainsKey(market))
            {
                if (risk > 0)
                {
                    info.risksPerMarket[market] = risk;
                }
                else
                {
                    info.risksPerMarket.Remove(market);
                }
            }
            else if (risk > 0)
            {
                info.risksPerMarket.Add(market, risk);
            }

            UpdateUserInfo(account, info);
            return info;
        }

        public UserInfoData ChangeGroupRisk(string account, string group, int risk)
        {
            var info = GetUserInfo(account);

            if (risk > 100)
            {
                return info;
            }

            if (info.risksPerGroup == null)
            {
                info.risksPerGroup = new Dictionary<string, int>();
            }

            if (info.risksPerGroup.ContainsKey(group))
            {
                if (risk > 0)
                {
                    info.risksPerGroup[group] = risk;
                }
                else
                {
                    info.risksPerGroup.Remove(group);
                }
            }
            else if (risk > 0)
            {
                info.risksPerGroup.Add(group, risk);
            }

            UpdateUserInfo(account, info);
            return info;
        }

        public UserInfoData ChangeAccountRisk(string account, int risk)
        {
            var info = GetUserInfo(account);

            if (risk > 100 || risk <= 0)
            {
                return info;
            }

            info.accountRisk = risk;

            UpdateUserInfo(account, info);
            return info;
        }

        public UserInfoData ChangeDefaultMarketRisk(string account, int risk)
        {
            var info = GetUserInfo(account);

            if (risk > 100)
            {
                return info;
            }

            if (risk < 0)
            {
                risk = 12;
            }

            info.defaultMarketRisk = risk;

            UpdateUserInfo(account, info);
            return info;
        }

        public UserInfoData ChangeDefaultGroupRisk(string account, int risk)
        {
            var info = GetUserInfo(account);

            if (risk > 100)
            {
                return info;
            }

            if (risk < 0)
            {
                risk = 30;
            }

            info.defaultGroupRisk = risk;

            UpdateUserInfo(account, info);
            return info;
        }

        public UserInfoData AddMarkets(string account, List<UserDefinedMarketData> markets)
        {
            var info = GetUserInfo(account);

            if (info.markets == null)
            {
                info.markets = new List<UserDefinedMarketData>();
            }

            foreach (var market in markets)
            {
                var normalizedMarket = InstrumentsHelper.NormalizeInstrument(market.symbol);
                if (info.markets.Any((_) => string.Equals(InstrumentsHelper.NormalizeInstrument(_.symbol), normalizedMarket, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                info.markets.Add(market);
            }

            UpdateUserInfo(account, info);
            return info;
        }

        public UserInfoData AddDisabledMarkets(string account, List<string> markets)
        {
            var info = GetUserInfo(account);

            if (info.disabledMarkets == null)
            {
                info.disabledMarkets = new List<string>();
            }

            foreach (var market in markets)
            {
                var normalizedMarket = InstrumentsHelper.NormalizeInstrument(market);
                if (info.disabledMarkets.Any((_) => string.Equals(InstrumentsHelper.NormalizeInstrument(_), normalizedMarket, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                info.disabledMarkets.Add(market);
            }

            UpdateUserInfo(account, info);
            return info;
        }

        public UserInfoData RemoveMarkets(string account, List<string> markets)
        {
            var info = GetUserInfo(account);

            if (info.markets == null)
            {
                info.markets = new List<UserDefinedMarketData>();
            }

            foreach (var market in markets)
            {
                var normalizedMarket = InstrumentsHelper.NormalizeInstrument(market);
                info.markets.RemoveAll((_) => string.Equals(InstrumentsHelper.NormalizeInstrument(_.symbol), normalizedMarket, StringComparison.InvariantCultureIgnoreCase));
            }

            UpdateUserInfo(account, info);
            return info;
        }

        public UserInfoData RemoveDisabledMarkets(string account, List<string> markets)
        {
            var info = GetUserInfo(account);

            if (info.disabledMarkets == null)
            {
                info.disabledMarkets = new List<string>();
            }

            foreach (var market in markets)
            {
                var normalizedMarket = InstrumentsHelper.NormalizeInstrument(market);
                info.disabledMarkets.RemoveAll((_) => string.Equals(InstrumentsHelper.NormalizeInstrument(_), normalizedMarket, StringComparison.InvariantCultureIgnoreCase));
            }

            UpdateUserInfo(account, info);
            return info;
        }

        public UserInfoData UpdateUseManualTrading(string account, bool useManualTrading)
        {
            var info = GetUserInfo(account);
            info.useManualTrading = useManualTrading;
            UpdateUserInfo(account, info);
            return info;
        }

        public UserInfoData UpdateBotState(string account, bool botShutDown)
        {
            var info = GetUserInfo(account);
            info.botShutDown = botShutDown;
            UpdateUserInfo(account, info);
            return info;
        }

        public UserInfoData ResetSettings(string account)
        {
            var info = GetDefaultUserInfo(account);
            UpdateUserInfo(account, info);
            return info;
        }
    }
}