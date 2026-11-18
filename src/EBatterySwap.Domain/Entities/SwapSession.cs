using EBatterySwap.Domain.Enums;

namespace EBatterySwap.Domain.Entities;

public class SwapSession
{
    public long SwapSessionId { get; set; }
    public long VehicleId { get; set; }
    public long StationId { get; set; }

    /// <summary>1 or 2 batteries.</summary>
    public byte RequestedBatteryCount { get; set; }
    public SwapSessionStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? FailureReason { get; set; }

    public Vehicle Vehicle { get; set; } = null!;
    public Station Station { get; set; } = null!;
    public ICollection<SwapItem> Items { get; set; } = new List<SwapItem>();
}
