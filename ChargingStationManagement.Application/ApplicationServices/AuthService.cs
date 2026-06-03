// Services/ApplicationServices/AuthService.cs
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Interfaces;
using ChargingStationManagement.Services.Interfaces;
using ChargingStationManagement.Services.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChargingStationManagement.Services.ApplicationServices
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IRepository<User> userRepository,
            IRepository<UserRole> userRoleRepository,
            IRepository<Role> roleRepository,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<string> AuthenticateAsync(string username, string password)
        {
            var (_, _, token) = await AuthenticateWithDetailsAsync(username, password);
            return token;
        }

        public async Task<(User user, IEnumerable<RoleInfo> roles, string token)> AuthenticateWithDetailsAsync(string username, string password)
        {
            var user = await _userRepository.Query()
                .FirstOrDefaultAsync(u => u.UserId == username);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");
            if (!user.VerifyPassword(password))
                throw new UnauthorizedAccessException("Invalid credentials");
            if (user.Status != UserStatus.Active)
                throw new UnauthorizedAccessException("Account is not active");

            var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == user.Id);
            var roleIds = userRoles.Select(ur => ur.RoleId).Distinct();
            var roleInfos = new List<RoleInfo>();

            foreach (var roleId in roleIds)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role != null)
                    roleInfos.Add(new RoleInfo(role.Id, role.Name));
            }

            var token = GenerateJwtToken(user.UserId, roleInfos.Select(r => r.RoleName));
            return (user, roleInfos, token);
        }

        private string GenerateJwtToken(string username, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var secretKey = _jwtSettings.SecretKey;
            if (Encoding.UTF8.GetBytes(secretKey).Length < 32)
                throw new InvalidOperationException("JWT SecretKey must be at least 256 bits");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            _logger.LogInformation("JWT token generated for user {Username}, expires at {Expiry}", 
                username, token.ValidTo);

            return tokenString;
        }
    }
}