// ChargingStationManagement.Services/DTOs/StationDto.cs
using System;
using System.Collections.Generic;

namespace ChargingStationManagement.Services.DTOs
{
    public class StationDto
    {
        public string StationId { get; set; }
        public string OperatorId { get; set; }
        public string OperatorName { get; set; }
        public string StationName { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public int AvailableConnectors { get; set; }
        public int TotalConnectors { get; set; }
        public decimal TotalPower { get; set; }
        public RateDto Rates { get; set; }
        public List<EquipmentDto> Equipment { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Source { get; set; }
    }

    public class StationDetailDto : StationDto
    {
        public string StationTel { get; set; }
        public string ServiceTel { get; set; }
        public string SiteGuide { get; set; }
        public List<string> Pictures { get; set; }
        public string BusinessHours { get; set; }
        public string ParkInfo { get; set; }
        public decimal StationElectricity { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<StationStatusHistoryDto> StatusHistory { get; set; }
    }

    public class EquipmentDto
    {
        public string EquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public int EquipmentType { get; set; }
        public string EquipmentTypeText { get; set; }
        public decimal Power { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public List<ConnectorDto> Connectors { get; set; }
        public decimal EquipmentElectricity { get; set; }
        public int TotalSessions { get; set; }
        public string ManufacturerName { get; set; }
        public string FirmwareVersion { get; set; }
    }

    public class ConnectorDto
    {
        public string ConnectorId { get; set; }
        public string EquipmentId { get; set; }
        public string ConnectorName { get; set; }
        public int Standard { get; set; }
        public string StandardText { get; set; }
        public decimal Power { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public int ParkStatus { get; set; }
        public string ParkStatusText { get; set; }
        public int LockStatus { get; set; }
        public string LockStatusText { get; set; }
        public string ParkNo { get; set; }
        public DateTime StatusUpdateTime { get; set; }
        public bool CanStartCharging { get; set; }
        public decimal VoltageUpperLimits { get; set; }
        public decimal VoltageLowerLimits { get; set; }
        public decimal Current { get; set; }
    }

    public class RateDto
    {
        public decimal ElectricityRate { get; set; }  // 元/kWh
        public decimal ServiceRate { get; set; }      // 元/kWh
        public decimal ParkRate { get; set; }         // 元/小时
        public decimal TimeRate { get; set; }         // 元/分钟
    }

    public class ConnectorStatusDto
    {
        public required string ConnectorId { get; set; }
        public int Status { get; set; }
        public int ParkStatus { get; set; }
        public int LockStatus { get; set; }
        public DateTime UpdateTime { get; set; }
    }

    public class StationStatusHistoryDto
    {
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string Reason { get; set; }
        public DateTime ChangeTime { get; set; }
    }
}


