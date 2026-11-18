using EBatterySwap.Domain.Enums;

namespace EBatterySwap.Domain.Entities;

public class BatteryTelemetry
{
    public long TelemetryId { get; set; }
    public long BatteryId { get; set; }
    public decimal Soc { get; set; }
    public decimal? Soh { get; set; }
    public decimal? Temperature { get; set; }
    public DateTime MeasuredAt { get; set; }
    public DateTime ReceivedAt { get; set; }
    public TelemetrySource Source { get; set; }

    public Battery Battery { get; set; } = null!;
}
