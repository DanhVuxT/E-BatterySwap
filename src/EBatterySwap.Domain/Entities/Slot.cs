using EBatterySwap.Domain.Enums;

namespace EBatterySwap.Domain.Entities;

public class Slot
{
    public long SlotId { get; set; }
    public long StationId { get; set; }
    public short SlotNumber { get; set; }
    public SlotDoorStatus DoorStatus { get; set; }
    public SlotLockStatus LockStatus { get; set; }
    public SlotOperationalStatus OperationalStatus { get; set; }
    public DateTime CreatedAt { get; set; }

    public Station Station { get; set; } = null!;
}
