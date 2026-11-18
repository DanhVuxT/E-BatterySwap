using EBatterySwap.Domain.Enums;

namespace EBatterySwap.Domain.Entities;

public class Vehicle
{
    public long VehicleId { get; set; }
    public long UserId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;

    /// <summary>Number of batteries the vehicle uses: 1 or 2.</summary>
    public byte BatteryCapacityCount { get; set; }
    public VehicleStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
