// ChargingStationManagement.Infrastructure/Identity/PasswordHasher.cs
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace ChargingStationManagement.Infrastructure.Identity
{
    public interface IPasswordHasher
    {
        (string Hash, string Salt) HashPassword(string password);
        bool VerifyPassword(string password, string hash, string salt);
        string GenerateSecureRandomString(int length);
    }

    public class PasswordHasher : IPasswordHasher
    {
        private readonly ILogger<PasswordHasher> _logger;

        public PasswordHasher(ILogger<PasswordHasher> logger)
        {
            _logger = logger;
        }

        public (string Hash, string Salt) HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("密码不能为空", nameof(password));
            }

            try
            {
                // 生成随机盐
                var salt = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                // 使用PBKDF2算法生成哈希
                var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                return (hash, Convert.ToBase64String(salt));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "密码哈希失败");
                throw;
            }
        }

        public bool VerifyPassword(string password, string hash, string salt)
        {
            if (string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(hash) ||
                string.IsNullOrWhiteSpace(salt))
            {
                return false;
            }

            try
            {
                var saltBytes = Convert.FromBase64String(salt);

                var computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: saltBytes,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                return hash == computedHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "密码验证失败");
                return false;
            }
        }

        public string GenerateSecureRandomString(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentException("长度必须大于0", nameof(length));
            }

            try
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_-+=<>?";
                var randomBytes = new byte[length];

                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomBytes);
                }

                var result = new char[length];
                for (int i = 0; i < length; i++)
                {
                    result[i] = chars[randomBytes[i] % chars.Length];
                }

                return new string(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成随机字符串失败");
                throw;
            }
        }

        public string GenerateVerificationCode(int length = 6)
        {
            try
            {
                var random = new Random();
                var code = new StringBuilder(length);

                for (int i = 0; i < length; i++)
                {
                    code.Append(random.Next(0, 10));
                }

                return code.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成验证码失败");
                throw;
            }
        }
    }
}