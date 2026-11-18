using EBatterySwap.Domain.Enums;

namespace EBatterySwap.Domain.Entities;

public class StationEvent
{
    public long EventId { get; set; }
    public long StationId { get; set; }
    public long? CommandId { get; set; }
    public long? SlotId { get; set; }
    public long? BatteryId { get; set; }

    /// <summary>Identifier supplied by the simulator; unique per station to reject duplicates.</summary>
    public string EventIdentifier { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime ReceivedAt { get; set; }
    public EventProcessingStatus ProcessingStatus { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessingError { get; set; }

    public Station Station { get; set; } = null!;
    public StationCommand? Command { get; set; }
    public Slot? Slot { get; set; }
    public Battery? Battery { get; set; }
}
