namespace EBatterySwap.Domain.Enums;

public enum SwapSessionStatus
{
    Pending,
    InProgress,
    Completed,
    PartiallyCompleted,
    Failed,
    Cancelled
}

public enum SwapItemStatus
{
    Pending,
    ReturnConfirmed,
    IssueConfirmed,
    Completed,
    Failed,
    Cancelled
}
