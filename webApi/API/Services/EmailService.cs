using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CarPartsInventory.API.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken, string firstName)
        {
            // 在实际应用中，这里应该发送真正的电子邮件
            // 这里我们只是记录日志
            _logger.LogInformation($"Password reset email sent to {email}");
            _logger.LogInformation($"Reset Token: {resetToken}");
            _logger.LogInformation($"Dear {firstName}, please use this token to reset your password.");

            await Task.CompletedTask;
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName)
        {
            _logger.LogInformation($"Welcome email sent to {email}");
            _logger.LogInformation($"Welcome {firstName}! Your account has been created successfully.");

            await Task.CompletedTask;
        }
    }
}