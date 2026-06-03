// ChargingStationManagement.Infrastructure/HealthChecks/CacheHealthCheck.cs
using ChargingStationManagement.Infrastructure.Cache;
using ChargingStationManagement.Infrastructure.External;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace ChargingStationManagement.Infrastructure
{
    public class CacheHealthCheck : IHealthCheck
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheHealthCheck> _logger;

        public CacheHealthCheck(
            ICacheService cacheService,
            ILogger<CacheHealthCheck> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 测试缓存服务
                var testKey = $"health_check_{Guid.NewGuid()}";
                var testValue = "test";

                // 写入缓存
                await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(10));

                // 读取缓存
                var cachedValue = await _cacheService.GetAsync<string>(testKey);

                // 删除缓存
                await _cacheService.RemoveAsync(testKey);

                if (cachedValue == testValue)
                {
                    return HealthCheckResult.Healthy("缓存服务正常");
                }
                else
                {
                    return HealthCheckResult.Degraded("缓存服务响应异常");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "缓存健康检查失败");
                return HealthCheckResult.Unhealthy("缓存服务异常", ex);
            }
        }
    }

    public class ExternalApiHealthCheck : IHealthCheck
    {
        private readonly IThirdPartyApiClientFactory _apiClientFactory;
        private readonly ILogger<ExternalApiHealthCheck> _logger;

        public ExternalApiHealthCheck(
            IThirdPartyApiClientFactory apiClientFactory,
            ILogger<ExternalApiHealthCheck> logger)
        {
            _apiClientFactory = apiClientFactory;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var clients = _apiClientFactory.GetAllClients();
                var unhealthyClients = new List<string>();

                foreach (var client in clients)
                {
                    try
                    {
                        // 测试API连通性（使用一个简单的查询）
                        var request = new QueryStationsInfoRequest
                        {
                            PageNo = 1,
                            PageSize = 1
                        };

                        await client.QueryStationsInfoAsync(request);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"第三方API {client.ProviderName} 健康检查失败");
                        unhealthyClients.Add(client.ProviderName);
                    }
                }

                if (unhealthyClients.Count == 0)
                {
                    return HealthCheckResult.Healthy("所有第三方API正常");
                }
                else if (unhealthyClients.Count < clients.Count)
                {
                    return HealthCheckResult.Degraded(
                        $"部分第三方API异常: {string.Join(", ", unhealthyClients)}");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("所有第三方API异常");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "第三方API健康检查失败");
                return HealthCheckResult.Unhealthy("第三方API健康检查异常", ex);
            }
        }
    }
}