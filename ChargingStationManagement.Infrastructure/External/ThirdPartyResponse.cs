// ChargingStationManagement.Infrastructure/External/ThirdPartyResponse.cs
using System.Text.Json.Serialization;

namespace ChargingStationManagement.Infrastructure.External
{
    public class ThirdPartyResponse<T>
    {
        [JsonPropertyName("ret")]
        public int ReturnCode { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }

        public bool IsSuccess => ReturnCode == 0;
    }

    public class StationInfoResponse
    {
        [JsonPropertyName("ItemSize")]
        public int ItemSize { get; set; }

        [JsonPropertyName("PageCount")]
        public int PageCount { get; set; }

        [JsonPropertyName("PageNo")]
        public int PageNo { get; set; }

        [JsonPropertyName("StationInfos")]
        public List<StationInfo> StationInfos { get; set; }
    }

    public class StationInfo
    {
        [JsonPropertyName("OperatorID")]
        public string OperatorID { get; set; }

        [JsonPropertyName("StationID")]
        public string StationID { get; set; }

        [JsonPropertyName("EquipmentOwnerID")]
        public string EquipmentOwnerID { get; set; }

        [JsonPropertyName("StationName")]
        public string StationName { get; set; }

        [JsonPropertyName("CountryCode")]
        public string CountryCode { get; set; }

        [JsonPropertyName("AreaCode")]
        public string AreaCode { get; set; }

        [JsonPropertyName("Address")]
        public string Address { get; set; }

        [JsonPropertyName("StationTel")]
        public string StationTel { get; set; }

        [JsonPropertyName("ServiceTel")]
        public string ServiceTel { get; set; }

        [JsonPropertyName("StationType")]
        public int StationType { get; set; }

        [JsonPropertyName("StationStatus")]
        public int StationStatus { get; set; }

        [JsonPropertyName("ParkNums")]
        public int ParkNums { get; set; }

        [JsonPropertyName("StationLng")]
        public decimal StationLng { get; set; }

        [JsonPropertyName("StationLat")]
        public decimal StationLat { get; set; }

        [JsonPropertyName("SiteGuide")]
        public string SiteGuide { get; set; }

        [JsonPropertyName("Construction")]
        public int Construction { get; set; }

        [JsonPropertyName("Pictures")]
        public List<string> Pictures { get; set; }

        [JsonPropertyName("MatchCars")]
        public string MatchCars { get; set; }

        [JsonPropertyName("ParkInfo")]
        public string ParkInfo { get; set; }

        [JsonPropertyName("BusineHours")]
        public string BusinessHours { get; set; }

        [JsonPropertyName("ElectricityFee")]
        public string ElectricityFee { get; set; }

        [JsonPropertyName("ServiceFee")]
        public string ServiceFee { get; set; }

        [JsonPropertyName("ParkFee")]
        public string ParkFee { get; set; }

        [JsonPropertyName("EquipmentInfos")]
        public List<EquipmentInfo> EquipmentInfos { get; set; }
    }

    public class EquipmentInfo
    {
        [JsonPropertyName("EquipmentID")]
        public string EquipmentID { get; set; }

        [JsonPropertyName("ManufacturerID")]
        public string ManufacturerID { get; set; }

        [JsonPropertyName("ManufacturerName")]
        public string ManufacturerName { get; set; }

        [JsonPropertyName("EquipmentModel")]
        public string EquipmentModel { get; set; }

        [JsonPropertyName("ProductionDate")]
        public string ProductionDate { get; set; }

        [JsonPropertyName("EquipmentType")]
        public int EquipmentType { get; set; }

        [JsonPropertyName("EquipmentLng")]
        public decimal? EquipmentLng { get; set; }

        [JsonPropertyName("EquipmentLat")]
        public decimal? EquipmentLat { get; set; }

        [JsonPropertyName("Power")]
        public decimal Power { get; set; }

        [JsonPropertyName("EquipmentName")]
        public string EquipmentName { get; set; }

        [JsonPropertyName("ConnectorInfos")]
        public List<ConnectorInfo> ConnectorInfos { get; set; }
    }

    public class ConnectorInfo
    {
        [JsonPropertyName("ConnectorID")]
        public string ConnectorID { get; set; }

        [JsonPropertyName("ConnectorName")]
        public string ConnectorName { get; set; }

        [JsonPropertyName("ConnectorType")]
        public int ConnectorType { get; set; }

        [JsonPropertyName("VoltageUpperLimits")]
        public decimal VoltageUpperLimits { get; set; }

        [JsonPropertyName("VoltageLowerLimits")]
        public decimal VoltageLowerLimits { get; set; }

        [JsonPropertyName("Current")]
        public decimal Current { get; set; }

        [JsonPropertyName("Power")]
        public decimal Power { get; set; }

        [JsonPropertyName("ParkNo")]
        public string ParkNo { get; set; }

        [JsonPropertyName("NationalStandard")]
        public int NationalStandard { get; set; }
    }

    public class StationStatusResponse
    {
        [JsonPropertyName("Total")]
        public int Total { get; set; }

        [JsonPropertyName("StationStatusInfos")]
        public List<StationStatusInfo> StationStatusInfos { get; set; }
    }

    public class StationStatusInfo
    {
        [JsonPropertyName("StationID")]
        public string StationID { get; set; }

