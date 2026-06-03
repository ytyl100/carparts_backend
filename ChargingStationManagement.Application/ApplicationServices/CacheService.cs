using ChargingStationManagement.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ChargingStationManagement.Services.ApplicationServices
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<CacheService> _logger;
        private readonly bool _useRedis;

        public CacheService(
            IMemoryCache memoryCache,
            IConnectionMultiplexer redis,
            ILogger<CacheService> logger,
            IConfiguration configuration)
        {
            _memoryCache = memoryCache;
            _redis = redis;
            _logger = logger;
            _useRedis = configuration.GetValue<bool>("Cache:UseRedis", false);
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            try
            {
                if (_useRedis)
                {
                    var db = _redis.GetDatabase();
                    var cachedValue = await db.StringGetAsync(key);

                    if (!cachedValue.IsNullOrEmpty)
                    {
                        return Deserialize<T>(cachedValue);
                    }

                    var value = await factory();
                    var serializedValue = Serialize(value);

                    await db.StringSetAsync(key, serializedValue, expiration ?? TimeSpan.FromMinutes(30));
                    return value;
                }
                else
                {
                    return await _memoryCache.GetOrCreateAsync(key, async entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30);
                        return await factory();
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrSetAsync for key {Key}", key);
                // 降级：直接调用factory
                return await factory();
            }
        }

        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                if (_useRedis)
                {
                    var db = _redis.GetDatabase();
                    var cachedValue = await db.StringGetAsync(key);

                    if (!cachedValue.IsNullOrEmpty)
                    {
                        return Deserialize<T>(cachedValue);
                    }
                }
                else
                {
                    if (_memoryCache.TryGetValue(key, out T value))
                    {
                        return value;
                    }
                }

                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAsync for key {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                if (_useRedis)
                {
                    var db = _redis.GetDatabase();
                    var serializedValue = Serialize(value);
                    await db.StringSetAsync(key, serializedValue, expiration ?? TimeSpan.FromMinutes(30));
                }
                else
                {
                    _memoryCache.Set(key, value, expiration ?? TimeSpan.FromMinutes(30));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetAsync for key {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                if (_useRedis)
                {
                    var db = _redis.GetDatabase();
                    await db.KeyDeleteAsync(key);
                }
                else
                {
                    _memoryCache.Remove(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RemoveAsync for key {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                if (_useRedis)
                {
                    var db = _redis.GetDatabase();
                    return await db.KeyExistsAsync(key);
                }
                else
                {
                    return _memoryCache.TryGetValue(key, out _);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExistsAsync for key {Key}", key);
                return false;
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                if (_useRedis)
                {
                    var db = _redis.GetDatabase();
                    var endpoints = _redis.GetEndPoints();

                    foreach (var endpoint in endpoints)
                    {
                        var server = _redis.GetServer(endpoint);
                        var keys = server.Keys(pattern: pattern).ToArray();

                        if (keys.Any())
                        {
                            await db.KeyDeleteAsync(keys);
                        }
                    }
                }
                else
                {
                    // MemoryCache不支持模式删除，需要自己维护key列表
                    // 这里简化处理，实际应用中可能需要更复杂的key管理
                    _logger.LogWarning("MemoryCache does not support pattern deletion");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RemoveByPatternAsync for pattern {Pattern}", pattern);
            }
        }

        private string Serialize<T>(T value)
        {
            return System.Text.Json.JsonSerializer.Serialize(value);
        }

        private T Deserialize<T>(string value)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(value);
        }
    }
}

