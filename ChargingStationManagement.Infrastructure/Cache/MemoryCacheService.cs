// ChargingStationManagement.Infrastructure/Cache/MemoryCacheService.cs
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ChargingStationManagement.Infrastructure.Cache
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);
    }

    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public MemoryCacheService(
            IMemoryCache memoryCache,
            ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public Task<T> GetAsync<T>(string key)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T cachedValue))
                {
                    _logger.LogDebug($"从内存缓存获取数据: {key}");
                    return Task.FromResult(cachedValue);
                }

                return Task.FromResult(default(T));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"从内存缓存获取数据失败: {key}");
                return Task.FromResult(default(T));
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    Size = 1 // 设置大小限制
                };

                if (expiry.HasValue)
                {
                    cacheEntryOptions.SetAbsoluteExpiration(expiry.Value);
                }
                else
                {
                    // 默认缓存时间
                    cacheEntryOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                }

                _memoryCache.Set(key, value, cacheEntryOptions);
                _logger.LogDebug($"设置内存缓存: {key}, 过期时间: {expiry?.ToString() ?? "5分钟"}");

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"设置内存缓存失败: {key}");
                return Task.CompletedTask;
            }
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _logger.LogDebug($"删除内存缓存: {key}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除内存缓存失败: {key}");
                return Task.CompletedTask;
            }
        }

        public Task<bool> ExistsAsync(string key)
        {
            try
            {
                var exists = _memoryCache.TryGetValue(key, out _);
                return Task.FromResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"检查内存缓存失败: {key}");
                return Task.FromResult(false);
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            try
            {
                // 尝试从缓存获取
                if (_memoryCache.TryGetValue(key, out T cachedValue))
                {
                    _logger.LogDebug($"从内存缓存获取数据: {key}");
                    return cachedValue;
                }

                // 调用工厂方法创建数据
                _logger.LogDebug($"缓存未命中，创建新数据: {key}");
                var value = await factory();

                if (value != null)
                {
                    // 设置缓存
                    var cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        Size = 1
                    };

                    if (expiry.HasValue)
                    {
                        cacheEntryOptions.SetAbsoluteExpiration(expiry.Value);
                    }
                    else
                    {
                        cacheEntryOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                    }

                    _memoryCache.Set(key, value, cacheEntryOptions);
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取或创建缓存失败: {key}");
                return await factory();
            }
        }

        public Task ClearByPatternAsync(string pattern)
        {
            try
            {
                // 注意：MemoryCache不支持模式删除，这里只是简单示例
                // 实际应用中可能需要使用分布式缓存如Redis
                _logger.LogWarning($"MemoryCache不支持模式删除: {pattern}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"清除模式缓存失败: {pattern}");
                return Task.CompletedTask;
            }
        }

        public Task<long> IncrementAsync(string key, long value = 1)
        {
            try
            {
                var current = _memoryCache.GetOrCreate(key, entry => 0L);
                var newValue = current + value;
                _memoryCache.Set(key, newValue);

                return Task.FromResult(newValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"递增缓存值失败: {key}");
                return Task.FromResult(0L);
            }
        }

        public Task<long> DecrementAsync(string key, long value = 1)
        {
            try
            {
                var current = _memoryCache.GetOrCreate(key, entry => 0L);
                var newValue = Math.Max(0, current - value);
                _memoryCache.Set(key, newValue);

                return Task.FromResult(newValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"递减缓存值失败: {key}");
                return Task.FromResult(0L);
            }
        }
    }
}