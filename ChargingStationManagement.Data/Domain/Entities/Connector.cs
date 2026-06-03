using ChargingStationManagement.Domain.Enums;

namespace ChargingStationManagement.Domain.Entities;

public class Connector
{
    public Guid Id { get; private set; }
    public string ConnectorId { get; private set; } = null!;
    public Guid EquipmentId { get; private set; }
    public string ConnectorName { get; private set; } = null!;
    public ConnectorStandard Standard { get; private set; }
    public decimal Power { get; private set; }
    public ConnectorStatus Status { get; private set; }
    public ParkStatus ParkStatus { get; private set; }
    public LockStatus LockStatus { get; private set; }
    public string? ParkNo { get; private set; }
    public DateTime StatusUpdateTime { get; private set; }
    public decimal VoltageUpperLimits { get; private set; }
    public decimal VoltageLowerLimits { get; private set; }
    public decimal Current { get; private set; }

    private Connector() { }

    public Connector(string connectorId, Guid equipmentId, ConnectorStandard standard, decimal power, string connectorName, string source)
    {
        Id = Guid.NewGuid();
        ConnectorId = connectorId;
        EquipmentId = equipmentId;
        Standard = standard;
        Power = power;
        ConnectorName = connectorName;
        Status = ConnectorStatus.Idle;
        ParkStatus = ParkStatus.Free;
        LockStatus = LockStatus.Unlocked;
        StatusUpdateTime = DateTime.UtcNow;
    }

    public void SetTechnicalSpecs(decimal voltageUpper, decimal voltageLower, decimal current, string? parkNo)
    {
        VoltageUpperLimits = voltageUpper;
        VoltageLowerLimits = voltageLower;
        Current = current;
        ParkNo = parkNo;
    }

    public void UpdateStatus(ConnectorStatus status, string reason)
    {
        Status = status;
        StatusUpdateTime = DateTime.UtcNow;
    }

    public void UpdateParkStatus(ParkStatus status)
    {
        ParkStatus = status;
        StatusUpdateTime = DateTime.UtcNow;
    }

    public void UpdateLockStatus(LockStatus status)
    {
        LockStatus = status;
        StatusUpdateTime = DateTime.UtcNow;
    }

    public bool CanStartCharging()
    {
        return Status == ConnectorStatus.Idle && ParkStatus != ParkStatus.Occupied && LockStatus != LockStatus.Locked;
    }

    public void StartSession(string sessionId, string userId)
    {
        Status = ConnectorStatus.Charging;
        StatusUpdateTime = DateTime.UtcNow;
    }

    public void EndSession()
    {
        Status = ConnectorStatus.Idle;
        StatusUpdateTime = DateTime.UtcNow;
    }

    public void UpdateRealTimeData(decimal voltage, decimal current, decimal power, decimal energy)
    {
        // Real-time data could be stored elsewhere; placeholder for connector metrics
    }
}