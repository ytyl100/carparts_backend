using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Interfaces;
using ChargingStationManagement.Services.DTOs;
using ChargingStationManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargingStationManagement.Services.ApplicationServices
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<User> _userRepository;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            IRepository<Role> roleRepository,
            IRepository<UserRole> userRoleRepository,
            IRepository<User> userRepository,
            ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<RoleDto>> GetRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            var dtos = new List<RoleDto>();

            foreach (var role in roles)
            {
                var userRoles = await _userRoleRepository.FindAsync(ur => ur.RoleId == role.Id);
                var userCount = userRoles.Count;

                dtos.Add(new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    UserCount = userCount,
                    CreatedAt = role.CreatedAt
                });
            }

            return dtos;
        }

        public async Task<RoleDto?> GetRoleByIdAsync(string roleId)
        {
            if (!Guid.TryParse(roleId, out var id))
            {
                throw new ArgumentException("Invalid role ID format", nameof(roleId));
            }

            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return null;

            var userRoles = await _userRoleRepository.FindAsync(ur => ur.RoleId == role.Id);
            var userCount = userRoles.Count;

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                UserCount = userCount,
                CreatedAt = role.CreatedAt
            };
        }

        public async Task<RoleDto> CreateRoleAsync(string actorUserId, CreateRoleRequest request)
        {
            // 🔥 修复：使用 Query() + EF Core 的 FirstOrDefaultAsync
            var existing = await _roleRepository.Query()
                .FirstOrDefaultAsync(r => r.Name == request.Name);
            
            if (existing != null)
                throw new InvalidOperationException($"Role with name '{request.Name}' already exists");

            var role = new Role(request.Name, request.Description);
            var createdRole = await _roleRepository.AddAsync(role);

            _logger.LogInformation(
                "Role {RoleName} created by {ActorUserId}", 
                role.Name, actorUserId);

            return new RoleDto
            {
                Id = createdRole.Id,
                Name = createdRole.Name,
                Description = createdRole.Description,
                UserCount = 0,
                CreatedAt = createdRole.CreatedAt
            };
        }

        public async Task<RoleDto> UpdateRoleAsync(string actorUserId, string roleId, UpdateRoleRequest request)
        {
            if (!Guid.TryParse(roleId, out var id))
            {
                throw new ArgumentException("Invalid role ID format", nameof(roleId));
            }

            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                throw new ArgumentException($"Role {roleId} not found");

            if (role.Name != request.Name)
            {
                // 🔥 修复：使用 Query() + EF Core 的 FirstOrDefaultAsync
                var existing = await _roleRepository.Query()
                    .FirstOrDefaultAsync(r => r.Name == request.Name);
                
                if (existing != null)
                    throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
            }

            role.Update(request.Name, request.Description);
            await _roleRepository.UpdateAsync(role);

            _logger.LogInformation(
                "Role {RoleId} updated by {ActorUserId}", 
                roleId, actorUserId);

            var userRoles = await _userRoleRepository.FindAsync(ur => ur.RoleId == role.Id);
            var userCount = userRoles.Count;

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                UserCount = userCount,
                CreatedAt = role.CreatedAt
            };
        }

        public async Task DeleteRoleAsync(string actorUserId, string roleId)
        {
            if (!Guid.TryParse(roleId, out var id))
            {
                throw new ArgumentException("Invalid role ID format", nameof(roleId));
            }

            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                throw new ArgumentException($"Role {roleId} not found");

            var systemRoles = new[] { "normal", "contributor", "admin", "super_admin" };
            if (systemRoles.Contains(role.Name.ToLower()))
                throw new InvalidOperationException("Cannot delete system roles");

            var userRoles = await _userRoleRepository.FindAsync(ur => ur.RoleId == role.Id);
            if (userRoles.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete role '{role.Name}' because it has {userRoles.Count} assigned user(s)");
            }

            await _roleRepository.DeleteAsync(role);

            _logger.LogInformation(
                "Role {RoleId} ({RoleName}) deleted by {ActorUserId}", 
                roleId, role.Name, actorUserId);
        }

        public async Task<IEnumerable<UserDto>> GetUsersInRoleAsync(string roleId)
        {
            if (!Guid.TryParse(roleId, out var id))
            {
                throw new ArgumentException("Invalid role ID format", nameof(roleId));
            }

            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                throw new ArgumentException($"Role {roleId} not found");

            var userRoles = await _userRoleRepository.FindAsync(ur => ur.RoleId == role.Id);
            var userIds = userRoles.Select(ur => ur.UserId).Distinct();
            
            var users = new List<UserDto>();
            foreach (var uid in userIds)
            {
                var user = await _userRepository.GetByIdAsync(uid, 
                    query => query.Include(u => u.UserRoles).ThenInclude(ur => ur.Role));
                
                if (user != null)
                {
                    users.Add(new UserDto
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
                            Description = ur.Role.Description,
                            UserCount = 0,
                            CreatedAt = ur.Role.CreatedAt
                        }).ToList()
                    });
                }
            }

            return users;
        }
    }
}