// ChargingStationManagement.Services/DTOs/ChargingDto.cs
using System;
using System.Collections.Generic;

namespace ChargingStationManagement.Services.DTOs
{
    public class StartSessionRequestDto
    {
        public string UserId { get; set; }
        public string ConnectorId { get; set; }
        public int ChargingMode { get; set; }
        public string VehicleLicensePlate { get; set; }
        public decimal? VehicleBatteryCapacity { get; set; }
        public decimal? StartBatteryLevel { get; set; }
        public DateTime? ScheduledEndTime { get; set; }
    }

    public class StartSessionResultDto
    {
        public bool Success { get; set; }
        public string SessionId { get; set; }
        public string StartChargeSeq { get; set; }
        public string Message { get; set; }
        public string QRCode { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class ChargingDataDto
    {
        public decimal Voltage { get; set; }
        public decimal Current { get; set; }
        public decimal Power { get; set; }
        public decimal Energy { get; set; }
        public decimal BatteryLevel { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class StopSessionRequestDto
    {
        public string SessionId { get; set; }
        public string StoppedBy { get; set; } // User/Device/System
        public string Reason { get; set; }
        public decimal? EndMeterValue { get; set; }
        public decimal? EndBatteryLevel { get; set; }
    }

    public class StopSessionResultDto
    {
        public bool Success { get; set; }
        public string SessionId { get; set; }
        public decimal TotalEnergy { get; set; }
        public decimal TotalAmount { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime EndTime { get; set; }
        public string Message { get; set; }
    }

    public class SessionDto
    {
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ConnectorId { get; set; }
        public string EquipmentId { get; set; }
        public string StationId { get; set; }
        public string StationName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public int OrderStatus { get; set; }
        public string OrderStatusText { get; set; }
        public decimal TotalEnergy { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; }
        public string VehicleLicensePlate { get; set; }
        public decimal StartBatteryLevel { get; set; }
        public decimal EndBatteryLevel { get; set; }
        public RateDto Rates { get; set; }
        public ChargingDataDto CurrentData { get; set; }
        public string StartedBy { get; set; }
        public string StoppedBy { get; set; }
        public string StopReason { get; set; }
        public decimal CurrentPower { get; internal set; }
    }

    public class ActiveSessionDto : SessionDto
    {
        public decimal CurrentPower { get; set; }
        public decimal CurrentEnergy { get; set; }
        public DateTime LastDataUpdate { get; set; }
        public decimal EstimatedRemainingTime { get; set; }
        public decimal EstimatedRemainingCost { get; set; }

        internal bool IsScheduledToEnd()
        {
            throw new NotImplementedException();
        }
    }

    public class ChargingCostDto
    {
        public decimal ElectricityCost { get; set; }
        public decimal ServiceCost { get; set; }
        public decimal ParkCost { get; set; }
        public decimal TotalCost { get; set; }
        public RateDto Rates { get; set; }
        public decimal EstimatedEnergy { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
    }

    public enum ChargingMode
    {
        TimeBased = 1,
        EnergyBased = 2,
        TimeCard = 3
    }
}
