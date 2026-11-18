using EBatterySwap.Domain.Enums;

namespace EBatterySwap.Domain.Entities;

public class ChargingSession
{
    public long ChargingSessionId { get; set; }
    public long BatteryId { get; set; }
    public long SlotId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public decimal? StartSoc { get; set; }
    public decimal? EndSoc { get; set; }
    public ChargingSessionStatus Status { get; set; }
    public string? StopReason { get; set; }

    public Battery Battery { get; set; } = null!;
    public Slot Slot { get; set; } = null!;
}
