namespace EBatterySwap.Domain.Enums;

public enum StationOperationalStatus
{
    Active,
    Inactive,
    Maintenance
}

public enum StationConnectionStatus
{
    Online,
    Offline
}

public enum SlotDoorStatus
{
    Open,
    Closed,
    Unknown
}

public enum SlotLockStatus
{
    Locked,
    Unlocked,
    Unknown
}

public enum SlotOperationalStatus
{
    Available,
    Disabled,
    Fault,
    Maintenance
}
