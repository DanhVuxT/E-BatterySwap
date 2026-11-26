using EBatterySwap.Domain.Enums;

namespace EBatterySwap.Domain.Entities;

/// <summary>One battery exchange inside a swap session; a session may complete only partially.</summary>
public class SwapItem
{
    public long SwapItemId { get; set; }
    public long SwapSessionId { get; set; }
    public byte SequenceNumber { get; set; }
    public long? ReturnedBatteryId { get; set; }
    public long? IssuedBatteryId { get; set; }
    public long? ReturnSlotId { get; set; }
    public long? IssueSlotId { get; set; }
    public SwapItemStatus Status { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }

    public SwapSession SwapSession { get; set; } = null!;
    public Battery? ReturnedBattery { get; set; }
    public Battery? IssuedBattery { get; set; }
    public Slot? ReturnSlot { get; set; }
    public Slot? IssueSlot { get; set; }
}
