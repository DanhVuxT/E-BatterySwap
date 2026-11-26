namespace EBatterySwap.Domain.Enums;

public enum BatteryOperationalStatus
{
    Normal,
    Warning,
    Fault,
    Maintenance,
    Retired
}

public enum BatteryLocationType
{
    Slot,
    Vehicle,
    Storage,
    Maintenance,
    InTransit,
    Unknown
}

public enum AssignmentSource
{
    InitialHandover,
    Swap,
    ManualAdjustment
}

public enum TelemetrySource
{
    Simulator,
    Station,
    Manual
}

public enum ChargingSessionStatus
{
    Charging,
    Completed,
    Interrupted,
    Failed
}
