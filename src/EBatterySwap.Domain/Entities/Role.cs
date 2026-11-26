namespace EBatterySwap.Domain.Entities;

public class Role
{
    public long RoleId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
