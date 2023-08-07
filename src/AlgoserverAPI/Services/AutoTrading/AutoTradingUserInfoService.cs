using System;
using System.Collections.Generic;
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
    }
}