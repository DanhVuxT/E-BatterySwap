using EBatterySwap.Domain.Enums;

namespace EBatterySwap.Domain.Entities;

public class Station
{
    public long StationId { get; set; }
    public string StationCode { get; set; } = string.Empty;
    public string StationName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    /// <summary>Identifies the station only; scanning it never unlocks a slot.</summary>
    public string QrIdentifier { get; set; } = string.Empty;
    public StationOperationalStatus OperationalStatus { get; set; }
    public StationConnectionStatus ConnectionStatus { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Slot> Slots { get; set; } = new List<Slot>();
}
