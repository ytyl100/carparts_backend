using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChargingStationManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CarDrivers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public AuthController(
            IAuthService authService,
            IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var (user, roles, token) = await _authService.AuthenticateWithDetailsAsync(request.Username, request.Password);

                var resp = new LoginResponse
                {
                    Id = user.Id,
                    UserId = user.UserId,
                    Name = user.Name,
                    Status = user.Status.ToString(),
                    RegisteredAt = user.RegisteredAt,
                    ApprovedBy = user.ApprovedBy,
                    ApprovedAt = user.ApprovedAt,
                    RejectionReason = user.RejectionReason,
                    Roles = roles.Select(r => new LoginRoleInfo { RoleId = r.RoleId, RoleName = r.RoleName }),
                    Token = token
                };

                return Ok(resp);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginRoleInfo
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = null!;
    }

    public class LoginResponse
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime RegisteredAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public IEnumerable<LoginRoleInfo> Roles { get; set; } = Array.Empty<LoginRoleInfo>();
        public string Token { get; set; } = string.Empty;
    }
}