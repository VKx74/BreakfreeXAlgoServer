using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Algoserver.API.Services.CacheServices
{

    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool TryGetValue<T>(string prefix, string key, out T result)
        {
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

        public bool Set(string prefix, string key, object value, TimeSpan expiration)
        {
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

        // public async Task<bool> SetAsync(string prefix, string key, object value, TimeSpan expiration)
        // {
        //     var res = Set(prefix, key, value, expiration);
        //     return res;
        // }

        // public async Task<T> TryGetValueAsync<T>(string prefix, string key)
        // {
        //     try
        //     {
        //         var cachedResult = _cache.Get<T>(prefix + key);

        //         if (cachedResult != null)
        //         {
        //             return cachedResult;
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Error during update of cache: {ex.Message}");
        //     }

        //     return default(T);
        // }
    }
}