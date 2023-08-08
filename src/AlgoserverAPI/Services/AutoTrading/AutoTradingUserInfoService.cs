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

        public UserInfoData GetUserInfo(string account)
        {
            try
            {
                if (_cache.TryGetValue<UserInfoData>(_cachePrefix, account, out var res))
                {
                    return res ?? new UserInfoData();
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

        public UserInfoData UpdateUseManualTrading(string account, bool useManualTrading)
        {
            var info = GetUserInfo(account);
            info.useManualTrading = useManualTrading;
            UpdateUserInfo(account, info);
            return info;
        }
    }
}