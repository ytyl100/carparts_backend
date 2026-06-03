// ChargingStationManagement.Domain/Interfaces/INotificationService.cs
using System.Threading.Tasks;

namespace ChargingStationManagement.Domain.Interfaces
{
    /// <summary>
    /// 通知服务接口
    /// </summary>
    public interface INotificationService
    {
        Task SendLowBalanceNotificationAsync(string userId, decimal currentBalance, decimal threshold);
        Task SendChargingStartedNotificationAsync(string userId, string sessionId, string stationName);
        Task SendChargingCompletedNotificationAsync(string userId, string sessionId, decimal amount);
        Task SendChargingStoppedNotificationAsync(string userId, string sessionId, string reason);
        Task SendPaymentSuccessNotificationAsync(string userId, decimal amount, string transactionId);
        Task SendPaymentFailedNotificationAsync(string userId, decimal amount, string reason);
        Task SendSystemAlertAsync(string alertType, string message, string[] recipients);
    }
}