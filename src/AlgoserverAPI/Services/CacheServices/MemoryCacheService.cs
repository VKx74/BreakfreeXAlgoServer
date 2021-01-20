using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Algoserver.API.Services.CacheServices {
    
    public class MemoryCacheService: ICacheService {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool TryGetValue<T>(string prefix, string key, out T result) {
            try
            {
                if (_cache.TryGetValue<T>(prefix + key, out var cachedResponse))
                {
                   result = cachedResponse;
                   return true;
                }
            }
            catch (Exception e)
            {
            }

            result = default(T);

            return false;
        }

        public bool Set(string prefix, string key, object value, TimeSpan expiration) {
            try
            {
                _cache.Set(prefix + key, value, expiration);
                return true;
            }
            catch (Exception e)
            {
            }
            return false;
        }
    }
}