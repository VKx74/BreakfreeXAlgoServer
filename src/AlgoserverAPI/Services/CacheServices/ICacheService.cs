using System;
using System.Threading.Tasks;

namespace Algoserver.API.Services.CacheServices
{
    public interface ICacheService
    {
        bool TryGetValue<T>(string prefix, string key, out T result);
        bool Set(string prefix, string key, object value, TimeSpan expiration);
    }
}
