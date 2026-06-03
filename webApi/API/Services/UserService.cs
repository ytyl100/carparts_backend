using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarPartsInventory.API.Models;
using CarPartsInventory.API.Models.DTOs;
using CarPartsInventory.API.Utilities;

namespace CarPartsInventory.API.Services
{
    public class UserService : IUserService
    {
        private readonly IJsonFileService<User> _jsonFileService;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly EmailService _emailService;

        public UserService(
            IJsonFileService<User> jsonFileService,
            JwtTokenGenerator jwtTokenGenerator,
            EmailService emailService)
        {
            _jsonFileService = jsonFileService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
        }

        public async Task<User> RegisterUserAsync(RegisterRequest request)
        {
            // 检查用户名是否已存在
            var existingUserByUsername = await GetUserByUsernameAsync(request.Username);
            if (existingUserByUsername != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            // 检查邮箱是否已存在
            var existingUserByEmail = await GetUserByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                throw new InvalidOperationException("Email already registered.");
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                Role = "User", // 默认角色
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            return await _jsonFileService.CreateAsync(user);
        }

        public async Task<AuthResponse> AuthenticateUserAsync(LoginRequest request)
        {
            var user = await GetUserByUsernameAsync(request.Username) ??
                      await GetUserByEmailAsync(request.Username);

            if (user == null)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "Invalid username or password."
                };
            }

            // 检查账户是否被锁定
            if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = $"Account is locked until {user.LockedUntil.Value:yyyy-MM-dd HH:mm}"
                };
            }

            // 检查账户是否激活
            if (!user.IsActive)
            {
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "Account is deactivated."
                };
            }

            // 验证密码
            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                // 增加失败登录尝试次数
                user.FailedLoginAttempts++;

                // 如果失败次数达到阈值，锁定账户
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                    await _jsonFileService.UpdateAsync(user.Id, user);

                    return new AuthResponse
                    {
                        IsAuthenticated = false,
                        Message = "Too many failed login attempts. Account locked for 30 minutes."
                    };
                }

                await _jsonFileService.UpdateAsync(user.Id, user);
                return new AuthResponse
                {
                    IsAuthenticated = false,
                    Message = "Invalid username or password."
                };
            }

            // 登录成功，重置失败计数
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockedUntil = null;
            user.LastLoginDate = DateTime.UtcNow;
            await _jsonFileService.UpdateAsync(user.Id, user);

            // 生成JWT令牌
            var token = _jwtTokenGenerator.GenerateToken(user);
            var tokenExpiry = DateTime.UtcNow.AddMinutes(60); // 假设令牌有效期为60分钟

            return new AuthResponse
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                FullName = $"{user.FirstName} {user.LastName}",
                IsAuthenticated = true,
                Message = "Login successful",
                TokenExpiry = tokenExpiry
            };
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null)
            {
                // 出于安全考虑，即使邮箱不存在也返回成功
                return true;
            }

            // 生成密码重置令牌
            var resetToken = PasswordResetTokenGenerator.GenerateToken();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // 令牌有效期1小时

            await _jsonFileService.UpdateAsync(user.Id, user);

            // 发送密码重置邮件
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, user.FirstName);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // 验证令牌
            if (string.IsNullOrEmpty(user.PasswordResetToken) ||
                user.PasswordResetToken != request.Token ||
                !user.PasswordResetTokenExpiry.HasValue ||
                user.PasswordResetTokenExpiry.Value < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Invalid or expired reset token.");
            }

            // 更新密码
            user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            user.LastPasswordChangeDate = DateTime.UtcNow;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockedUntil = null;

            await _jsonFileService.UpdateAsync(user.Id, user);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // 验证当前密码
            if (!PasswordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Current password is incorrect.");
            }

            // 更新密码
            user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            user.LastPasswordChangeDate = DateTime.UtcNow;

            await _jsonFileService.UpdateAsync(user.Id, user);
            return true;
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _jsonFileService.GetByIdAsync(id);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var users = await _jsonFileService.GetAllAsync();
            return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var users = await _jsonFileService.GetAllAsync();
            return users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _jsonFileService.GetAllAsync();
        }

        public async Task<User> UpdateUserAsync(string id, User updatedUser)
        {
            return await _jsonFileService.UpdateAsync(id, updatedUser);
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _jsonFileService.DeleteAsync(id);
        }

        public async Task<bool> VerifyPasswordResetTokenAsync(string email, string token)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null)
                return false;

            return !string.IsNullOrEmpty(user.PasswordResetToken) &&
                   user.PasswordResetToken == token &&
                   user.PasswordResetTokenExpiry.HasValue &&
                   user.PasswordResetTokenExpiry.Value > DateTime.UtcNow;
        }

        public async Task<bool> UnlockUserAccountAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return false;

            user.IsLocked = false;
            user.LockedUntil = null;
            user.FailedLoginAttempts = 0;

            await _jsonFileService.UpdateAsync(user.Id, user);
            return true;
        }

        public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return false;

            var validRoles = new[] { "Admin", "Manager", "User" };
            if (!validRoles.Contains(newRole))
                return false;

            user.Role = newRole;
            await _jsonFileService.UpdateAsync(user.Id, user);
            return true;
        }
    }
}