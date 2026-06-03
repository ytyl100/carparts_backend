using System;
using System.Collections.Generic;

namespace ChargingStationManagement.Domain.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    
    // 🔥 添加 CreatedAt 属性
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private Role() { } // EF Core

    public Role(string name, string description = "")
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow; // 🔥 设置创建时间
    }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void AssignToUser(Guid userId, string assignedBy)
    {
        if (_userRoles.Any(ur => ur.UserId == userId))
            return; // Already assigned

        _userRoles.Add(new UserRole(userId, Id, assignedBy));
    }

    public void RemoveFromUser(Guid userId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.UserId == userId);
        if (userRole != null)
            _userRoles.Remove(userRole);
    }
}