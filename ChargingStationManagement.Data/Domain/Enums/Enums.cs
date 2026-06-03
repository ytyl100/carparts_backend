namespace ChargingStationManagement.Domain.Enums;

public enum StationStatus
{
    Unknown = 0,
    UnderConstruction = 1,
    Closed = 5,
    Maintenance = 6,
    Normal = 50
}

public enum EquipmentType
{
    TwoWheeler = 1,
    FourWheeler = 2,
    FastCharger = 3,
    BatterySwap = 4
}

public enum ConnectorStandard
{
    GB_T = 1,
    CCS = 2,
    CHAdeMO = 3,
    Tesla = 4,
    AC = 5
}

public enum ConnectorStatus
{
    Offline = 0,
    Idle = 1,
    OccupiedNoCharging = 2,
    Charging = 3,
    Reserved = 4,
    Fault = 255
}

public enum ParkStatus
{
    Unknown = 0,
    Free = 10,
    Occupied = 50
}

public enum LockStatus
{
    Unknown = 0,
    Unlocked = 10,
    Locked = 50
}

public enum ChargeStatus
{
    Starting = 1,
    Charging = 2,
    Stopping = 3,
    Ended = 4,
    Unknown = 5
}

public enum OrderStatus
{
    Created = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    Refunded = 5
}

public enum ChargingMode
{
    TimeBased = 1,
    EnergyBased = 2,
    TimeCard = 3
}

public enum UserStatus
{
    Pending = 0,    // Awaiting approval
    Active = 1,     // Approved and active
    Rejected = 2,   // Rejected
    Suspended = 3   // Temporarily suspended
}