// ChargingStationManagement.Infrastructure/External/IThirdPartyApiClient.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.ValueObjects;

namespace ChargingStationManagement.Infrastructure.External
{
    public interface IThirdPartyApiClient
    {
        string ProviderName { get; }
        string OperatorId { get; }

        Task<StationInfoResponse> QueryStationsInfoAsync(QueryStationsInfoRequest request);
        Task<StationStatusResponse> QueryStationStatusAsync(QueryStationStatusRequest request);
        Task<StationStatsResponse> QueryStationStatsAsync(QueryStationStatsRequest request);
        Task<EquipmentAuthResponse> QueryEquipAuthAsync(EquipmentAuthRequest request);
        Task<StartChargeResponse> QueryStartChargeAsync(StartChargeRequest request);
        Task<StopChargeResponse> QueryStopChargeAsync(StopChargeRequest request);
        Task<ChargeStatusResponse> QueryEquipChargeStatusAsync(ChargeStatusRequest request);
        Task<ThirdPartyResponse<object>> QueryEquipBusinessPolicyAsync(EquipBusinessPolicyRequest request);
        Task<ThirdPartyResponse<object>> NotificationStartChargeResultAsync(StartChargeResultNotification request);
        Task<ThirdPartyResponse<object>> NotificationEquipChargeStatusAsync(ChargeStatusNotification request);
        Task<ThirdPartyResponse<object>> NotificationStopChargeResultAsync(StopChargeResultNotification request);
        Task<ThirdPartyResponse<object>> NotificationChargeOrderInfoAsync(ChargeOrderInfoNotification request);
        Task<ThirdPartyResponse<object>> CheckChargeOrdersAsync(CheckChargeOrdersRequest request);
    }

    public class QueryStationsInfoRequest
    {
        public string LastQueryTime { get; set; }
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class QueryStationStatusRequest
    {
        public List<string> StationIDs { get; set; }
    }

    public class QueryStationStatsRequest
    {
        public string StationID { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class EquipmentAuthRequest
    {
        public string EquipAuthSeq { get; set; }
        public string ConnectorID { get; set; }
    }

    public class StartChargeRequest
    {
        public string StartChargeSeq { get; set; }
        public string ConnectorID { get; set; }
        public string QRCode { get; set; }
    }

    public class StopChargeRequest
    {
        public string StartChargeSeq { get; set; }
        public string ConnectorID { get; set; }
    }

    public class ChargeStatusRequest
    {
        public string StartChargeSeq { get; set; }
    }

    public class EquipBusinessPolicyRequest
    {
        public string EquipBizSeq { get; set; }
        public string ConnectorID { get; set; }
    }

    public class StartChargeResultNotification
    {
        public string StartChargeSeq { get; set; }
        public int StartChargeSeqStat { get; set; }
        public int FailReason { get; set; }
        public string ConnectorID { get; set; }
    }

    public class ChargeStatusNotification
    {
        public string StartChargeSeq { get; set; }
        public int StartChargeSeqStat { get; set; }
        public string ConnectorID { get; set; }
        public decimal CurrentA { get; set; }
        public decimal VoltageA { get; set; }
        public decimal Soc { get; set; }
        public decimal TotalPower { get; set; }
        public decimal TotalElectricity { get; set; }
    }

    public class StopChargeResultNotification
    {
        public string StartChargeSeq { get; set; }
        public int StartChargeSeqStat { get; set; }
        public int SuccStat { get; set; }
        public int FailReason { get; set; }
    }

    public class ChargeOrderInfoNotification
    {
        public string StartChargeSeq { get; set; }
        public string ConnectorID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal TotalElectricity { get; set; }
        public decimal TotalMoney { get; set; }
        public decimal TotalPeriod { get; set; }
        public int StopReason { get; set; }
    }

    public class CheckChargeOrdersRequest
    {
        public List<string> StartChargeSeqs { get; set; }
    }
}