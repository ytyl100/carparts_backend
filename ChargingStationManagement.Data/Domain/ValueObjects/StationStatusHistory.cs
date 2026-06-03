using ChargingStationManagement.Domain.Enums;

namespace ChargingStationManagement.Domain.ValueObjects;

public class StationStatusHistory
{
    public StationStatus Status { get; private set; }
    public string Reason { get; private set; }
    public DateTime ChangeTime { get; private set; }

    private StationStatusHistory() { }

    public StationStatusHistory(StationStatus status, string reason, DateTime changeTime)
    {
        Status = status;
        Reason = reason;
        ChangeTime = changeTime;
    }
}