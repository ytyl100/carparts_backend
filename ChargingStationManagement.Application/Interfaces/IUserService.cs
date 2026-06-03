using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Services.DTOs;

namespace ChargingStationManagement.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(Guid userId);
    Task<UserDto> GetUserByUserIdAsync(string userId);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<List<UserDto>> GetUsersByStatusAsync(UserStatus status);
    Task<List<UserDto>> GetPendingUsersAsync();
    
    // Status management methods
    Task<UserDto> ApproveUserAsync(Guid userId, string approvedBy);
    Task<UserDto> RejectUserAsync(Guid userId, string rejectedBy, string reason);
    Task<UserDto> SuspendUserAsync(Guid userId, string suspendedBy, string reason);
    Task<UserDto> ReactivateUserAsync(Guid userId, string reactivatedBy);
    
    // Role management
    Task<UserDto> AssignRoleAsync(Guid userId, Guid roleId, string assignedBy);
    Task<UserDto> RemoveRoleAsync(Guid userId, Guid roleId);
    
    // User creation
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<bool> DeleteUserAsync(Guid userId);
}