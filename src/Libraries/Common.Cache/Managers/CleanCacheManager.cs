using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Cache.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Common.Cache.Managers
{
    public class CleanCacheManager
    {
        private readonly IConfigurationRoot _config;
        private readonly IMemoryCache _memoryCache;

        public CleanCacheManager(IMemoryCache memoryCache, IConfigurationRoot configuration)
        {
            _config = configuration;
            _memoryCache = memoryCache;
        }

        public async Task CleanCache(ICollection<Type> types)
        {
            foreach (var type in types)
            {
                Type cacheType = typeof(CacheManager<>).MakeGenericType(type);
                try
                {
                    ICacheManager cache = (ICacheManager)Activator.CreateInstance(cacheType, _memoryCache, _config);
                    await cache.RemoveAllHash();
                }
                catch (Exception e)
                {
                    Log.Error(e.StackTrace, e.Message);
                }
            }
        }
    }
}
