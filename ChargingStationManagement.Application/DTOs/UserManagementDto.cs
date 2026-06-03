using ChargingStationManagement.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ChargingStationManagement.Services.DTOs;

// ============= User DTOs =============
public class UserDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public List<RoleDto> Roles { get; set; } = new();
}

public class CreateUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Password { get; set; }
}

public class ApproveUserRequest
{
    public string ApprovedBy { get; set; } = string.Empty;
}

public class RejectUserRequest
{
    public string RejectedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class SuspendUserRequest
{
    public string SuspendedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class ReactivateUserRequest
{
    public string ReactivatedBy { get; set; } = string.Empty;
}

public class AssignRoleRequest
{
    public Guid RoleId { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
}

// ============= Role DTOs =============
public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // 🔥 添加这两个可选属性（用于 UserRole 关系）
    public DateTime? AssignedAt { get; set; }
    public string? AssignedBy { get; set; }
    
    // 用于 Role 管理界面
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}