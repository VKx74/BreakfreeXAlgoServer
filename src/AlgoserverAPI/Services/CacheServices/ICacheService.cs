using System;
using System.Threading.Tasks;

namespace Algoserver.API.Services.CacheServices
{
    public interface ICacheService
    {
        bool TryGetValue<T>(string prefix, string key, out T result);
        bool Set(string prefix, string key, object value, TimeSpan expiration);
        // Task<bool> SetAsync(string prefix, string key, object value, TimeSpan expiration);
        // Task<T> TryGetValueAsync<T>(string prefix, string key);
    }
}
