using System;
using System.Threading.Tasks;
using CarPartsInventory.API.Models.DTOs;
using CarPartsInventory.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarPartsInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _userService.RegisterUserAsync(request);

                var response = new AuthResponse
                {
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    FullName = $"{user.FirstName} {user.LastName}",
                    IsAuthenticated = false, // 需要登录
                    Message = "Registration successful. Please login.",
                    TokenExpiry = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "An error occurred during registration." });
            }
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var authResponse = await _userService.AuthenticateUserAsync(request);

                if (!authResponse.IsAuthenticated)
                {
                    return Unauthorized(new { message = authResponse.Message });
                }

                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "An error occurred during login." });
            }
        }

        // POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var result = await _userService.ForgotPasswordAsync(request.Email);

                if (result)
                {
                    // 出于安全考虑，即使邮箱不存在也返回相同的信息
                    return Ok(new { message = "If an account exists with this email, a password reset link has been sent." });
                }

                return BadRequest(new { message = "Failed to process forgot password request." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password");
                return StatusCode(500, new { message = "An error occurred processing your request." });
            }
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var result = await _userService.ResetPasswordAsync(request);

                if (result)
                {
                    return Ok(new { message = "Password has been reset successfully." });
                }

                return BadRequest(new { message = "Failed to reset password. Token may be invalid or expired." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return StatusCode(500, new { message = "An error occurred resetting your password." });
            }
        }

        // POST: api/auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                // 从JWT令牌中获取用户ID
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _userService.ChangePasswordAsync(userId, request);

                if (result)
                {
                    return Ok(new { message = "Password changed successfully." });
                }

                return BadRequest(new { message = "Failed to change password." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { message = "An error occurred changing your password." });
            }
        }

        // GET: api/auth/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                var profile = new UserProfile
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { message = "An error occurred getting your profile." });
            }
        }

        // POST: api/auth/verify-token
        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyPasswordResetToken([FromBody] VerifyTokenRequest request)
        {
            try
            {
                var isValid = await _userService.VerifyPasswordResetTokenAsync(request.Email, request.Token);

                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying token");
                return StatusCode(500, new { message = "An error occurred verifying the token." });
            }
        }
    }

    public class VerifyTokenRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}