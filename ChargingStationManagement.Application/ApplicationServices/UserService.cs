using ChargingStationManagement.Application.Interfaces;
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Interfaces;
using ChargingStationManagement.Services.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace ChargingStationManagement.Application.ApplicationServices;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId, 
            query => (query).Include(u => u.UserRoles).ThenInclude(ur => ur.Role));
        
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        return MapToDto(user);
    }

    public async Task<UserDto> GetUserByUserIdAsync(string userId)
    {
        var user = await _userRepository.Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        
        if (user == null)
            throw new KeyNotFoundException($"User with UserId {userId} not found");

        return MapToDto(user);
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderByDescending(u => u.RegisteredAt)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    public async Task<List<UserDto>> GetUsersByStatusAsync(UserStatus status)
    {
        var users = await _userRepository.Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => u.Status == status)
            .OrderByDescending(u => u.RegisteredAt)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    public async Task<List<UserDto>> GetPendingUsersAsync()
    {
        return await GetUsersByStatusAsync(UserStatus.Pending);
    }

    public async Task<UserDto> ApproveUserAsync(Guid userId, string approvedBy)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        try
        {
            // Call domain method
            user.Approve(approvedBy);
            
            // Save to database
            await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation(
                "User {UserId} ({UserName}) approved by {ApprovedBy}", 
                user.UserId, user.Name, approvedBy);

            // Reload with relationships
            var updatedUser = await _userRepository.GetByIdAsync(userId, 
                query => ((IQueryable<User>)query).Include(u => u.UserRoles).ThenInclude(ur => ur.Role));

            return MapToDto(updatedUser!);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to approve user {UserId}", user.UserId);
            throw;
        }
    }

    public async Task<UserDto> RejectUserAsync(Guid userId, string rejectedBy, string reason)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        try
        {
            // Call domain method
            user.Reject(rejectedBy, reason);
            
            // Save to database
            await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation(
                "User {UserId} ({UserName}) rejected by {RejectedBy}, Reason: {Reason}", 
                user.UserId, user.Name, rejectedBy, reason);

            // Reload with relationships
            var updatedUser = await _userRepository.GetByIdAsync(userId, 
                query => ((IQueryable<User>)query).Include(u => u.UserRoles).ThenInclude(ur => ur.Role));

            return MapToDto(updatedUser!);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to reject user {UserId}", user.UserId);
            throw;
        }
    }

    public async Task<UserDto> SuspendUserAsync(Guid userId, string suspendedBy, string reason)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        try
        {
            // Call domain method
            user.Suspend(suspendedBy, reason);
            
            // Save to database
            await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation(
                "User {UserId} ({UserName}) suspended by {SuspendedBy}, Reason: {Reason}", 
                user.UserId, user.Name, suspendedBy, reason);

            // Reload with relationships
            var updatedUser = await _userRepository.GetByIdAsync(userId, 
                query => ((IQueryable<User>)query).Include(u => u.UserRoles).ThenInclude(ur => ur.Role));

            return MapToDto(updatedUser!);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to suspend user {UserId}", user.UserId);
            throw;
        }
    }

    public async Task<UserDto> ReactivateUserAsync(Guid userId, string reactivatedBy)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        try
        {
            // Call domain method
            user.Reactivate(reactivatedBy);
            
            // Save to database
            await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation(
                "User {UserId} ({UserName}) reactivated by {ReactivatedBy}", 
                user.UserId, user.Name, reactivatedBy);

            // Reload with relationships
            var updatedUser = await _userRepository.GetByIdAsync(userId, 
                query => ((IQueryable<User>)query).Include(u => u.UserRoles).ThenInclude(ur => ur.Role));

            return MapToDto(updatedUser!);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to reactivate user {UserId}", user.UserId);
            throw;
        }
    }

    public async Task<UserDto> AssignRoleAsync(Guid userId, Guid roleId, string assignedBy)
    {
        var user = await _userRepository.GetByIdAsync(userId, 
            query => ((IQueryable<User>)query).Include(u => u.UserRoles).ThenInclude(ur => ur.Role));
        
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException($"Role with ID {roleId} not found");

        // Call domain method
        user.AssignRole(roleId, assignedBy);
        
        // Save to database
        await _userRepository.UpdateAsync(user);
        
        _logger.LogInformation(
            "Role {RoleName} assigned to user {UserId} by {AssignedBy}", 
            role.Name, user.UserId, assignedBy);

        // Reload with relationships
        var updatedUser = await _userRepository.GetByIdAsync(userId, 
            query => ((IQueryable<User>)query).Include(u => u.UserRoles).ThenInclude(ur => ur.Role));

        return MapToDto(updatedUser!);
    }

    public async Task<UserDto> RemoveRoleAsync(Guid userId, Guid roleId)
    {
        var user = await _userRepository.GetByIdAsync(userId, 
            query => ((IQueryable<User>)query).Include(u => u.UserRoles).ThenInclude(ur => ur.Role));
        
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        // Call domain method
        user.RemoveRole(roleId);
        
        // Save to database
        await _userRepository.UpdateAsync(user);
        
        _logger.LogInformation("Role removed from user {UserId}", user.UserId);

        // Reload with relationships
        var updatedUser = await _userRepository.GetByIdAsync(userId, 
            query => ((IQueryable<User>)query).Include(u => u.UserRoles).ThenInclude(ur => ur.Role));

        return MapToDto(updatedUser!);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        // Check if user already exists
        var existingUser = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.UserId == createUserDto.UserId);
        
        if (existingUser != null)
            throw new InvalidOperationException($"User with UserId {createUserDto.UserId} already exists");

        var user = new User(createUserDto.UserId, createUserDto.Name);
        
        if (!string.IsNullOrEmpty(createUserDto.Password))
        {
            user.SetPassword(createUserDto.Password);
        }

        await _userRepository.AddAsync(user);
        
        _logger.LogInformation("User {UserId} ({UserName}) created", user.UserId, user.Name);

        // Reload with relationships
        var createdUser = await _userRepository.GetByIdAsync(user.Id, 
            query => ((IQueryable<User>)query).Include(u => u.UserRoles).ThenInclude(ur => ur.Role));

        return MapToDto(createdUser!);
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        await _userRepository.DeleteAsync(user);
        
        _logger.LogInformation("User {UserId} deleted", user.UserId);
        
        return true;
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserId = user.UserId,
            Name = user.Name,
            Status = user.Status,
            RegisteredAt = user.RegisteredAt,
            ApprovedBy = user.ApprovedBy,
            ApprovedAt = user.ApprovedAt,
            RejectionReason = user.RejectionReason,
            Roles = user.UserRoles.Select(ur => new RoleDto
            {
                Id = ur.Role.Id,
                Name = ur.Role.Name,
                AssignedAt = ur.AssignedAt,
                AssignedBy = ur.AssignedBy
            }).ToList()
        };
    }
}