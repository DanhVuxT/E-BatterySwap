using EBatterySwap.Domain.Enums;

namespace EBatterySwap.Domain.Entities;

public class Battery
{
    public long BatteryId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public decimal? Soc { get; set; }
    public decimal? Soh { get; set; }
    public decimal? Temperature { get; set; }
    public int CycleCount { get; set; }
    public BatteryOperationalStatus OperationalStatus { get; set; }
    public BatteryLocationType LocationType { get; set; }

    /// <summary>Required when <see cref="LocationType"/> is Slot, otherwise null.</summary>
    public long? CurrentSlotId { get; set; }
    public DateTime? LastTelemetryAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Slot? CurrentSlot { get; set; }
}
