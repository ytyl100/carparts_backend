// ChargingStationManagement.Infrastructure/Identity/JwtTokenService.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChargingStationManagement.Infrastructure.Identity
{
    public interface IJwtTokenService
    {
        string GenerateToken(string userId, string phoneNumber, string userName, string[] roles);
        ClaimsPrincipal ValidateToken(string token);
        string GetUserIdFromToken(string token);
        bool IsTokenValid(string token);
        TokenValidationResult ValidateTokenWithResult(string token);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(
            IOptions<JwtSettings> jwtSettings,
            ILogger<JwtTokenService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public string GenerateToken(string userId, string phoneNumber, string userName, string[] roles)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userId),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.MobilePhone, phoneNumber ?? string.Empty),
                    new Claim(ClaimTypes.Name, userName ?? "Anonymous"),
                    new Claim("userId", userId)
                };

                // 添加角色声明
                if (roles != null && roles.Length > 0)
                {
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成JWT令牌失败");
                throw;
            }
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("JWT令牌已过期");
                throw;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("JWT令牌签名无效");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证JWT令牌失败");
                throw;
            }
        }

        public string GetUserIdFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "userId");
                return userIdClaim?.Value;
            }
            catch
            {
                return null;
            }
        }

        public bool IsTokenValid(string token)
        {
            try
            {
                ValidateToken(token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public TokenValidationResult ValidateTokenWithResult(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                return new TokenValidationResult
                {
                    IsValid = true,
                    Principal = principal,
                    ValidatedToken = validatedToken
                };
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning("JWT令牌已过期");
                return new TokenValidationResult
                {
                    IsValid = false,
                    Exception = ex,
                    Error = TokenValidationError.Expired
                };
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning("JWT令牌签名无效");
                return new TokenValidationResult
                {
                    IsValid = false,
                    Exception = ex,
                    Error = TokenValidationError.InvalidSignature
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证JWT令牌失败");
                return new TokenValidationResult
                {
                    IsValid = false,
                    Exception = ex,
                    Error = TokenValidationError.Other
                };
            }
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpireMinutes { get; set; } = 120;
        public int RefreshTokenExpireDays { get; set; } = 7;
    }

    public class TokenValidationResult
    {
        public bool IsValid { get; set; }
        public ClaimsPrincipal Principal { get; set; }
        public SecurityToken ValidatedToken { get; set; }
        public Exception Exception { get; set; }
        public TokenValidationError Error { get; set; }
    }

    public enum TokenValidationError
    {
        None,
        Expired,
        InvalidSignature,
        InvalidIssuer,
        InvalidAudience,
        Other
    }
}