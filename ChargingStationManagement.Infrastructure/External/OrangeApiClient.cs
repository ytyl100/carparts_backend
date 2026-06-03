// ChargingStationManagement.Infrastructure/External/OrangeApiClient.cs
using ChargingStationManagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ChargingStationManagement.Infrastructure.External
{
    public class OrangeApiClient : IThirdPartyApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ThirdPartyApiSettings _settings;
        private readonly ILogger<OrangeApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public string ProviderName => "Orange";
        public string OperatorId => _settings.OrangeOperatorId;

        public OrangeApiClient(
            HttpClient httpClient,
            IOptions<ThirdPartyApiSettings> settings,
            ILogger<OrangeApiClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            // 配置HttpClient
            _httpClient.BaseAddress = new Uri(_settings.OrangeApiBaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.OrangeApiToken}");
            _httpClient.DefaultRequestHeaders.Add("OperatorID", _settings.OrangeOperatorId);
        }

        public async Task<StationInfoResponse> QueryStationsInfoAsync(QueryStationsInfoRequest request)
        {
            try
            {
                _logger.LogInformation("调用小橘API查询充电站信息: {@Request}", request);

                // 实现具体的API调用逻辑
                // 这里省略具体实现，与GuangqiApiClient类似

                await Task.Delay(100); // 模拟API调用

                // 返回模拟数据
                return new StationInfoResponse
                {
                    ItemSize = 1,
                    PageCount = 1,
                    PageNo = 1,
                    StationInfos = new List<StationInfo>
                    {
                        new StationInfo
                        {
                            StationID = "ORANGE001",
                            StationName = "小橘充电站1",
                            StationStatus = 50,
                            StationLng = 120.123456m,
                            StationLat = 30.123456m,
                            Address = "杭州市西湖区",
                            EquipmentInfos = new List<EquipmentInfo>()
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用小橘API失败");
                throw new ThirdPartyApiException(500, "调用小橘API失败");
            }
        }

        // 实现其他接口方法（简化版）
        public async Task<StationStatusResponse> QueryStationStatusAsync(QueryStationStatusRequest request)
        {
            await Task.Delay(100);
            return new StationStatusResponse
            {
                Total = request.StationIDs?.Count ?? 0,
                StationStatusInfos = new List<StationStatusInfo>()
            };
        }

        public async Task<StationStatsResponse> QueryStationStatsAsync(QueryStationStatsRequest request)
        {
            await Task.Delay(100);
            return new StationStatsResponse
            {
                StationStats = new StationStatsInfo
                {
                    StationID = request.StationID,
                    StationElectricity = 1000.5m
                }
            };
        }

        public async Task<EquipmentAuthResponse> QueryEquipAuthAsync(EquipmentAuthRequest request)
        {
            await Task.Delay(100);
            return new EquipmentAuthResponse
            {
                Ret = 0,
                Msg = "成功",
                Data = new EquipmentAuthData
                {
                    IsAuthenticated = true,
                    ConnectorID = request.ConnectorID
                }
            };
        }

        public async Task<StartChargeResponse> QueryStartChargeAsync(StartChargeRequest request)
        {
            await Task.Delay(100);
            return new StartChargeResponse
            {
                Ret = 0,
                Msg = "成功",
                Data = new StartChargeData
                {
                    StartChargeSeq = request.StartChargeSeq,
                    ConnectorStatus = 3,
                    StartChargeSeqStat = 2
                }
            };
        }

        public async Task<StopChargeResponse> QueryStopChargeAsync(StopChargeRequest request)
        {
            await Task.Delay(100);
            return new StopChargeResponse
            {
                Ret = 0,
                Msg = "成功",
                Data = new StopChargeData
                {
                    TotalAmount = 25.5m,
                    TotalEnergy = 15.3m,
                    TotalDuration = 45.2m,
                    StartChargeSeqStat = 4
                }
            };
        }

        public async Task<ChargeStatusResponse> QueryEquipChargeStatusAsync(ChargeStatusRequest request)
        {
            await Task.Delay(100);
            return new ChargeStatusResponse
            {
                Ret = 0,
                Msg = "成功",
                Data = new ChargeStatusData
                {
                    StartChargeSeq = request.StartChargeSeq,
                    StartChargeSeqStat = 2,
                    ConnectorStatus = 3,
                    TotalPower = 7.5m,
                    TotalElectricity = 12.3m
                }
            };
        }

        // 其他接口方法
        public Task<ThirdPartyResponse<object>> QueryEquipBusinessPolicyAsync(EquipBusinessPolicyRequest request) => Task.FromResult(new ThirdPartyResponse<object> { ReturnCode = 0 });
        public Task<ThirdPartyResponse<object>> NotificationStartChargeResultAsync(StartChargeResultNotification request) => Task.FromResult(new ThirdPartyResponse<object> { ReturnCode = 0 });
        public Task<ThirdPartyResponse<object>> NotificationEquipChargeStatusAsync(ChargeStatusNotification request) => Task.FromResult(new ThirdPartyResponse<object> { ReturnCode = 0 });
        public Task<ThirdPartyResponse<object>> NotificationStopChargeResultAsync(StopChargeResultNotification request) => Task.FromResult(new ThirdPartyResponse<object> { ReturnCode = 0 });
        public Task<ThirdPartyResponse<object>> NotificationChargeOrderInfoAsync(ChargeOrderInfoNotification request) => Task.FromResult(new ThirdPartyResponse<object> { ReturnCode = 0 });
        public Task<ThirdPartyResponse<object>> CheckChargeOrdersAsync(CheckChargeOrdersRequest request) => Task.FromResult(new ThirdPartyResponse<object> { ReturnCode = 0 });
    }
}