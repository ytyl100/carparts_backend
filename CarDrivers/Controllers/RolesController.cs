using ChargingStationManagement.Services.DTOs;
using ChargingStationManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChargingStationManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [Authorize(Roles = "admin,super_admin")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{roleId}")]
        [Authorize(Roles = "admin,super_admin")]
        public async Task<IActionResult> GetRole(string roleId)
        {
            var role = await _roleService.GetRoleByIdAsync(roleId);
            if (role == null)
                return NotFound();
            return Ok(role);
        }

        [HttpPost]
        [Authorize(Roles = "admin,super_admin")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
            var role = await _roleService.CreateRoleAsync(currentUserId, request);
            return CreatedAtAction(nameof(GetRole), new { roleId = role.Id }, role);
        }

        [HttpPut("{roleId}")]
        [Authorize(Roles = "admin,super_admin")]
        public async Task<IActionResult> UpdateRole(string roleId, [FromBody] UpdateRoleRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
            var role = await _roleService.UpdateRoleAsync(currentUserId, roleId, request);
            return Ok(role);
        }

        [HttpDelete("{roleId}")]
        [Authorize(Roles = "admin,super_admin")]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
            await _roleService.DeleteRoleAsync(currentUserId, roleId);
            return NoContent();
        }

        [HttpGet("{roleId}/users")]
        [Authorize(Roles = "admin,super_admin")]
        public async Task<IActionResult> GetUsersInRole(string roleId)
        {
            var users = await _roleService.GetUsersInRoleAsync(roleId);
            return Ok(users);
        }

        [HttpGet("debug/claims")]
        [AllowAnonymous]
        public IActionResult GetClaims()
        {
            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            });

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToList();

            return Ok(new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                AllClaims = claims,
                Roles = roles,
                RoleClaimType = ClaimTypes.Role
            });
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            return Ok(new 
            { 
                Message = "RolesController is reachable!",
                ServerTime = DateTime.UtcNow,
                ServerTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
        }

        [HttpGet("test-auth")]
        [Authorize]
        public IActionResult TestAuth()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(new
            {
                User = User.Identity?.Name,
                IsAuthenticated = User.Identity?.IsAuthenticated,
                Claims = claims
            });
        }

        [HttpGet("test-role")]
        [Authorize(Roles = "admin")]
        public IActionResult TestRole()
        {
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);

            return Ok(new
            {
                User = User.Identity?.Name,
                Roles = roles,
                Message = "Admin role verified!"
            });
        }
    }
}