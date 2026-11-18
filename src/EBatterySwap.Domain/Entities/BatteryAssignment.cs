using EBatterySwap.Domain.Enums;

namespace EBatterySwap.Domain.Entities;

/// <summary>Battery usage right granted to a vehicle (not the physical location).</summary>
public class BatteryAssignment
{
    public long AssignmentId { get; set; }
    public long BatteryId { get; set; }
    public long VehicleId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public AssignmentSource AssignmentSource { get; set; }
    public long? RecordedByUserId { get; set; }

    public Battery Battery { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
    public User? RecordedByUser { get; set; }
}
