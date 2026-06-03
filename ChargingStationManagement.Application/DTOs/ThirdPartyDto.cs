
// ChargingStationManagement.Services/DTOs/ThirdPartyDto.cs
using System;
using System.Collections.Generic;

namespace ChargingStationManagement.Services.DTOs.ThirdParty
{
    public class ThirdPartyStationDto
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
        public List<ThirdPartyEquipmentDto> EquipmentInfos { get; set; }
        public DateTime SyncTime { get; set; }
        public string Source { get; set; }
    }

    public class ThirdPartyEquipmentDto
    {
        public string EquipmentID { get; set; }
        public string EquipmentName { get; set; }
        public int EquipmentType { get; set; }
        public decimal Power { get; set; }
        public decimal? EquipmentLng { get; set; }
        public decimal? EquipmentLat { get; set; }
        public List<ThirdPartyConnectorDto> ConnectorInfos { get; set; }
    }

    public class ThirdPartyConnectorDto
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

    public class StationStatusDto
    {
        public string StationID { get; set; }
        public List<ConnectorStatusInfoDto> ConnectorStatusInfos { get; set; }
    }

    public class ConnectorStatusInfoDto
    {
        public string ConnectorID { get; set; }
        public int Status { get; set; }
        public int ParkStatus { get; set; }
        public int LockStatus { get; set; }
        public DateTime UpdateTime { get; set; }
    }

    public class StationStatsDto
    {
        public string StationID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal StationElectricity { get; set; }
        public List<EquipmentStatsDto> EquipmentStatsInfos { get; set; }
    }

    public class EquipmentStatsDto
    {
        public string EquipmentID { get; set; }
        public decimal EquipmentElectricity { get; set; }
        public List<ConnectorStatsDto> ConnectorStatsInfos { get; set; }
    }

    public class ConnectorStatsDto
    {
        public string ConnectorID { get; set; }
        public decimal ConnectorElectricity { get; set; }
    }

    public class EquipmentAuthResultDto
    {
        public bool Success { get; set; }
        public string EquipAuthSeq { get; set; }
        public int FailReason { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class StartChargeResultDto
    {
        public bool Success { get; set; }
        public string StartChargeSeq { get; set; }
        public int FailReason { get; set; }
        public string Message { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class StopChargeResultDto
    {
        public bool Success { get; set; }
        public string StartChargeSeq { get; set; }
        public decimal TotalPower { get; set; }
        public decimal TotalMoney { get; set; }
        public string Message { get; set; }
        public DateTime StopTime { get; set; }
    }

    public class ChargeStatusDto
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
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal TotalPower { get; set; }
        public decimal TotalMoney { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}