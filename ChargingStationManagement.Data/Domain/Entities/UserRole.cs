namespace ChargingStationManagement.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public string AssignedBy { get; private set; } = null!;   // Who assigned this role (UserId or "system")
    public DateTime AssignedAt { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole() { } // EF Core

    public UserRole(Guid userId, Guid roleId, string assignedBy)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedBy = assignedBy;
        AssignedAt = DateTime.UtcNow;
    }
}