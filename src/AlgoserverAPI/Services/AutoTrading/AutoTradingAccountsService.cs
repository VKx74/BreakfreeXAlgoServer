using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Services.CacheServices;

namespace Algoserver.API.Services
{
    public class AutoTradingAccountsService
    {
        private readonly List<string> _accounts = new List<string>();
        private ICacheService _cache;
        private string _cachePrefix = "user_auto_trading_";
        private string _cacheKey = "accounts";
        public AutoTradingAccountsService(ICacheService cache)
        {
            _cache = cache;
        }

        public void Update()
        {
            try
            {
                if (_cache.TryGetValue<List<string>>(_cachePrefix, _cacheKey, out var cachedResponse))
                {
                    lock (_accounts)
                    {
                        _accounts.Clear();
                        _accounts.AddRange(cachedResponse);
                    }
                    Console.WriteLine(">>> Loaded auto trading account: " + _accounts.Count);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(">>> Error: Failed to load user_auto_trading_accounts from cache");
            }
        }

        public bool Validate(string id)
        {
            var allAccounts = _accounts.ToList();
            return allAccounts.Any((_) => string.Equals(_, id, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}