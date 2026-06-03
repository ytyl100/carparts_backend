using ChargingStationManagement.Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingStationManagement.Services.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(string roleId);
    Task<RoleDto> CreateRoleAsync(string actorUserId, CreateRoleRequest request);
    Task<RoleDto> UpdateRoleAsync(string actorUserId, string roleId, UpdateRoleRequest request);
    Task DeleteRoleAsync(string actorUserId, string roleId);
    Task<IEnumerable<UserDto>> GetUsersInRoleAsync(string roleId);
}