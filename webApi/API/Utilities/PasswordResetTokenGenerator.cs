using System;
using System.Security.Cryptography;
using System.Text;

namespace CarPartsInventory.API.Utilities
{
    public static class PasswordResetTokenGenerator
    {
        public static string GenerateToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData)
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Replace("=", "");
            }
        }

        public static string GenerateNumericToken(int length = 6)
        {
            var random = new Random();
            var token = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                token.Append(random.Next(0, 10));
            }

            return token.ToString();
        }
    }
}