using System;
using System.Threading.Tasks;
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error during update of cache: {ex.Message}");
            }

            result = default(T);

            return false;
        }

        public async Task<T> TryGetValueAsync<T>(string prefix, string key) {
            try
            {
                var cachedResult = await _cache.GetAsync(prefix + key);

                if (cachedResult != null)
                {
                    return cachedResult.ToObject<T>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during update of cache: {ex.Message}");
            }

            return default(T);
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error during update of cache: {ex.Message}");
            }

            return false;
        }

        public async Task<bool> SetAsync(string prefix, string key, object value, TimeSpan expiration) {
            try
            {
                await _cache.SetAsync(prefix + key, value.ToByteArray(), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during update of cache: {ex.Message}");
            }

            return false;
        }
    }
}