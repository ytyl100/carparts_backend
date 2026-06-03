// ChargingStationManagement.Infrastructure/External/XiaojuApiClient.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChargingStationManagement.Infrastructure.External
{
    public class XiaojuApiClient : IThirdPartyApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ThirdPartyApiSettings _settings;
        private readonly ILogger<XiaojuApiClient> _logger;

        public string ProviderName => "Xiaoju";
        public string OperatorId => _settings.XiaojuOperatorId;

        public XiaojuApiClient(
            HttpClient httpClient,
            IOptions<ThirdPartyApiSettings> settings,
            ILogger<XiaojuApiClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        // 实现所有接口方法（简化版）
        public Task<StationInfoResponse> QueryStationsInfoAsync(QueryStationsInfoRequest request)
            => Task.FromResult(new StationInfoResponse());

        public Task<StationStatusResponse> QueryStationStatusAsync(QueryStationStatusRequest request)
            => Task.FromResult(new StationStatusResponse());

        public Task<StationStatsResponse> QueryStationStatsAsync(QueryStationStatsRequest request)
            => Task.FromResult(new StationStatsResponse());

        public Task<EquipmentAuthResponse> QueryEquipAuthAsync(EquipmentAuthRequest request)
            => Task.FromResult(new EquipmentAuthResponse());

        public Task<StartChargeResponse> QueryStartChargeAsync(StartChargeRequest request)
            => Task.FromResult(new StartChargeResponse());

        public Task<StopChargeResponse> QueryStopChargeAsync(StopChargeRequest request)
            => Task.FromResult(new StopChargeResponse());

        public Task<ChargeStatusResponse> QueryEquipChargeStatusAsync(ChargeStatusRequest request)
            => Task.FromResult(new ChargeStatusResponse());

        public Task<ThirdPartyResponse<object>> QueryEquipBusinessPolicyAsync(EquipBusinessPolicyRequest request)
            => Task.FromResult(new ThirdPartyResponse<object> { ReturnCode = 0 });

        public Task<ThirdPartyResponse<object>> NotificationStartChargeResultAsync(StartChargeResultNotification request)
            => Task.FromResult(new ThirdPartyResponse<object> { ReturnCode = 0 });

        public Task<ThirdPartyResponse<object>> NotificationEquipChargeStatusAsync(ChargeStatusNotification request)
            => Task.FromResult(new ThirdPartyResponse<object> { ReturnCode = 0 });

        public Task<ThirdPartyResponse<object>> NotificationStopChargeResultAsync(StopChargeResultNotification request)
            => Task.FromResult(new ThirdPartyResponse<object> { ReturnCode = 0 });

        public Task<ThirdPartyResponse<object>> NotificationChargeOrderInfoAsync(ChargeOrderInfoNotification request)
            => Task.FromResult(new ThirdPartyResponse<object> { ReturnCode = 0 });

        public Task<ThirdPartyResponse<object>> CheckChargeOrdersAsync(CheckChargeOrdersRequest request)
            => Task.FromResult(new ThirdPartyResponse<object> { ReturnCode = 0 });
    }
}