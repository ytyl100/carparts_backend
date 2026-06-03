using System.Collections.Generic;
using System.Threading.Tasks;
using CarPartsInventory.API.Models;
using CarPartsInventory.API.Models.DTOs;

namespace CarPartsInventory.API.Services
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(RegisterRequest request);
        Task<AuthResponse> AuthenticateUserAsync(LoginRequest request);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request);
        Task<User> GetUserByIdAsync(string id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> UpdateUserAsync(string id, User user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> VerifyPasswordResetTokenAsync(string email, string token);
        Task<bool> UnlockUserAccountAsync(string userId);
        Task<bool> UpdateUserRoleAsync(string userId, string newRole);
    }
}