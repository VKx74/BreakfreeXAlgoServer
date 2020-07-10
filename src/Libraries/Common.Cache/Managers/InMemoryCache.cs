using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logic.Enums;
using Common.Logic.Infrastructure;
using Common.Logic.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog;

namespace Common.Cache.Managers
{
    public class InMemoryCache<TModel> where TModel : class
    {
        private readonly string _source;
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
        private static CancellationTokenSource _cts = new CancellationTokenSource();


        public InMemoryCache(IMemoryCache memoryCache, IConfigurationRoot configuration)
        {
            if (bool.TryParse(configuration["InMemoryCache:Enabled"], out var enabled) && enabled)
            {
                _memoryCache = memoryCache;

                if (int.TryParse(configuration["InMemoryCache:Options:AbsoluteExpirationRelativeToNow"],
                    out var absoluteExpirationRelativeToNow))
                {
                    _memoryCacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(absoluteExpirationRelativeToNow != 0 ? absoluteExpirationRelativeToNow : 20),
                        ExpirationTokens = { new CancellationChangeToken(_cts.Token) }
                    };
                }
            }
            _source = $"{GetType().Name}({typeof(TModel).Name})";
        }

        public async Task<IOperationResult<ICollection<TModel>>> TryGetCollection(string key)
        {
            try
            {
                if (_memoryCache != null && _memoryCache.TryGetValue(key, out ICollection<TModel> found))
                {
                    return new OperationResult<ICollection<TModel>>(found);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return new OperationResult<ICollection<TModel>>(ErrorType.UnknownError, e);
            }
            return new OperationResult<ICollection<TModel>>(ErrorType.ItemDoesNotExist, _source);
        }

        public async Task<IOperationResult<TModel>> TryGetObject(string key)
        {
            try
            {
                if (_memoryCache != null && _memoryCache.TryGetValue(key, out TModel found))
                {
                    return new OperationResult<TModel>(found);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return new OperationResult<TModel>(ErrorType.UnknownError, e);
            }
            return new OperationResult<TModel>(ErrorType.ItemDoesNotExist, _source);
        }

        public void TryAddObjectToCache(string key, TModel model)
        {
            try
            {
                if (_memoryCache != null && model != null)
                {
                    _memoryCache.Set(key, model, _memoryCacheEntryOptions);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }

        public void TryAddObjectToCache(string key, ICollection<TModel> colection)
        {
            try
            {
                if (_memoryCache != null && colection != null && colection.Any())
                {
                    _memoryCache.Set(key, colection, _memoryCacheEntryOptions);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }

        public async Task RemoveByKey(string key)
        {
            try
            {
                await Task.Run(() => _memoryCache.Remove(key));
            }
            catch (Exception e)
            {
                Log.Error(e, _source, e.Message);
            }
        }

        public async Task RemoveByCancelationTokenAsync()
        {
            try
            {
                await Task.Run(() => _cts.Cancel());
            }
            catch (Exception e)
            {
                Log.Error(e, _source, e.Message);
            }
        }
    }
}
