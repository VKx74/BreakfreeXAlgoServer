using System;
using System.Collections.Generic;
using System.Linq;
using Algoserver.API.Services.CacheServices;

namespace Algoserver.API.Services
{
    public class AutoTradingAccountsService
    {
        private readonly List<UserAutoTradingAccountCacheItem> _accounts = new List<UserAutoTradingAccountCacheItem>();
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
                if (_cache.TryGetValue<List<UserAutoTradingAccountCacheItem>>(_cachePrefix, _cacheKey, out var cachedResponse))
                {
                    lock (_accounts)
                    {
                        _accounts.Clear();
                        _accounts.AddRange(cachedResponse);
                    }
                    Console.WriteLine(">>> Loaded auto trading account from cache: " + _accounts.Count);
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
            return allAccounts.Any((_) => string.Equals(_.AccountId, id, StringComparison.InvariantCultureIgnoreCase));
        }

        public int GetMaxTradingInstrumentsCount(string id)
        {
            var allAccounts = _accounts.ToList();
            var account = allAccounts.FirstOrDefault((_) => string.Equals(_.AccountId, id, StringComparison.InvariantCultureIgnoreCase));
            if (account == null)
            {
                return 0;
            }

            if (account.Subscriptions == null || !account.Subscriptions.Any())
            {
                return 1;
            }

            var neural = "Neural";
            var pro = "Pro";
            var discovery = "Discovery";
            var starter = "Starter";
            var wings = "Wings";
            var ascension = "Ascension";
            var god = "God";

            var isNeural = account.Subscriptions.Any((_) => _.IndexOf(neural) != -1);
            var isPro = account.Subscriptions.Any((_) => _.IndexOf(pro) != -1);
            var isDiscovery = account.Subscriptions.Any((_) => _.IndexOf(discovery) != -1);
            var isStarter = account.Subscriptions.Any((_) => _.IndexOf(starter) != -1);
            var isWings = account.Subscriptions.Any((_) => _.IndexOf(wings) != -1);
            var isAscension = account.Subscriptions.Any((_) => _.IndexOf(ascension) != -1);
            var isGod = account.Subscriptions.Any((_) => _.IndexOf(god) != -1);

            if (isGod || isNeural || isPro || isDiscovery)
            {
                return int.MaxValue;
            }  
            
            if (isAscension)
            {
                return 4;
            }
            
            if (isWings)
            {
                return 2;
            }

            return 2;
        }
    }
}