using System;
using Algoserver.API.Exceptions;
using Microsoft.Extensions.Caching.Distributed;

namespace Algoserver.API.Services.CacheServices {
    
    public class RedisCacheService: ICacheService {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public bool TryGetValue<T>(string prefix, string key, out T result) {
            try
            {
                var cachedResult = _cache.Get(prefix + key);

                if (cachedResult != null)
                {
                    result = cachedResult.ToObject<T>();
                    return true;
                }
            }
            catch (Exception e)
            {}

            result = default(T);

            return false;
        }

        public bool Set(string prefix, string key, object value, TimeSpan expiration) {
            try
            {
                _cache.Set(prefix + key, value.ToByteArray(), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                });
                return true;
            }
            catch (Exception e)
            {}

            return false;
        }
    }
}