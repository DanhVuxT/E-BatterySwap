using EBatterySwap.Domain.Enums;

namespace EBatterySwap.Domain.Entities;

public class StationCommand
{
    public long CommandId { get; set; }
    public long StationId { get; set; }
    public long? SlotId { get; set; }
    public long? SwapItemId { get; set; }
    public long? ChargingSessionId { get; set; }
    public string CommandType { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public StationCommandStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public byte RetryCount { get; set; }

    public Station Station { get; set; } = null!;
    public Slot? Slot { get; set; }
    public SwapItem? SwapItem { get; set; }
    public ChargingSession? ChargingSession { get; set; }
}
