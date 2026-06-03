using ChargingStationManagement.Domain.Interfaces;
using ChargingStationManagement.Services.DTOs;
using ChargingStationManagement.Services.DTOs.ThirdParty;
using ChargingStationManagement.Services.Interfaces;
using ChargingStationManagement.Services.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChargingStationManagement.Services.ApplicationServices
{
    public class ThirdPartyIntegrationService : IApiThirdPartyIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ThirdPartyIntegrationService> _logger;
        private readonly IMemoryCache _cache;
        private readonly JsonSerializerOptions _jsonOptions;

        private readonly Dictionary<string, ThirdPartyConfig> _thirdPartyConfigs;

        public ThirdPartyIntegrationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ThirdPartyIntegrationService> logger,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // 加载第三方配置
            _thirdPartyConfigs = LoadThirdPartyConfigs();
        }

        private Dictionary<string, ThirdPartyConfig> LoadThirdPartyConfigs()
        {
            var configs = new Dictionary<string, ThirdPartyConfig>();

            var section = _configuration.GetSection("ThirdPartyConfigurations");
            foreach (var child in section.GetChildren())
            {
                var config = child.Get<ThirdPartyConfig>();
                if (config != null)
                {
                    configs[config.Name] = config;
                }
            }

            return configs;
        }

        public async Task<string> GetAccessTokenAsync(string thirdPartyName)
        {
            var cacheKey = $"token_{thirdPartyName}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                if (!_thirdPartyConfigs.TryGetValue(thirdPartyName, out var config))
                    throw new ArgumentException($"Third party {thirdPartyName} not configured");

                try
                {
                    var request = new
                    {
                        OperatorID = config.OperatorID,
                        OperatorSecret = config.OperatorSecret
                    };

                    var response = await _httpClient.PostAsJsonAsync(
                        $"{config.BaseUrl}/evcs/{config.Version}/query_token",
                        request);

                    response.EnsureSuccessStatusCode();

                    var result = await response.Content.ReadFromJsonAsync<TokenResponse>(_jsonOptions);

                    if (result?.Ret == 0 && !string.IsNullOrEmpty(result.AccessToken))
                    {
                        // 设置缓存过期时间，通常token有效期为7天，我们设置为6天23小时
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(6).Add(TimeSpan.FromHours(23));
                        return result.AccessToken;
                    }

                    throw new Exception($"Failed to get token: {result?.Msg}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get access token for {ThirdParty}", thirdPartyName);
                    throw;
                }
            });
        }

        private async Task<string> GetEncryptedDataAsync(string data, ThirdPartyConfig config)
        {
            // 实现AES加密逻辑
            // 这里简化实现，实际应根据附录B实现AES-128-CBC加密
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = Convert.FromBase64String(config.DataSecret);
            aes.IV = Convert.FromBase64String(config.DataSecretIV);
            aes.Mode = System.Security.Cryptography.CipherMode.CBC;
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }

        private string GenerateSignature(string data, ThirdPartyConfig config)
        {
            // 实现HMAC-MD5签名
            // 这里简化实现，实际应根据附录C实现HMAC-MD5签名
            using var hmac = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(config.SigSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }

        private async Task<T> CallThirdPartyApiAsync<T>(string thirdPartyName, string apiPath, object requestData)
        {
            if (!_thirdPartyConfigs.TryGetValue(thirdPartyName, out var config))
                throw new ArgumentException($"Third party {thirdPartyName} not configured");

            var token = await GetAccessTokenAsync(thirdPartyName);

            // 准备请求数据
            var timestamp = DateTime.Now.ToString("yyyyMMdd HHmmss");
            var seq = "0001"; // 这里应该实现自增序列
            var dataJson = JsonSerializer.Serialize(requestData, _jsonOptions);
            var encryptedData = await GetEncryptedDataAsync(dataJson, config);
            var signature = GenerateSignature($"{config.OperatorID}{encryptedData}{timestamp}{seq}", config);

            var request = new
            {
                OperatorID = config.OperatorID,
                Data = encryptedData,
                TimeStamp = timestamp,
                Seq = seq,
                Sig = signature
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{config.BaseUrl}/evcs/{config.Version}/{apiPath}")
            {
                Headers =
                {
                    { "Authorization", $"Bearer {token}" }
                },
                Content = JsonContent.Create(request, null, _jsonOptions)
            };

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ThirdPartyApiResponse<T>>(_jsonOptions);

            if (result?.Ret != 0)
                throw new Exception($"API call failed: {result?.Msg} (Code: {result?.Ret})");

            return result.Data;
        }

        public async Task<List<ThirdPartyStationDto>> SyncStationsAsync(string thirdPartyName)
        {
            try
            {
                var request = new
                {
                    LastQueryTime = "", // 可以传入最后查询时间实现增量同步
                    PageNo = 1,
                    PageSize = 100
                };

                var result = await CallThirdPartyApiAsync<StationQueryResult>(thirdPartyName, "query_stations_info", request);

                var stations = new List<ThirdPartyStationDto>();
                foreach (var station in result.StationInfos)
                {
                    stations.Add(new ThirdPartyStationDto
                    {
                        OperatorID = station.OperatorID,
                        StationID = station.StationID,
                        StationName = station.StationName,
                        Address = station.Address,
                        StationLat = station.StationLat,
                        StationLng = station.StationLng,
                        StationStatus = station.StationStatus,
                        ParkNums = station.ParkNums,
                        StationTel = station.StationTel,
                        ServiceTel = station.ServiceTel,
                        SiteGuide = station.SiteGuide,
                        Pictures = station.Pictures,
                        BusineHours = station.BusineHours,
                        ElectricityFee = station.ElectricityFee,
                        ServiceFee = station.ServiceFee,
                        ParkFee = station.ParkFee,
                        EquipmentInfos = station.EquipmentInfos?.Select(e => new ThirdPartyEquipmentDto
                        {
                            EquipmentID = e.EquipmentID,
                            EquipmentName = e.EquipmentName,
                            EquipmentType = e.EquipmentType,
                            Power = e.Power,
                            EquipmentLng = e.EquipmentLng,
                            EquipmentLat = e.EquipmentLat,
                            ConnectorInfos = e.ConnectorInfos?.Select(c => new ThirdPartyConnectorDto
                            {
                                ConnectorID = c.ConnectorID,
                                ConnectorName = c.ConnectorName,
                                ConnectorType = c.ConnectorType,
                                VoltageUpperLimits = c.VoltageUpperLimits,
                                VoltageLowerLimits = c.VoltageLowerLimits,
                                Current = c.Current,
                                Power = c.Power,
                                ParkNo = c.ParkNo,
                                NationalStandard = c.NationalStandard
                            }).ToList()
                        }).ToList(),
                        SyncTime = DateTime.UtcNow,
                        Source = thirdPartyName
                    });
                }

                return stations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync stations from {ThirdParty}", thirdPartyName);
                throw;
            }
        }

        public async Task<List<StationStatusDto>> GetStationsStatusAsync(string thirdPartyName, List<string> stationIds)
        {
            try
            {
                var request = new
                {
                    StationIDs = stationIds.Take(50).ToList() // 限制最多50个
                };

                var result = await CallThirdPartyApiAsync<StationStatusResult>(thirdPartyName, "query_station_status", request);

                var statusList = new List<StationStatusDto>();
                foreach (var statusInfo in result.StationStatusInfos)
                {
                    statusList.Add(new StationStatusDto
                    {
                        StationID = statusInfo.StationID,
                        ConnectorStatusInfos = statusInfo.ConnectorStatusInfos?.Select(c => new ConnectorStatusInfoDto
                        {
                            ConnectorID = c.ConnectorID,
                            Status = c.Status,
                            ParkStatus = c.ParkStatus,
                            LockStatus = c.LockStatus,
                            UpdateTime = DateTime.UtcNow
                        }).ToList()
                    });
                }

                return statusList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get station status from {ThirdParty}", thirdPartyName);
                throw;
            }
        }

        public async Task<StationStatsDto> GetStationStatsAsync(string thirdPartyName, string stationId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var request = new
                {
                    StationID = stationId,
                    StartTime = startDate.ToString("yyyy-MM-dd"),
                    EndTime = endDate.ToString("yyyy-MM-dd")
                };

                var result = await CallThirdPartyApiAsync<StationStatsInfo>(thirdPartyName, "query_station_stats", request);

                return new StationStatsDto
                {
                    StationID = result.StationID,
                    StartTime = startDate,
                    EndTime = endDate,
                    StationElectricity = result.StationElectricity,
                    EquipmentStatsInfos = result.EquipmentStatsInfos?.Select(e => new EquipmentStatsDto
                    {
                        EquipmentID = e.EquipmentID,
                        EquipmentElectricity = e.EquipmentElectricity,
                        ConnectorStatsInfos = e.ConnectorStatsInfos?.Select(c => new ConnectorStatsDto
                        {
                            ConnectorID = c.ConnectorID,
                            ConnectorElectricity = c.ConnectorElectricity
                        }).ToList()
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get station stats from {ThirdParty}", thirdPartyName);
                throw;
            }
        }

        public async Task<EquipmentAuthResultDto> RequestEquipmentAuthAsync(string thirdPartyName, string connectorId)
        {
            try
            {
                var request = new
                {
                    EquipAuthSeq = GenerateSequence(thirdPartyName),
                    ConnectorID = connectorId
                };

                var result = await CallThirdPartyApiAsync<EquipmentAuthResponse>(thirdPartyName, "query_equip_auth", request);

                return new EquipmentAuthResultDto
                {
                    Success = result.Ret == 0,
                    EquipAuthSeq = result.EquipAuthSeq,
                    FailReason = result.FailReason,
                    Message = result.Msg,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to request equipment auth from {ThirdParty}", thirdPartyName);
                throw;
            }
        }

        public async Task<StartChargeResultDto> StartChargingAsync(string thirdPartyName, string connectorId, string userId)
        {
            try
            {
                var request = new
                {
                    StartChargeSeq = GenerateSequence(thirdPartyName),
                    ConnectorID = connectorId,
                    QRCode = GenerateQRCode(userId, connectorId)
                };

                var result = await CallThirdPartyApiAsync<StartChargeResponse>(thirdPartyName, "query_start_charge", request);

                return new StartChargeResultDto
                {
                    Success = result.Ret == 0,
                    StartChargeSeq = result.StartChargeSeq,
                    FailReason = result.FailReason,
                    Message = result.Msg,
                    StartTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start charging from {ThirdParty}", thirdPartyName);
                throw;
            }
        }

        public async Task<StopChargeResultDto> StopChargingAsync(string thirdPartyName, string startChargeSeq)
        {
            try
            {
                // 首先需要获取连接器ID，这里简化处理
                var connectorId = await GetConnectorIdByStartChargeSeqAsync(startChargeSeq);

                var request = new
                {
                    StartChargeSeq = startChargeSeq,
                    ConnectorID = connectorId
                };

                var result = await CallThirdPartyApiAsync<StopChargeResponse>(thirdPartyName, "query_stop_charge", request);

                return new StopChargeResultDto
                {
                    Success = result.Ret == 0,
                    StartChargeSeq = startChargeSeq,
                    TotalPower = result.TotalPower,
                    TotalMoney = result.TotalMoney,
                    Message = result.Msg,
                    StopTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop charging from {ThirdParty}", thirdPartyName);
                throw;
            }
        }

        public async Task<ChargeStatusDto> GetChargeStatusAsync(string thirdPartyName, string startChargeSeq)
        {
            try
            {
                var request = new
                {
                    StartChargeSeq = startChargeSeq
                };

                var result = await CallThirdPartyApiAsync<ChargeStatusResponse>(thirdPartyName, "query_equip_charge_status", request);

                return new ChargeStatusDto
                {
                    StartChargeSeq = result.StartChargeSeq,
                    StartChargeSeqStat = result.StartChargeSeqStat,
                    ConnectorID = result.ConnectorID,
                    CurrentA = result.CurrentA,
                    CurrentB = result.CurrentB,
                    CurrentC = result.CurrentC,
                    VoltageA = result.VoltageA,
                    VoltageB = result.VoltageB,
                    VoltageC = result.VoltageC,
                    Soc = result.Soc,
                    StartTime = DateTime.TryParse(result.StartTime, out var start) ? start : DateTime.MinValue,
                    EndTime = DateTime.TryParse(result.EndTime, out var end) ? end : (DateTime?)null,
                    TotalPower = result.TotalPower,
                    TotalMoney = result.TotalMoney,
                    UpdateTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get charge status from {ThirdParty}", thirdPartyName);
                throw;
            }
        }

        private string GenerateSequence(string thirdPartyName)
        {
            if (!_thirdPartyConfigs.TryGetValue(thirdPartyName, out var config))
                throw new ArgumentException($"Third party {thirdPartyName} not configured");

            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"{config.OperatorID}{timestamp}{random}";
        }

        private string GenerateQRCode(string userId, string connectorId)
        {
            // 生成二维码数据，这里简化实现
            return $"CHARGING:{userId}:{connectorId}:{DateTime.Now.Ticks}";
        }

        private async Task<string> GetConnectorIdByStartChargeSeqAsync(string startChargeSeq)
        {
            // 这里应该从数据库查询，返回连接器ID
            // 简化实现，返回一个默认值
            return "1";
        }

        public Task StartChargingAsync(string thirdPartyName, string connectorId, global::StartChargeRequest startChargeRequest)
        {
            throw new NotImplementedException();
        }

        //public Task StartChargingAsync(string thirdPartyName, string connectorId, StartChargeRequest startChargeRequest)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task StartChargingAsync(string thirdPartyName, string connectorId, string userId)
        //{
        //    throw new NotImplementedException();
        //}

        // 响应类
        private class TokenResponse
        {
            public int Ret { get; set; }
            public string Msg { get; set; }
            public string AccessToken { get; set; }
            public int ExpiresIn { get; set; }
        }

        private class ThirdPartyApiResponse<T>
        {
            public int Ret { get; set; }
            public string Msg { get; set; }
            public T Data { get; set; }
        }

        private class StationQueryResult
        {
            public int ItemSize { get; set; }
            public int PageCount { get; set; }
            public int PageNo { get; set; }
            public List<StationInfo> StationInfos { get; set; }
        }

        private class StationInfo
        {
            public string OperatorID { get; set; }
            public string StationID { get; set; }
            public string StationName { get; set; }
            public string Address { get; set; }
            public decimal StationLat { get; set; }
            public decimal StationLng { get; set; }
            public int StationStatus { get; set; }
            public int ParkNums { get; set; }
            public string StationTel { get; set; }
            public string ServiceTel { get; set; }
            public string SiteGuide { get; set; }
            public List<string> Pictures { get; set; }
            public string BusineHours { get; set; }
            public string ElectricityFee { get; set; }
            public string ServiceFee { get; set; }
            public string ParkFee { get; set; }
            public List<EquipmentInfo> EquipmentInfos { get; set; }
        }

        private class EquipmentInfo
        {
            public string EquipmentID { get; set; }
            public string EquipmentName { get; set; }
            public int EquipmentType { get; set; }
            public decimal Power { get; set; }
            public decimal? EquipmentLng { get; set; }
            public decimal? EquipmentLat { get; set; }
            public List<ConnectorInfo> ConnectorInfos { get; set; }
        }

        private class ConnectorInfo
        {
            public string ConnectorID { get; set; }
            public string ConnectorName { get; set; }
            public int ConnectorType { get; set; }
            public decimal VoltageUpperLimits { get; set; }
            public decimal VoltageLowerLimits { get; set; }
            public decimal Current { get; set; }
            public decimal Power { get; set; }
            public string ParkNo { get; set; }
            public int NationalStandard { get; set; }
        }

        private class StationStatusResult
        {
            public int Total { get; set; }
            public List<StationStatusInfo> StationStatusInfos { get; set; }
        }

        private class StationStatusInfo
        {
            public string StationID { get; set; }
            public List<ConnectorStatusInfo> ConnectorStatusInfos { get; set; }
        }

        private class ConnectorStatusInfo
        {
            public string ConnectorID { get; set; }
            public int Status { get; set; }
            public int ParkStatus { get; set; }
            public int LockStatus { get; set; }
        }

        private class StationStatsInfo
        {
            public string StationID { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public decimal StationElectricity { get; set; }
            public List<EquipmentStatsInfo> EquipmentStatsInfos { get; set; }
        }

        private class EquipmentStatsInfo
        {
            public string EquipmentID { get; set; }
            public decimal EquipmentElectricity { get; set; }
            public List<ConnectorStatsInfo> ConnectorStatsInfos { get; set; }
        }

        private class ConnectorStatsInfo
        {
            public string ConnectorID { get; set; }
            public decimal ConnectorElectricity { get; set; }
        }

        private class EquipmentAuthResponse
        {
            public int Ret { get; set; }
            public string Msg { get; set; }
            public string EquipAuthSeq { get; set; }
            public int FailReason { get; set; }
        }

        private class StartChargeResponse
        {
            public int Ret { get; set; }
            public string Msg { get; set; }
            public string StartChargeSeq { get; set; }
            public int FailReason { get; set; }
        }

        private class StopChargeResponse
        {
            public int Ret { get; set; }
            public string Msg { get; set; }
            public decimal TotalPower { get; set; }
            public decimal TotalMoney { get; set; }
        }

        private class ChargeStatusResponse
        {
            public string StartChargeSeq { get; set; }
            public int StartChargeSeqStat { get; set; }
            public string ConnectorID { get; set; }
            public decimal CurrentA { get; set; }
            public decimal CurrentB { get; set; }
            public decimal CurrentC { get; set; }
            public decimal VoltageA { get; set; }
            public decimal VoltageB { get; set; }
            public decimal VoltageC { get; set; }
            public decimal Soc { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public decimal TotalPower { get; set; }
            public decimal TotalMoney { get; set; }
        }
    }
}

public class StartChargeRequest
{
    public string ConnectorId { get; set; }
    public string UserId { get; set; }
}

