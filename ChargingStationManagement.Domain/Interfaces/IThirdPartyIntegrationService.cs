// ChargingStationManagement.Domain/Interfaces/IThirdPartyIntegrationService.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingStationManagement.Domain.Interfaces
{
    /// <summary>
    /// 第三方集成服务接口
    /// </summary>
    public interface IThirdPartyIntegrationService
    {
        Task<List<ThirdPartyStation>> SyncStationsFromThirdPartyAsync(string thirdPartyName);
        Task<ThirdPartyStationStatus> GetStationStatusAsync(string thirdPartyName, string stationId);
        Task<ThirdPartyStationStats> GetStationStatisticsAsync(string thirdPartyName, string stationId, DateRange dateRange);
        Task<ThirdPartyAuthResponse> AuthenticateDeviceAsync(string thirdPartyName, string connectorId);
        Task<ThirdPartyStartChargeResponse> StartChargingAsync(string thirdPartyName, string connectorId, StartChargeRequest request);
        Task<ThirdPartyStopChargeResponse> StopChargingAsync(string thirdPartyName, StopChargeRequest request);
        Task<ThirdPartyChargeStatus> GetChargeStatusAsync(string thirdPartyName, string startChargeSeq);
        Task StopChargingAsync(string thirdPartyName, string startChargeSeq);
    }

    public class ThirdPartyStation
    {
        public string StationId { get; set; }
        public string StationName { get; set; }
        // ... 其他属性
    }

    public class ThirdPartyStationStatus
    {
        public string StationId { get; set; }
        public int Status { get; set; }
        // ... 其他属性
    }

    public class ThirdPartyStationStats
    {
        public string StationId { get; set; }
        public decimal TotalElectricity { get; set; }
        // ... 其他属性
    }

    public class ThirdPartyAuthResponse
    {
        public bool IsAuthenticated { get; set; }
        public string Reason { get; set; }
        // ... 其他属性
    }

    public class ThirdPartyStartChargeResponse
    {
        public bool Success { get; set; }
        public string StartChargeSeq { get; set; }
        public object Message { get; set; }
        // ... 其他属性
    }

    public class ThirdPartyStopChargeResponse
    {
        public bool Success { get; set; }
        public decimal TotalEnergy { get; set; }
        // ... 其他属性
    }

    public class ThirdPartyChargeStatus
    {
        public string StartChargeSeq { get; set; }
        public int Status { get; set; }
        // ... 其他属性
    }

    public class StartChargeRequest
    {
        public string ConnectorId { get; set; }
        public string UserId { get; set; }
        public string QRCode { get; set; }
    }

    public class StopChargeRequest
    {
        public string StartChargeSeq { get; set; }
        public string ConnectorId { get; set; }
    }

    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}