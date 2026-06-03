using ChargingStationManagement.Domain.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ChargingStationManagement.Domain.Entities;

public class Equipment
{
    public Guid Id { get; private set; }
    public string EquipmentId { get; private set; } = null!;
    public Guid StationId { get; private set; }
    public string EquipmentName { get; private set; } = null!;
    public EquipmentType EquipmentType { get; private set; }
    public decimal Power { get; private set; }
    public EquipmentStatus Status { get; private set; }
    public decimal EquipmentElectricity { get; private set; }
    public int TotalSessions { get; private set; }
    public string? ManufacturerName { get; private set; }
    public string? FirmwareVersion { get; private set; }
    public string Source { get; private set; } = null!;

    private readonly List<Connector> _connectors = new();
    public IReadOnlyCollection<Connector> Connectors => _connectors.AsReadOnly();

    private Equipment() { }

    public Equipment(string equipmentId, Guid stationId, string equipmentName, EquipmentType type, decimal power, string source, EquipmentType equipmentType)
    {
        Id = Guid.NewGuid();
        EquipmentId = equipmentId;
        StationId = stationId;
        EquipmentName = equipmentName;
        EquipmentType = type;
        Power = power;
        Status = EquipmentStatus.Idle;
        Source = source;
        EquipmentType = equipmentType;
    }

    public Equipment(string equipmentId, Guid stationId, string equipmentName, EquipmentType equipmentType, decimal power, string source)
    {
        EquipmentId = equipmentId;
        StationId = stationId;
        EquipmentName = equipmentName;
        EquipmentType = equipmentType;
        Power = power;
        Source = source;
    }

    public void UpdateTechnicalSpecs(string name, string manufacturer, decimal voltage, decimal current, string protocol, string firmware)
    {
        EquipmentName = name;
        ManufacturerName = manufacturer;
        FirmwareVersion = firmware;
    }

    public void AddConnector(Connector connector)
    {
        _connectors.Add(connector);
    }

    public Connector? GetConnector(string connectorId)
        => _connectors.FirstOrDefault(c => c.ConnectorId == connectorId);

    public void UpdateStatistics(decimal energy, TimeSpan duration)
    {
        EquipmentElectricity += energy;
        TotalSessions++;
    }
}

// EquipmentStatus enum (add to Enums.cs if not exists)
public enum EquipmentStatus
{
    Unknown = 0,
    Idle = 1,
    Standby = 2,
    Charging = 3,
    Fault = 4,
    Offline = 5,
    Maintenance = 6,
    Upgrading = 7
}