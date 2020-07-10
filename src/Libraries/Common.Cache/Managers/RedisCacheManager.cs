using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logic.Enums;
using Common.Logic.Helpers;
using Common.Logic.Infrastructure;
using Common.Logic.Infrastructure.Interfaces;
using Common.Cache.Configuration;
using Common.Cache.Enums;
using Microsoft.Extensions.Configuration;
using Serilog;
using StackExchange.Redis;

namespace Common.Cache.Managers
{
    public class RedisCacheManager<TModel> where TModel : class
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly string _source;
        private readonly string _keyPrefix;
        private static long _lastUnhealthTime;

        public RedisCacheManager(IConfigurationRoot configuration)
        {
            _source = $"{GetType().Name}({typeof(TModel).Name})";
            var conf = ConnectionMultiplexerConfigs.CreateConfigs(configuration);

            if (bool.TryParse(configuration["RedisCache:Enabled"], out var enabled)
                && enabled
                && ConnectionMultiplexerConfigs.Health != RedisCacheHealthStatus.Unhealth)
            {
                try
                {
                    _connectionMultiplexer = ConnectionMultiplexer.Connect(conf.Value);
                    _keyPrefix = $"{_connectionMultiplexer.ClientName}:{typeof(TModel).Name}";

                    ConnectionMultiplexerConfigs.SetHealthy();
                    CheckLastUnhealthTime();
                }
                catch (RedisConnectionException rcex)
                {
                    ConnectionMultiplexerConfigs.SetUnHealthy();
                    Log.Error($"Could not connect to redis:{rcex.Message}");
                }
                catch (Exception e)
                {
                    Log.Error(e, _source, e.Message);
                }
            }
        }

        public async Task<IOperationResult<ICollection<TModel>>> TryGetCollection(string key, bool fromHash = false)
        {
            try
            {
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                {
                    var db = _connectionMultiplexer.GetDatabase();

                    var getResult = fromHash
                        ? await db.HashGetAsync(_keyPrefix, key)
                        : await db.StringGetAsync($"{_keyPrefix}:{key}");

                    if (getResult.HasValue)
                    {
                        var found = JsonHelper.Deserialize<ICollection<TModel>>(getResult);
                        return new OperationResult<ICollection<TModel>>(found);
                    }
                }
            }
            catch (RedisConnectionException rcex)
            {
                ConnectionMultiplexerConfigs.SetUnHealthy();
                Log.Error($"Could not connect to redis:{rcex.Message}");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return new OperationResult<ICollection<TModel>>(ErrorType.UnknownError, e);
            }

            return new OperationResult<ICollection<TModel>>(ErrorType.ItemDoesNotExist, _source);
        }

        public async Task<IOperationResult<TModel>> TryGetObject(string key, bool fromHash = false)
        {
            try
            {
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                {
                    var db = _connectionMultiplexer.GetDatabase();
                    var getResult = fromHash
                        ? await db.HashGetAsync(_keyPrefix, key)
                        : await db.StringGetAsync($"{_keyPrefix}:{key}");

                    if (getResult.HasValue)
                    {
                        var found = JsonHelper.Deserialize<TModel>(getResult);
                        return new OperationResult<TModel>(found);
                    }
                }
            }
            catch (RedisConnectionException rcex)
            {
                ConnectionMultiplexerConfigs.SetUnHealthy();
                Log.Error($"Could not connect to redis:{rcex.Message}");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return new OperationResult<TModel>(ErrorType.UnknownError, e);
            }
            return new OperationResult<TModel>(ErrorType.ItemDoesNotExist, _source);
        }

