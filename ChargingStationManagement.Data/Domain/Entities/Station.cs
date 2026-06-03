using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.ValueObjects;

namespace ChargingStationManagement.Domain.Entities;

public class Station
{
    public Guid Id { get; private set; }
    public string StationId { get; private set; } = null!; // Business key
    public string OperatorId { get; private set; } = null!;
    public string StationName { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public Coordinates Location { get; private set; } = null!;
    public StationStatus Status { get; private set; }
    public int AvailableConnectors { get; private set; }
    public int TotalConnectors { get; private set; }
    public decimal TotalPower { get; private set; }
    public Rate ElectricityRate { get; private set; } = null!;
    public Rate ServiceRate { get; private set; } = null!;
    public Rate ParkRate { get; private set; } = null!;
    public string Source { get; private set; } = null!;
    public string? StationTel { get; private set; }
    public string? ServiceTel { get; private set; }
    public string? SiteGuide { get; private set; }
    public string? Pictures { get; private set; } // Semicolon separated URLs
    public string? BusinessHours { get; private set; }
    public string? ParkInfo { get; private set; }
    public decimal StationElectricity { get; private set; }
    public decimal TotalRevenue { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<StationStatusHistory> _statusHistory = new();
    public IReadOnlyCollection<StationStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    private readonly List<Equipment> _equipment = new();
    public IReadOnlyCollection<Equipment> Equipment => _equipment.AsReadOnly();

    private Station() { } // EF Core

    public Station(string stationId, string operatorId, string stationName, Address address, Coordinates location, string source, string createdBy)
    {
        Id = Guid.NewGuid();
        StationId = stationId;
        OperatorId = operatorId;
        StationName = stationName;
        Address = address;
        Location = location;
        Source = source;
        Status = StationStatus.Normal;
        CreatedAt = DateTime.UtcNow;

        ElectricityRate = new Rate(0, 0, 0, 0);
        ServiceRate = new Rate(0, 0, 0, 0);
        ParkRate = new Rate(0, 0, 0, 0);
    }

    public void UpdateBasicInfo(string name, Address address, string? stationTel, string? serviceTel, string? siteGuide, string? businessHours)
    {
        StationName = name;
        Address = address;
        StationTel = stationTel;
        ServiceTel = serviceTel;
        SiteGuide = siteGuide;
        BusinessHours = businessHours;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(StationStatus status, string reason)
    {
        if (Status == status) return;
        Status = status;
        _statusHistory.Add(new StationStatusHistory(status, reason, DateTime.UtcNow));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRates(Rate electricityRate, Rate serviceRate, Rate parkRate)
    {
        ElectricityRate = electricityRate;
        ServiceRate = serviceRate;
        ParkRate = parkRate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddEquipment(Equipment equipment)
    {
        _equipment.Add(equipment);
        TotalConnectors += equipment.Connectors.Count;
        TotalPower += equipment.Power;
        UpdatedAt = DateTime.UtcNow;
    }

    public Equipment? GetEquipment(string equipmentId)
        => _equipment.FirstOrDefault(e => e.EquipmentId == equipmentId);

    public void SyncCompleted(string source)
    {
        Source = source;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatistics(decimal energy, decimal revenue)
    {
        StationElectricity += energy;
        TotalRevenue += revenue;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePictures(string pictures)
    {
        Pictures = pictures;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateParkInfo(string parkInfo)
    {
        ParkInfo = parkInfo;
        UpdatedAt = DateTime.UtcNow;
    }
    public decimal CalculateChargingCost(decimal energyKwh, TimeSpan duration, bool includeParking)
    {
        var electricity = energyKwh * ElectricityRate.ElectricityRate;
        var service = energyKwh * ServiceRate.ServiceRate;
        var park = includeParking ? (decimal)duration.TotalHours * ParkRate.ParkRate : 0;
        return electricity + service + park;
    }
}