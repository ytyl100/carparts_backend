// ChargingStationManagement.Infrastructure/External/GuangqiApiClient.cs
using ChargingStationManagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ChargingStationManagement.Infrastructure.External
{
    public class GuangqiApiClient : IThirdPartyApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ThirdPartyApiSettings _settings;
        private readonly ILogger<GuangqiApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public string ProviderName => "Guangqi";
        public string OperatorId => _settings.GuangqiOperatorId;

        public GuangqiApiClient(
            HttpClient httpClient,
            IOptions<ThirdPartyApiSettings> settings,
            ILogger<GuangqiApiClient> logger)
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
            _httpClient.BaseAddress = new Uri(_settings.GuangqiApiBaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.GuangqiApiToken}");
            _httpClient.DefaultRequestHeaders.Add("OperatorID", _settings.GuangqiOperatorId);
        }

        public async Task<StationInfoResponse> QueryStationsInfoAsync(QueryStationsInfoRequest request)
        {
            try
            {
                _logger.LogInformation("调用广汽API查询充电站信息: {@Request}", request);

                var apiRequest = new
                {
                    LastQueryTime = request.LastQueryTime,
                    PageNo = request.PageNo,
                    PageSize = request.PageSize
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(apiRequest, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("query_stations_info", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ThirdPartyResponse<StationInfoResponse>>(responseString, _jsonOptions);

                if (result.IsSuccess)
                {
                    return result.Data;
                }
                else
                {
                    _logger.LogError($"广汽API查询失败: {result.Message} (Code: {result.ReturnCode})");
                    throw new ThirdPartyApiException(result.ReturnCode, result.Message);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "调用广汽API网络错误");
                throw new ThirdPartyApiException(500, "网络请求失败");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "解析广汽API响应失败");
                throw new ThirdPartyApiException(500, "响应解析失败");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用广汽API未知错误");
                throw new ThirdPartyApiException(500, "未知错误");
            }
        }

        public async Task<StationStatusResponse> QueryStationStatusAsync(QueryStationStatusRequest request)
        {
            try
            {
                _logger.LogInformation("调用广汽API查询充电站状态: {@Request}", request);

                if (request.StationIDs == null || request.StationIDs.Count == 0)
                {
                    throw new ArgumentException("StationIDs不能为空");
                }

                if (request.StationIDs.Count > 50)
                {
                    throw new ArgumentException("StationIDs数量不能超过50个");
                }

                var apiRequest = new
                {
                    StationIDs = request.StationIDs
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(apiRequest, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("query_station_status", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ThirdPartyResponse<StationStatusResponse>>(responseString, _jsonOptions);

                if (result.IsSuccess)
                {
                    return result.Data;
                }
                else
                {
                    _logger.LogError($"广汽API查询状态失败: {result.Message} (Code: {result.ReturnCode})");
                    throw new ThirdPartyApiException(result.ReturnCode, result.Message);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "调用广汽API网络错误");
                throw new ThirdPartyApiException(500, "网络请求失败");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用广汽API查询状态失败");
                throw new ThirdPartyApiException(500, "未知错误");
            }
        }

        public async Task<StationStatsResponse> QueryStationStatsAsync(QueryStationStatsRequest request)
        {
            try
            {
                _logger.LogInformation("调用广汽API查询充电站统计: {@Request}", request);

                var apiRequest = new
                {
                    StationID = request.StationID,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(apiRequest, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("query_station_stats", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ThirdPartyResponse<StationStatsResponse>>(responseString, _jsonOptions);

                if (result.IsSuccess)
                {
                    return result.Data;
                }
                else
                {
                    _logger.LogError($"广汽API查询统计失败: {result.Message} (Code: {result.ReturnCode})");
                    throw new ThirdPartyApiException(result.ReturnCode, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用广汽API查询统计失败");
                throw new ThirdPartyApiException(500, "未知错误");
            }
        }

        public async Task<EquipmentAuthResponse> QueryEquipAuthAsync(EquipmentAuthRequest request)
        {
            try
            {
                _logger.LogInformation("调用广汽API设备认证: {@Request}", request);

                var apiRequest = new
                {
                    EquipAuthSeq = request.EquipAuthSeq,
                    ConnectorID = request.ConnectorID
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(apiRequest, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("query_equip_auth", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<EquipmentAuthResponse>(responseString, _jsonOptions);

                if (result.Ret == 0)
                {
                    return result;
                }
                else
                {
                    _logger.LogError($"广汽API设备认证失败: {result.Msg} (Code: {result.Ret})");
                    throw new ThirdPartyApiException(result.Ret, result.Msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用广汽API设备认证失败");
                throw new ThirdPartyApiException(500, "未知错误");
            }
        }

        public async Task<StartChargeResponse> QueryStartChargeAsync(StartChargeRequest request)
        {
            try
            {
                _logger.LogInformation("调用广汽API启动充电: {@Request}", request);

                var apiRequest = new
                {
                    StartChargeSeq = request.StartChargeSeq,
                    ConnectorID = request.ConnectorID,
                    QRCode = request.QRCode
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(apiRequest, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("query_start_charge", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<StartChargeResponse>(responseString, _jsonOptions);

                if (result.Ret == 0)
                {
                    return result;
                }
                else
                {
                    _logger.LogError($"广汽API启动充电失败: {result.Msg} (Code: {result.Ret})");
                    throw new ThirdPartyApiException(result.Ret, result.Msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用广汽API启动充电失败");
                throw new ThirdPartyApiException(500, "未知错误");
            }
        }

        public async Task<StopChargeResponse> QueryStopChargeAsync(StopChargeRequest request)
        {
            try
            {
                _logger.LogInformation("调用广汽API停止充电: {@Request}", request);

                var apiRequest = new
                {
                    StartChargeSeq = request.StartChargeSeq,
                    ConnectorID = request.ConnectorID
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(apiRequest, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("query_stop_charge", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<StopChargeResponse>(responseString, _jsonOptions);

                if (result.Ret == 0)
                {
                    return result;
                }
                else
                {
                    _logger.LogError($"广汽API停止充电失败: {result.Msg} (Code: {result.Ret})");
                    throw new ThirdPartyApiException(result.Ret, result.Msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用广汽API停止充电失败");
                throw new ThirdPartyApiException(500, "未知错误");
            }
        }

        public async Task<ChargeStatusResponse> QueryEquipChargeStatusAsync(ChargeStatusRequest request)
        {
            try
            {
                _logger.LogInformation("调用广汽API查询充电状态: {@Request}", request);

                var apiRequest = new
                {
                    StartChargeSeq = request.StartChargeSeq
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(apiRequest, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("query_equip_charge_status", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ChargeStatusResponse>(responseString, _jsonOptions);

                if (result.Ret == 0)
                {
                    return result;
                }
                else
                {
                    _logger.LogError($"广汽API查询充电状态失败: {result.Msg} (Code: {result.Ret})");
                    throw new ThirdPartyApiException(result.Ret, result.Msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用广汽API查询充电状态失败");
                throw new ThirdPartyApiException(500, "未知错误");
            }
        }

        // 实现其他接口方法（简化版）
        public async Task<ThirdPartyResponse<object>> QueryEquipBusinessPolicyAsync(EquipBusinessPolicyRequest request)
        {
            // 实现业务策略查询
            await Task.Delay(100); // 模拟API调用
            return new ThirdPartyResponse<object> { ReturnCode = 0, Message = "成功" };
        }

        public async Task<ThirdPartyResponse<object>> NotificationStartChargeResultAsync(StartChargeResultNotification request)
        {
            // 实现启动充电结果推送
            await Task.Delay(100);
            return new ThirdPartyResponse<object> { ReturnCode = 0, Message = "成功" };
        }

        public async Task<ThirdPartyResponse<object>> NotificationEquipChargeStatusAsync(ChargeStatusNotification request)
        {
            // 实现充电状态推送
            await Task.Delay(100);
            return new ThirdPartyResponse<object> { ReturnCode = 0, Message = "成功" };
        }

        public async Task<ThirdPartyResponse<object>> NotificationStopChargeResultAsync(StopChargeResultNotification request)
        {
            // 实现停止充电结果推送
            await Task.Delay(100);
            return new ThirdPartyResponse<object> { ReturnCode = 0, Message = "成功" };
        }

        public async Task<ThirdPartyResponse<object>> NotificationChargeOrderInfoAsync(ChargeOrderInfoNotification request)
        {
            // 实现充电订单信息推送
            await Task.Delay(100);
            return new ThirdPartyResponse<object> { ReturnCode = 0, Message = "成功" };
        }

        public async Task<ThirdPartyResponse<object>> CheckChargeOrdersAsync(CheckChargeOrdersRequest request)
        {
            // 实现订单对账
            await Task.Delay(100);
            return new ThirdPartyResponse<object> { ReturnCode = 0, Message = "成功" };
        }
    }
}