        public async Task TryAddObjectToCache(string key, TModel model, bool toHash = false)
        {
            try
            {
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                {
                    var db = _connectionMultiplexer.GetDatabase();

                    var json = JsonHelper.Serialize(model);

                    var setResult = toHash
                        ? await db.HashSetAsync(_keyPrefix, key, json)
                        : await db.StringSetAsync($"{_keyPrefix}:{key}", json, TimeSpan.FromSeconds(ConnectionMultiplexerConfigs.AbsoluteExpirationRelativeToNow));

                    if (!setResult)
                    {
                        var isExist = toHash
                            ? await db.HashExistsAsync(_keyPrefix, key)
                            : await db.KeyExistsAsync($"{_keyPrefix}:{key}");
                        if (!isExist)
                        {
                            var info = toHash
                                ? $"Object with key:{key} was not recorded to hash {_keyPrefix}"
                                : $"Object with key:{_keyPrefix}:{key} was not recorded";
                            Log.Information(info);
                        }
                    }
                }
            }
            catch (RedisConnectionException rcex)
            {
                ConnectionMultiplexerConfigs.SetUnHealthy();
                Log.Error($"Could not connect to redis:{rcex.Message}");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }

        public async Task TryAddObjectToCache(string key, ICollection<TModel> colection, bool toHash = false)
        {
            try
            {
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                {
                    var db = _connectionMultiplexer.GetDatabase();
                    var json = JsonHelper.Serialize(colection);
                    var setResult = toHash
                        ? await db.HashSetAsync(_keyPrefix, key, json)
                        : await db.StringSetAsync($"{_keyPrefix}:{key}", json, TimeSpan.FromSeconds(ConnectionMultiplexerConfigs.AbsoluteExpirationRelativeToNow));

                    if (!setResult)
                    {
                        var isExist = toHash
                            ? await db.HashExistsAsync(_keyPrefix, key)
                            : await db.KeyExistsAsync($"{_keyPrefix}:{key}");
                        if (!isExist)
                        {
                            var info = toHash
                                ? $"Collection with key:{key} was not recorded to hash {_keyPrefix}"
                                : $"Collection with key:{_keyPrefix}:{key} was not recorded";
                            Log.Information(info);
                        }
                    }
                }
            }
            catch (RedisConnectionException rcex)
            {
                ConnectionMultiplexerConfigs.SetUnHealthy();
                Log.Error($"Could not connect to redis:{rcex.Message}");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
        }

        //public async Task RemoveByDefaulteHashNameAsync()
        //{
        //    try
        //    {
        //        if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
        //        {
        //            var db = _connectionMultiplexer.GetDatabase();
        //            var deleteRes = await db.KeyDeleteAsync(_keyPrefix);

        //            if (!deleteRes)
        //            {
        //                var isExist = await db.KeyExistsAsync(_keyPrefix);
        //                if(isExist)
        //                    Log.Information($"Object with key:{_keyPrefix} was not deleted!");
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error(e, _source, e.Message);
        //    }
        //}

        public async Task RemoveByKey(string key = null)
        {
            var keyForDelete = string.IsNullOrEmpty(key)
                ? _keyPrefix
                : $"{_keyPrefix}:{key}";

            try
            {
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                {
                    var db = _connectionMultiplexer.GetDatabase();
                    var deleteRes = await db.KeyDeleteAsync(keyForDelete);

                    if (!deleteRes)
                    {
                        var isExist = await db.KeyExistsAsync(keyForDelete);
                        if (isExist)
                            Log.Information($"Key:{keyForDelete} was not deleted!");
                    }
                }
            }
            catch (RedisConnectionException rcex)
            {
                ConnectionMultiplexerConfigs.SetUnHealthy();
                Log.Error($"Could not connect to redis:{rcex.Message}");
            }
            catch (Exception e)
            {
                Log.Error(e, _source, e.Message);
            }
        }


        //todo: fix error with build script
        public async Task RemoveByKeyPrefixAsync()
        {
            throw new NotImplementedException();

            //try
            //{
            //    if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
            //    {
            //        var db = _connectionMultiplexer.GetDatabase();

            //        var script =
            //            $"EVAL \"return redis.call('del', unpack(redis.call('keys', ARGV[1])))\" 0 {_keyPrefix}:*";

            //        var deleteRes = await db.ScriptEvaluateAsync(script);

            //        if (string.IsNullOrEmpty(deleteRes.ToString()))
            //        {
            //            Log.Information($"Object with prefix:{_keyPrefix} was not deleted!");
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Log.Error(e, _source, e.Message);
            //}
        }

        #region Private

        private async void CheckLastUnhealthTime()
        {
            if (_lastUnhealthTime != ConnectionMultiplexerConfigs.LastUnhealthTime)
            {
                await RemoveByKey();
                _lastUnhealthTime = ConnectionMultiplexerConfigs.LastUnhealthTime;
            }
        }

        #endregion

    }
}
