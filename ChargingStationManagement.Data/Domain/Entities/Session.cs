using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.ValueObjects;

namespace ChargingStationManagement.Domain.Entities;

public class Session
{
    public Guid Id { get; private set; }
    public string SessionId { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public Guid ConnectorId { get; private set; }
    public Guid EquipmentId { get; private set; }
    public Guid StationId { get; private set; }
    public string StartChargeSeq { get; private set; } = null!;
    public ChargingMode ChargingMode { get; private set; }
    public string StartedBy { get; private set; } = null!;
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public ChargeStatus Status { get; private set; }
    public OrderStatus OrderStatus { get; private set; }
    public decimal TotalEnergy { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsPaid { get; private set; }
    public string? VehicleLicensePlate { get; private set; }
    public decimal StartBatteryLevel { get; private set; }
    public decimal EndBatteryLevel { get; private set; }
    public Rate AppliedRates { get; private set; } = null!;
    public decimal CurrentVoltage { get; private set; }
    public decimal CurrentCurrent { get; private set; }
    public decimal CurrentPower { get; private set; }
    public decimal CurrentEnergy { get; private set; }
    public DateTime LastDataUpdate { get; private set; }
    public string? StoppedBy { get; private set; }
    public string? StopReason { get; private set; }

    private Session() { }

    public Session(string sessionId, Guid userId, Guid connectorId, Guid equipmentId, Guid stationId,
        string startChargeSeq, ChargingMode chargingMode, string startedBy)
    {
        Id = Guid.NewGuid();
        SessionId = sessionId;
        UserId = userId;
        ConnectorId = connectorId;
        EquipmentId = equipmentId;
        StationId = stationId;
        StartChargeSeq = startChargeSeq;
        ChargingMode = chargingMode;
        StartedBy = startedBy;
        StartTime = DateTime.UtcNow;
        Status = ChargeStatus.Starting;
        OrderStatus = OrderStatus.InProgress;
        AppliedRates = new Rate(0, 0, 0, 0);
    }

    public void UpdateRealTimeData(decimal voltage, decimal current, decimal power, decimal energy, decimal batteryLevel)
    {
        CurrentVoltage = voltage;
        CurrentCurrent = current;
        CurrentPower = power;
        CurrentEnergy = energy;
        LastDataUpdate = DateTime.UtcNow;
    }

    public void StopCharging(string stoppedBy, string reason)
    {
        Status = ChargeStatus.Stopping;
        StoppedBy = stoppedBy;
        StopReason = reason;
        EndTime = DateTime.UtcNow;
    }

    public void CompleteCharging(decimal totalEnergy, decimal totalAmount, decimal electricityCost, decimal serviceCost, decimal parkCost, Rate rates)
    {
        TotalEnergy = totalEnergy;
        TotalAmount = totalAmount;
        EndTime = DateTime.UtcNow;
        Status = ChargeStatus.Ended;
        OrderStatus = OrderStatus.Completed;
        AppliedRates = rates;
        IsPaid = false; // Will be paid later
    }

    public TimeSpan? GetDuration()
    {
        if (EndTime.HasValue)
            return EndTime.Value - StartTime;
        return null;
    }

    public void CompleteCharging(decimal totalPower, decimal v1, decimal v2, int v3, Rate rates)
    {
        throw new NotImplementedException();
    }
}