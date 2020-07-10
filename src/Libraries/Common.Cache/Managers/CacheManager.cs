using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Interfaces;
using Common.Logic.Enums;
using Common.Logic.Infrastructure;
using Common.Logic.Infrastructure.Interfaces;
using Common.Cache.Extensions;
using Common.Cache.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Common.Cache.Managers
{
    public class CacheManager<TModel> : ICacheManager where TModel : class
    {
        private readonly string _source;
        private readonly bool _toHashSet;
        private readonly RedisCacheManager<TModel> _redisCacheManager;
        private readonly InMemoryCache<TModel> _inMemoryCache;

        public CacheManager(IMemoryCache memoryCache, IConfigurationRoot configuration)
        {
            if (bool.TryParse(configuration["RedisCache:Enabled"], out var redisCacheEnabled) &&
                redisCacheEnabled)
            {
                _redisCacheManager = new RedisCacheManager<TModel>(configuration);
            }

            if (bool.TryParse(configuration["InMemoryCache:Enabled"], out var inMemoryCacheEnabled) &&
                inMemoryCacheEnabled)
            {
                _inMemoryCache = new InMemoryCache<TModel>(memoryCache, configuration);
            }
            _source = $"{GetType().Name}({typeof(TModel).Name})";
            _toHashSet = configuration.GetValue("RedisCache:ToHashSet", true);
        }

        public async Task<IOperationResult<ICollection<TModel>>> GetCollection(
            Func<Func<TModel, bool>, int, int, Task<IDbOperationResult<ICollection<TModel>>>> func,
            Func<TModel, bool> predicate,
            int skip,
            int take)
        {
            var key = predicate.GetBodyHashCode(skip, take);

            var getFromCachResult = await GetCollectionFromCache(key);
            if (getFromCachResult.IsSuccess)
            {
                return new OperationResult<ICollection<TModel>>(getFromCachResult.Data);
            }

            var resaltSearchInDB = await func(predicate, skip, take);
            if (!resaltSearchInDB.IsSuccess)
            {
                return new OperationResult<ICollection<TModel>>(ErrorType.DbExceptionError, resaltSearchInDB.Error.ErrorSource, resaltSearchInDB.Error.ErrorDescription);
            }
            if (resaltSearchInDB.Data.Any())
                await AddCollectionModelsToCache(key, resaltSearchInDB.Data);
            return new OperationResult<ICollection<TModel>>(resaltSearchInDB.Data);
        }

        public async Task<IOperationResult<IEnumerable<TModel>>> GetCollectionlWithSkipTake(
            Func<Func<TModel, bool>, int, int, Task<IEnumerable<TModel>>> func,
            Func<TModel, bool> predicate,
            int skip,
            int take)
        {
            var key = predicate.GetBodyHashCode(skip, take);

            var getFromCachResult = await GetCollectionFromCache(key);
            if (getFromCachResult.IsSuccess)
            {
                return new OperationResult<IEnumerable<TModel>>(getFromCachResult.Data);
            }

            var resaltSearchInDb = await func(predicate, skip, take);
            if (resaltSearchInDb.Any())
            {
                await AddCollectionModelsToCache(key, resaltSearchInDb.ToList());
            }
            return new OperationResult<IEnumerable<TModel>>(resaltSearchInDb);
        }

        public async Task<IOperationResult<IEnumerable<TModel>>> GetCollectionlWithSkipTakeAsynk(
            Func<string, int, int, Task<IEnumerable<TModel>>> func,
            string property,
            int skip,
            int take)
        {
            var key = func.GetBodyHashCode(skip, take, property);

            var getFromCachResult = await GetCollectionFromCache(key);
            if (getFromCachResult.IsSuccess)
            {
                return new OperationResult<IEnumerable<TModel>>(getFromCachResult.Data);
            }

            var resaltSearchInDb = await func(property, skip, take);
            if (resaltSearchInDb.Any())
            {
                await AddCollectionModelsToCache(key, resaltSearchInDb.ToList());
            }
                return new OperationResult<IEnumerable<TModel>>(resaltSearchInDb);
        }

        public async Task<IOperationResult<TModel>> GetModelWithSkipTake(
            Func<Func<TModel, bool>, int, int, Task<TModel>> func,
            Func<TModel, bool> predicate,
            int skip,
            int take)
        {
            var key = predicate.GetBodyHashCode(skip, take);

            var getFromCachResult = await GetObjectFromCache(key);
            if (getFromCachResult.IsSuccess)
            {
                return new OperationResult<TModel>(getFromCachResult.Data);
            }

            var resaltSearchInDB = await func(predicate, skip, take);
            if (resaltSearchInDB != null)
            {
                await AddModelToCache(key, resaltSearchInDB);
                return new OperationResult<TModel>(resaltSearchInDB);
            }
            return new OperationResult<TModel>(ErrorType.ItemDoesNotExist, _source);
        }

        public async Task<IOperationResult<ICollection<TModel>>> GetDbOperationResaltCollection(
            Func<Func<TModel, bool>, Task<IDbOperationResult<ICollection<TModel>>>> func,
            Func<TModel, bool> predicate)
        {
            var key = predicate.GetBodyHashCode();

            var getFromCachResult = await GetCollectionFromCache(key);
            if (getFromCachResult.IsSuccess)
            {
                return new OperationResult<ICollection<TModel>>(getFromCachResult.Data);
            }

            var resaltSearchInDB = await func(predicate);
            if (!resaltSearchInDB.IsSuccess || resaltSearchInDB.Data.Any())
            {
                return new OperationResult<ICollection<TModel>>(ErrorType.DbExceptionError, resaltSearchInDB.Error.ErrorSource, resaltSearchInDB.Error.ErrorDescription);
            }
            if(resaltSearchInDB.Data.Any())
                await AddCollectionModelsToCache(key, resaltSearchInDB.Data);
            return new OperationResult<ICollection<TModel>>(resaltSearchInDB.Data);
        }


        public async Task<IOperationResult<ICollection<TModel>>> GetCollection(
            Func<Func<TModel, bool>, ICollection<TModel>> func,
            Func<TModel, bool> predicate)
        {
            var key = predicate.GetBodyHashCode();

            var getFromCachResult = await GetCollectionFromCache(key);
            if (getFromCachResult.IsSuccess)
            {
                return new OperationResult<ICollection<TModel>>(getFromCachResult.Data);
            }

            var resaltSearchInDB = func(predicate);
            if (resaltSearchInDB.Any())
            {
                await AddCollectionModelsToCache(key, resaltSearchInDB);
            }
            return new OperationResult<ICollection<TModel>>(resaltSearchInDB);
        }


        public async Task<IOperationResult<ICollection<TModel>>> GetCollectionAsync(
            Func<Func<TModel, bool>, Task<ICollection<TModel>>> func,
            Func<TModel, bool> predicate)
        {
            var key = predicate.GetBodyHashCode();

            var getFromCachResult = await GetCollectionFromCache(key);
            if (getFromCachResult.IsSuccess)
            {
                return new OperationResult<ICollection<TModel>>(getFromCachResult.Data);
            }

            var resaltSearchInDB = await func(predicate);
            if (resaltSearchInDB.Any())
            {
                await AddCollectionModelsToCache(key, resaltSearchInDB);
            }
            return new OperationResult<ICollection<TModel>>(resaltSearchInDB, resaltSearchInDB.Count);
        }

        public async Task<IOperationResult<TModel>> Get(
            Func<Func<TModel, bool>, Task<TModel>> func,
            Func<TModel, bool> predicate)
        {
            var key = predicate.GetBodyHashCode();

            var getFromCachResult = await GetObjectFromCache(key);
            if (getFromCachResult.IsSuccess)
            {
                return new OperationResult<TModel>(getFromCachResult.Data);
            }

            var resaltSearchInDB = await func(predicate);
            if (resaltSearchInDB != null)
            {
                await AddModelToCache(key, resaltSearchInDB);
                return new OperationResult<TModel>(resaltSearchInDB);
            }
            return new OperationResult<TModel>(ErrorType.ItemDoesNotExist, _source);
        }


        public async Task<IOperationResult<TModel>> Get(
            Func<string, Task<TModel>> func,
            string parameter)
        {
            var key = func.GetBodyHashCode(param:parameter);

            var getFromCachResult = await GetObjectFromCache(key);
            if (getFromCachResult.IsSuccess)
            {
                return new OperationResult<TModel>(getFromCachResult.Data);
            }

            var resaltSearchInDB = await func(parameter);
            if (resaltSearchInDB != null)
            {
                await AddModelToCache(key, resaltSearchInDB);
                return new OperationResult<TModel>(resaltSearchInDB);
            }
            return new OperationResult<TModel>(ErrorType.ItemDoesNotExist, _source);
        }


        public async Task<IOperationResult<ICollection<TModel>>> GetCollection(
            Func<string, Task<ICollection<TModel>>> func,
            string parameter)
        {
            var key = func.GetBodyHashCode(param: parameter);

            var getFromCachResult = await GetCollectionFromCache(key);
            if (getFromCachResult.IsSuccess)
            {
                return new OperationResult<ICollection<TModel>>(getFromCachResult.Data);
            }

            var resaltSearchInDB = await func(parameter);
            if (resaltSearchInDB != null)
            {
                await AddCollectionModelsToCache(key, resaltSearchInDB);
                return new OperationResult<ICollection<TModel>>(resaltSearchInDB);
            }
            return new OperationResult<ICollection<TModel>>(ErrorType.ItemDoesNotExist, _source);
        }


        public async Task<IOperationResult<TModel>> GetObjectFromCache(string callerName, string key)
        {
            key = $"{callerName}_Object_{typeof(TModel)}_{key}";
            return await GetObjectFromCache(key);
        }

        public async Task<IOperationResult<ICollection<TModel>>> GetCollectionFromCache(string callerName, string key)
        {
            key = $"{callerName}_Collection_{typeof(TModel)}_{key}";
            return await GetCollectionFromCache(key);
        }

        public async Task AddModelToCache(string callerName, string key, TModel model)
        {
            key = $"{callerName}_Object_{typeof(TModel)}_{key}";
            await AddModelToCache(key, model);
        }

        public async Task AddCollectionModelsToCache(string callerName, string key, ICollection<TModel> model)
        {
            key = $"{callerName}_Collection_{typeof(TModel)}_{key}";
            await AddCollectionModelsToCache(key, model);
        }

        public async Task RemoveAllHash()
        {
            if (_inMemoryCache != null)
            {
                await _inMemoryCache.RemoveByCancelationTokenAsync();

            }
            if (_redisCacheManager != null)
            {
                await _redisCacheManager.RemoveByKey();
            }
        }

        #region Private

        private async Task<IOperationResult<ICollection<TModel>>> GetCollectionFromCache(string key)
        {
            if (_inMemoryCache != null)
            {
                var resaltSearchInMemoryCache = await _inMemoryCache.TryGetCollection(key);
                if (resaltSearchInMemoryCache.IsSuccess)
                {
                    return new OperationResult<ICollection<TModel>>(resaltSearchInMemoryCache.Data);
                }
            }

            if (_redisCacheManager != null)
            {
                var resaltSearchInRedisCache = await _redisCacheManager.TryGetCollection(key, _toHashSet);
                if (resaltSearchInRedisCache.IsSuccess)
                {
                    _inMemoryCache?.TryAddObjectToCache(key, resaltSearchInRedisCache.Data);
                    return new OperationResult<ICollection<TModel>>(resaltSearchInRedisCache.Data);
                }
            }

            return new OperationResult<ICollection<TModel>>(ErrorType.ItemDoesNotExist, _source);
        }

        private async Task<IOperationResult<TModel>> GetObjectFromCache(string key)
        {
            if (_inMemoryCache != null)
            {
                var resaltSearchInMemoryCache = await _inMemoryCache.TryGetObject(key);
                if (resaltSearchInMemoryCache.IsSuccess)
                {
                    return new OperationResult<TModel>(resaltSearchInMemoryCache.Data);
                }
            }

            if (_redisCacheManager != null)
            {
                var resaltSearchInRedisCache = await _redisCacheManager.TryGetObject(key, _toHashSet);
                if (resaltSearchInRedisCache.IsSuccess)
                {
                    _inMemoryCache?.TryAddObjectToCache(key, resaltSearchInRedisCache.Data);
                    return new OperationResult<TModel>(resaltSearchInRedisCache.Data);
                }
            }
            return new OperationResult<TModel>(ErrorType.ItemDoesNotExist, _source);
        }

        private async Task AddModelToCache(string key, TModel model)
        {
            if (model != null)
            {
                _inMemoryCache?.TryAddObjectToCache(key, model);

                if (_redisCacheManager != null)
                    await _redisCacheManager.TryAddObjectToCache(key, model, _toHashSet);
            }
        }

        private async Task AddCollectionModelsToCache(string key, ICollection<TModel> model)
        {
            if (model != null && model.Any())
            {
                _inMemoryCache?.TryAddObjectToCache(key, model);

                if (_redisCacheManager != null)
                    await _redisCacheManager.TryAddObjectToCache(key, model, _toHashSet);
            }
        }

        #endregion
    }
}
