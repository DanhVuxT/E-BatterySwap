using EBatterySwap.Domain.Enums;

namespace EBatterySwap.Domain.Entities;

/// <summary>Maps to table dbo.Users.</summary>
public class User
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Role Role { get; set; } = null!;
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
