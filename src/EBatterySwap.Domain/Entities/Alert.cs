using EBatterySwap.Domain.Enums;

namespace EBatterySwap.Domain.Entities;

public class Alert
{
    public long AlertId { get; set; }
    public long StationId { get; set; }
    public long? EventId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AlertStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public long? AcknowledgedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public long? ResolvedByUserId { get; set; }

    public Station Station { get; set; } = null!;
    public StationEvent? Event { get; set; }
    public User? AcknowledgedByUser { get; set; }
    public User? ResolvedByUser { get; set; }
}
