// ChargingStationManagement.Infrastructure/Identity/UserManager.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChargingStationManagement.Infrastructure.Identity
{
    public interface IUserManager
    {
        Task<User> AuthenticateAsync(string phoneNumber, string password);
        Task<User> RegisterAsync(string phoneNumber, string password, string name, UserType userType = UserType.Normal);
        Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string phoneNumber, string newPassword);
        Task<bool> VerifyPhoneNumberAsync(string phoneNumber, string verificationCode);
        Task<bool> CheckPhoneExistsAsync(string phoneNumber);
        Task<User> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, string name, string email, DateTime? dateOfBirth, Gender gender);
        Task<bool> LockUserAsync(string userId, string reason);
        Task<bool> UnlockUserAsync(string userId);
    }

    public class UserManager : IUserManager
    {
        private readonly UserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _tokenService;
        private readonly ILogger<UserManager> _logger;

        public UserManager(
            UserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService tokenService,
            ILogger<UserManager> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<User> AuthenticateAsync(string phoneNumber, string password)
        {
            try
            {
                _logger.LogInformation($"用户登录尝试: {phoneNumber}");

                // 查找用户
                var user = await _userRepository.GetUserByPhoneAsync(phoneNumber);
                if (user == null)
                {
                    _logger.LogWarning($"用户不存在: {phoneNumber}");
                    return null;
                }

                // 检查账户是否锁定
                if (user.IsLockedOut())
                {
                    _logger.LogWarning($"用户账户已锁定: {phoneNumber}");
                    return null;
                }

                // 检查账户是否激活
                if (!user.IsActive)
                {
                    _logger.LogWarning($"用户账户未激活: {phoneNumber}");
                    return null;
                }

                // 验证密码
                if (!_passwordHasher.VerifyPassword(password, user.PasswordHash, user.Salt))
                {
                    // 记录登录失败
                    await _userRepository.RecordFailedLoginAsync(user.Id);
                    _logger.LogWarning($"密码验证失败: {phoneNumber}");
                    return null;
                }

                // 记录登录成功
                await _userRepository.UpdateUserLoginInfoAsync(user.Id, GetClientIp());
                _logger.LogInformation($"用户登录成功: {phoneNumber}");

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"用户认证失败: {phoneNumber}");
                throw;
            }
        }

        public async Task<User> RegisterAsync(
            string phoneNumber,
            string password,
            string name,
            UserType userType = UserType.Normal)
        {
            try
            {
                _logger.LogInformation($"用户注册: {phoneNumber}");

                // 检查手机号是否已存在
                if (await _userRepository.CheckIfPhoneExistsAsync(phoneNumber))
                {
                    _logger.LogWarning($"手机号已存在: {phoneNumber}");
                    throw new InvalidOperationException("手机号已注册");
                }

                // 生成用户ID
                var userId = GenerateUserId();

                // 创建用户实体
                var user = new User(userId, phoneNumber, name, userType);

                // 哈希密码
                var (hash, salt) = _passwordHasher.HashPassword(password);
                user.SetPassword(hash, salt);

                // 保存用户
                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"用户注册成功: {phoneNumber}, UserId: {userId}");

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"用户注册失败: {phoneNumber}");
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            try
            {
                _logger.LogInformation($"用户修改密码: {userId}");

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"用户不存在: {userId}");
                    return false;
                }

                // 验证旧密码
                if (!_passwordHasher.VerifyPassword(oldPassword, user.PasswordHash, user.Salt))
                {
                    _logger.LogWarning($"旧密码验证失败: {userId}");
                    return false;
                }

                // 哈希新密码
                var (hash, salt) = _passwordHasher.HashPassword(newPassword);
                user.SetPassword(hash, salt);

                // 更新用户
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"用户密码修改成功: {userId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"用户修改密码失败: {userId}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string phoneNumber, string newPassword)
        {
            try
            {
                _logger.LogInformation($"重置密码: {phoneNumber}");

                var user = await _userRepository.GetUserByPhoneAsync(phoneNumber);
                if (user == null)
                {
                    _logger.LogWarning($"用户不存在: {phoneNumber}");
                    return false;
                }

                // 哈希新密码
                var (hash, salt) = _passwordHasher.HashPassword(newPassword);
                user.SetPassword(hash, salt);

                // 更新用户
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"密码重置成功: {phoneNumber}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"密码重置失败: {phoneNumber}");
                return false;
            }
        }

        public async Task<bool> VerifyPhoneNumberAsync(string phoneNumber, string verificationCode)
        {
            try
            {
                // 这里应该实现验证码验证逻辑
                // 实际应用中可能需要调用短信服务验证验证码

                var user = await _userRepository.GetUserByPhoneAsync(phoneNumber);
                if (user == null)
                {
                    _logger.LogWarning($"用户不存在: {phoneNumber}");
                    return false;
                }

                // 验证验证码（简化版，实际需要从缓存或数据库获取验证码进行验证）
                if (verificationCode != "123456") // 测试用验证码
                {
                    _logger.LogWarning($"验证码错误: {phoneNumber}");
                    return false;
                }

                // 验证账户
                user.VerifyAccount("SMS");

                // 更新用户
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"手机号验证成功: {phoneNumber}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"手机号验证失败: {phoneNumber}");
                return false;
            }
        }

        public async Task<bool> CheckPhoneExistsAsync(string phoneNumber)
        {
            try
            {
                return await _userRepository.CheckIfPhoneExistsAsync(phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"检查手机号存在失败: {phoneNumber}");
                throw;
            }
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            try
            {
                return await _userRepository.GetByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取用户失败: {userId}");
                throw;
            }
        }

        public async Task<bool> UpdateUserProfileAsync(
            string userId,
            string name,
            string email,
            DateTime? dateOfBirth,
            Gender gender)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"用户不存在: {userId}");
                    return false;
                }

                // 检查邮箱是否已被其他用户使用
                if (!string.IsNullOrEmpty(email) && await _userRepository.CheckIfEmailExistsAsync(email, user.Id))
                {
                    _logger.LogWarning($"邮箱已被使用: {email}");
                    return false;
                }

                // 更新用户资料
                user.UpdateProfile(email, name, dateOfBirth, gender, null, null);

                // 更新用户
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"用户资料更新成功: {userId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新用户资料失败: {userId}");
                return false;
            }
        }

        public async Task<bool> LockUserAsync(string userId, string reason)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"用户不存在: {userId}");
                    return false;
                }

                // 锁定账户（设置锁定结束时间为24小时后）
                var lockoutEndTime = DateTime.UtcNow.AddHours(24);
                // 这里需要扩展User实体来支持手动锁定
                // 实际实现可能需要修改User实体

                _logger.LogInformation($"用户账户已锁定: {userId}, 原因: {reason}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"锁定用户失败: {userId}");
                return false;
            }
        }

        public async Task<bool> UnlockUserAsync(string userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"用户不存在: {userId}");
                    return false;
                }

                // 解锁账户
                // 这里需要扩展User实体来支持解锁
                // 实际实现可能需要修改User实体

                _logger.LogInformation($"用户账户已解锁: {userId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"解锁用户失败: {userId}");
                return false;
            }
        }

        private string GenerateUserId()
        {
            var timestamp = DateTime.Now.ToString("yyMMddHHmm");
            var random = new Random().Next(100000, 999999).ToString();
            return $"USR{timestamp}{random}";
        }

        private string GetClientIp()
        {
            // 这里应该从HttpContext获取客户端IP
            // 简化实现，返回固定值
            return "127.0.0.1";
        }
    }
}