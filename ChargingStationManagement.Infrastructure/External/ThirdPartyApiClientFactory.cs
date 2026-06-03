// ChargingStationManagement.Infrastructure/External/ThirdPartyApiClientFactory.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChargingStationManagement.Infrastructure.External
{
    public interface IThirdPartyApiClientFactory
    {
        IThirdPartyApiClient GetClient(string providerName);
        IThirdPartyApiClient GetClientByOperatorId(string operatorId);
        List<IThirdPartyApiClient> GetAllClients();
    }

    public class ThirdPartyApiClientFactory : IThirdPartyApiClientFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ThirdPartyApiClientFactory> _logger;
        private readonly Dictionary<string, Type> _clientTypes;

        public ThirdPartyApiClientFactory(
            IServiceProvider serviceProvider,
            ILogger<ThirdPartyApiClientFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // 注册所有第三方API客户端类型
            _clientTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "Guangqi", typeof(GuangqiApiClient) },
                { "Orange", typeof(OrangeApiClient) },
                { "Tesla", typeof(TeslaApiClient) },
                { "Xiaoju", typeof(XiaojuApiClient) }
            };
        }

        public IThirdPartyApiClient GetClient(string providerName)
        {
            try
            {
                if (_clientTypes.TryGetValue(providerName, out var clientType))
                {
                    return (IThirdPartyApiClient)_serviceProvider.GetRequiredService(clientType);
                }

                _logger.LogWarning($"未找到第三方API客户端: {providerName}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建第三方API客户端失败: {providerName}");
                return null;
            }
        }

        public IThirdPartyApiClient GetClientByOperatorId(string operatorId)
        {
            // 根据运营商ID映射到具体的客户端
            // 这里可以根据配置或数据库查询来确定
            var operatorToProvider = new Dictionary<string, string>
            {
                { "123456789", "Guangqi" },
                { "987654321", "Orange" },
                { "456789123", "Tesla" },
                { "789123456", "Xiaoju" }
            };

            if (operatorToProvider.TryGetValue(operatorId, out var providerName))
            {
                return GetClient(providerName);
            }

            _logger.LogWarning($"未找到运营商ID对应的API客户端: {operatorId}");
            return null;
        }

        public List<IThirdPartyApiClient> GetAllClients()
        {
            var clients = new List<IThirdPartyApiClient>();

            foreach (var clientType in _clientTypes.Values)
            {
                try
                {
                    var client = (IThirdPartyApiClient)_serviceProvider.GetRequiredService(clientType);
                    clients.Add(client);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"创建客户端失败: {clientType.Name}");
                }
            }

            return clients;
        }
    }
}