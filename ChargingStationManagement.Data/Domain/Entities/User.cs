using ChargingStationManagement.Domain.Enums;
using System.Text;

namespace ChargingStationManagement.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Email { get; private set; }  // 邮箱字段

    // New fields for approval and status
    public UserStatus Status { get; private set; }
    public DateTime RegisteredAt { get; private set; }
    public string? ApprovedBy { get; private set; }    // UserId of approver
    public DateTime? ApprovedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    // Navigation property for roles
    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    public string? PasswordHash { get; private set; }
    public string? PasswordSalt { get; private set; }
    private User() { } // EF Core

    public User(string userId, string name, string? email = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        Email = email;
        Status = UserStatus.Pending;   // New users start as pending
        RegisteredAt = DateTime.UtcNow;
    }

    public void SetEmail(string? email)
    {
        Email = email;
    }

    // Methods for status changes
    public void Approve(string approvedBy)
    {
        if (Status != UserStatus.Pending)
            throw new InvalidOperationException($"Cannot approve user in status {Status}");

        Status = UserStatus.Active;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        RejectionReason = null;
    }

    public void Reject(string rejectedBy, string reason)
    {
        if (Status != UserStatus.Pending)
            throw new InvalidOperationException($"Cannot reject user in status {Status}");

        Status = UserStatus.Rejected;
        ApprovedBy = rejectedBy;   // Reusing field for who rejected
        ApprovedAt = DateTime.UtcNow;
        RejectionReason = reason;
    }

    public void Suspend(string suspendedBy, string reason)
    {
        if (Status != UserStatus.Active)
            throw new InvalidOperationException("Only active users can be suspended");

        Status = UserStatus.Suspended;
        ApprovedBy = suspendedBy;
        ApprovedAt = DateTime.UtcNow;
        RejectionReason = reason;
    }

    public void Reactivate(string reactivatedBy)
    {
        if (Status != UserStatus.Suspended)
            throw new InvalidOperationException("Only suspended users can be reactivated");

        Status = UserStatus.Active;
        ApprovedBy = reactivatedBy;
        ApprovedAt = DateTime.UtcNow;
        RejectionReason = null;
    }

    // Role management
    public void AssignRole(Guid roleId, string assignedBy)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId))
            return; // already assigned

        _userRoles.Add(new UserRole(Id, roleId, assignedBy));
    }

    public void RemoveRole(Guid roleId)
    {
        var role = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (role != null)
            _userRoles.Remove(role);
    }

    public bool IsInRole(string roleName)
    {
        // This requires loading roles; we can use a method that expects roles already loaded
        // or we can implement a check after querying. For simplicity, we'll keep it as is.
        // Usually this is done via repository with includes.
        return _userRoles.Any(ur => ur.Role.Name == roleName);
    }

    public bool CanStartCharging() => true; // Simplified

    public void UpdateStatistics(decimal totalEnergy, decimal totalMoney)
    {
        // Update user consumption statistics
    }
    public void SetPassword(string password)
    {
        // Generate a salt and hash the password
        using var hmac = new System.Security.Cryptography.HMACSHA512();
        PasswordSalt = Convert.ToBase64String(hmac.Key);
        PasswordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }
    public bool VerifyPassword(string password)
    {
        if (string.IsNullOrEmpty(PasswordHash) || string.IsNullOrEmpty(PasswordSalt))
            return false;

        using var hmac = new System.Security.Cryptography.HMACSHA512(Convert.FromBase64String(PasswordSalt));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        var storedHash = Convert.FromBase64String(PasswordHash);
        return computedHash.SequenceEqual(storedHash);
    }
}