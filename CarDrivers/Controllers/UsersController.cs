using ChargingStationManagement.Application.Interfaces;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Services.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChargingStationManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid userId)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(userId);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving user" });
        }
    }

    /// <summary>
    /// Get user by UserId (string)
    /// </summary>
    [HttpGet("by-userid/{userId}")]
    public async Task<ActionResult<UserDto>> GetUserByUserId(string userId)
    {
        try
        {
            var user = await _userService.GetUserByUserIdAsync(userId);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving user" });
        }
    }

    /// <summary>
    /// Get current logged-in user
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                ?? User.FindFirstValue(ClaimTypes.Name);
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var user = await _userService.GetUserByUserIdAsync(userId);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred while retrieving user" });
        }
    }

    /// <summary>
    /// Get users by status
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<List<UserDto>>> GetUsersByStatus(UserStatus status)
    {
        try
        {
            var users = await _userService.GetUsersByStatusAsync(status);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by status {Status}", status);
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Get pending users (waiting for approval)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<List<UserDto>>> GetPendingUsers()
    {
        try
        {
            var users = await _userService.GetPendingUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending users");
            return StatusCode(500, new { message = "An error occurred while retrieving pending users" });
        }
    }

    /// <summary>
    /// Approve a pending user
    /// </summary>
    [HttpPost("{userId:guid}/approve")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<UserDto>> ApproveUser(
        Guid userId, 
        [FromBody] ApproveUserRequest request)
    {
        try
        {
            var user = await _userService.ApproveUserAsync(userId, request.ApprovedBy);
            return Ok(new 
            { 
                message = "User approved successfully",
                user 
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while approving user" });
        }
    }

    /// <summary>
    /// Reject a pending user
    /// </summary>
    [HttpPost("{userId:guid}/reject")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<UserDto>> RejectUser(
        Guid userId, 
        [FromBody] RejectUserRequest request)
    {
        try
        {
            var user = await _userService.RejectUserAsync(userId, request.RejectedBy, request.Reason);
            return Ok(new 
            { 
                message = "User rejected successfully",
                user 
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while rejecting user" });
        }
    }

    /// <summary>
    /// Suspend an active user
    /// </summary>
    [HttpPost("{userId:guid}/suspend")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<UserDto>> SuspendUser(
        Guid userId, 
        [FromBody] SuspendUserRequest request)
    {
        try
        {
            var user = await _userService.SuspendUserAsync(userId, request.SuspendedBy, request.Reason);
            return Ok(new 
            { 
                message = "User suspended successfully",
                user 
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while suspending user" });
        }
    }

    /// <summary>
    /// Reactivate a suspended user
    /// </summary>
    [HttpPost("{userId:guid}/reactivate")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<UserDto>> ReactivateUser(
        Guid userId, 
        [FromBody] ReactivateUserRequest request)
    {
        try
        {
            var user = await _userService.ReactivateUserAsync(userId, request.ReactivatedBy);
            return Ok(new 
            { 
                message = "User reactivated successfully",
                user 
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while reactivating user" });
        }
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    [HttpPost("{userId:guid}/roles")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<UserDto>> AssignRole(
        Guid userId, 
        [FromBody] AssignRoleRequest request)
    {
        try
        {
            var user = await _userService.AssignRoleAsync(userId, request.RoleId, request.AssignedBy);
            return Ok(new 
            { 
                message = "Role assigned successfully",
                user 
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while assigning role" });
        }
    }

    /// <summary>
    /// Remove role from user
    /// </summary>
    [HttpDelete("{userId:guid}/roles/{roleId:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<UserDto>> RemoveRole(Guid userId, Guid roleId)
    {
        try
        {
            var user = await _userService.RemoveRoleAsync(userId, roleId);
            return Ok(new 
            { 
                message = "Role removed successfully",
                user 
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while removing role" });
        }
    }

    /// <summary>
    /// Create new user
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var user = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "An error occurred while creating user" });
        }
    }

    /// <summary>
    /// Delete user
    /// </summary>
    [HttpDelete("{userId:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(userId);
            if (!result)
                return NotFound(new { message = "User not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while deleting user" });
        }
    }

    /// <summary>
    /// Debug endpoint - Get claims
    /// </summary>
    [HttpGet("_debug/claims")]
    [AllowAnonymous]
    public IActionResult GetClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            UserName = User.Identity?.Name,
            Claims = claims,
            Roles = roles
        });
    }
}