        [JsonPropertyName("ConnectorStatusInfos")]
        public List<ConnectorStatusInfo> ConnectorStatusInfos { get; set; }
    }

    public class ConnectorStatusInfo
    {
        [JsonPropertyName("ConnectorID")]
        public string ConnectorID { get; set; }

        [JsonPropertyName("Status")]
        public int Status { get; set; }

        [JsonPropertyName("ParkStatus")]
        public int ParkStatus { get; set; }

        [JsonPropertyName("LockStatus")]
        public int LockStatus { get; set; }
    }

    public class StationStatsResponse
    {
        [JsonPropertyName("StationStats")]
        public StationStatsInfo StationStats { get; set; }
    }

    public class StationStatsInfo
    {
        [JsonPropertyName("StationID")]
        public string StationID { get; set; }

        [JsonPropertyName("StartTime")]
        public string StartTime { get; set; }

        [JsonPropertyName("EndTime")]
        public string EndTime { get; set; }

        [JsonPropertyName("StationElectricity")]
        public decimal StationElectricity { get; set; }

        [JsonPropertyName("EquipmentStatsInfos")]
        public List<EquipmentStatsInfo> EquipmentStatsInfos { get; set; }
    }

    public class EquipmentStatsInfo
    {
        [JsonPropertyName("EquipmentID")]
        public string EquipmentID { get; set; }

        [JsonPropertyName("EquipmentElectricity")]
        public decimal EquipmentElectricity { get; set; }

        [JsonPropertyName("ConnectorStatsInfos")]
        public List<ConnectorStatsInfo> ConnectorStatsInfos { get; set; }
    }

    public class ConnectorStatsInfo
    {
        [JsonPropertyName("ConnectorID")]
        public string ConnectorID { get; set; }

        [JsonPropertyName("ConnectorElectricity")]
        public decimal ConnectorElectricity { get; set; }
    }

    // 充电业务相关响应
    public class EquipmentAuthResponse
    {
        [JsonPropertyName("Ret")]
        public int Ret { get; set; }

        [JsonPropertyName("Msg")]
        public string Msg { get; set; }

        [JsonPropertyName("Data")]
        public EquipmentAuthData Data { get; set; }
    }

    public class EquipmentAuthData
    {
        [JsonPropertyName("IsAuthenticated")]
        public bool IsAuthenticated { get; set; }

        [JsonPropertyName("ConnectorID")]
        public string ConnectorID { get; set; }

        [JsonPropertyName("FailureReason")]
        public int FailureReason { get; set; }
    }

    public class StartChargeResponse
    {
        [JsonPropertyName("Ret")]
        public int Ret { get; set; }

        [JsonPropertyName("Msg")]
        public string Msg { get; set; }

        [JsonPropertyName("Data")]
        public StartChargeData Data { get; set; }
    }

    public class StartChargeData
    {
        [JsonPropertyName("StartChargeSeq")]
        public string StartChargeSeq { get; set; }

        [JsonPropertyName("ConnectorStatus")]
        public int ConnectorStatus { get; set; }

        [JsonPropertyName("StartChargeSeqStat")]
        public int StartChargeSeqStat { get; set; }
    }

    public class StopChargeResponse
    {
        [JsonPropertyName("Ret")]
        public int Ret { get; set; }

        [JsonPropertyName("Msg")]
        public string Msg { get; set; }

        [JsonPropertyName("Data")]
        public StopChargeData Data { get; set; }
    }

    public class StopChargeData
    {
        [JsonPropertyName("TotalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("TotalEnergy")]
        public decimal TotalEnergy { get; set; }

        [JsonPropertyName("TotalDuration")]
        public decimal TotalDuration { get; set; }

        [JsonPropertyName("StartChargeSeqStat")]
        public int StartChargeSeqStat { get; set; }
    }

    public class ChargeStatusResponse
    {
        [JsonPropertyName("Ret")]
        public int Ret { get; set; }

        [JsonPropertyName("Msg")]
        public string Msg { get; set; }

        [JsonPropertyName("Data")]
        public ChargeStatusData Data { get; set; }
    }

    public class ChargeStatusData
    {
        [JsonPropertyName("StartChargeSeq")]
        public string StartChargeSeq { get; set; }

        [JsonPropertyName("StartChargeSeqStat")]
        public int StartChargeSeqStat { get; set; }

        [JsonPropertyName("ConnectorStatus")]
        public int ConnectorStatus { get; set; }

        [JsonPropertyName("CurrentA")]
        public decimal CurrentA { get; set; }

        [JsonPropertyName("CurrentB")]
        public decimal CurrentB { get; set; }

        [JsonPropertyName("CurrentC")]
        public decimal CurrentC { get; set; }

        [JsonPropertyName("VoltageA")]
        public decimal VoltageA { get; set; }

        [JsonPropertyName("VoltageB")]
        public decimal VoltageB { get; set; }

        [JsonPropertyName("VoltageC")]
        public decimal VoltageC { get; set; }

        [JsonPropertyName("Soc")]
        public decimal Soc { get; set; }

        [JsonPropertyName("StartTime")]
        public string StartTime { get; set; }

        [JsonPropertyName("EndTime")]
        public string EndTime { get; set; }

        [JsonPropertyName("TotalPower")]
        public decimal TotalPower { get; set; }

        [JsonPropertyName("TotalElectricity")]
        public decimal TotalElectricity { get; set; }
    }
}