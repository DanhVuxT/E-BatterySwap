namespace EBatterySwap.Domain.Enums;

public enum StationCommandStatus
{
    Pending,
    Sent,
    Acknowledged,
    Succeeded,
    Failed,
    Timeout,
    Cancelled
}

public enum EventProcessingStatus
{
    Pending,
    Processed,
    Failed
}

public enum AlertSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public enum AlertStatus
{
    Open,
    Acknowledged,
    Resolved
}
