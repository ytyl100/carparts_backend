// ChargingStationManagement.Domain/Interfaces/IWalletService.cs
using System.Threading.Tasks;
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;

namespace ChargingStationManagement.Domain.Interfaces
{
    /// <summary>
    /// 钱包领域服务接口
    /// </summary>
    public interface IWalletService
    {
        Task<Wallet> CreateWalletAsync(Guid userId);
        Task<Transaction> RechargeAsync(
            Guid walletId,
            decimal amount,
            PaymentMethod method,
            string referenceId,
            string operatorId = null);

        Task<Transaction> ConsumeAsync(
            Guid walletId,
            decimal amount,
            string sessionId,
            string description);

        Task<Transaction> RefundAsync(
            Guid walletId,
            decimal amount,
            string originalTransactionId,
            string reason);

        Task FreezeBalanceAsync(Guid walletId, decimal amount, string reason);
        Task UnfreezeBalanceAsync(Guid walletId, decimal amount, string reason);
        Task<bool> HasSufficientBalanceAsync(Guid walletId, decimal amount);
        Task<decimal> GetAvailableBalanceAsync(Guid walletId);
        Task SetSpendingLimitsAsync(Guid walletId, decimal dailyLimit, decimal singleLimit);
        Task EnableAutoRechargeAsync(Guid walletId, decimal threshold, decimal amount);
        Task<bool> CheckBalanceAsync(string userId, int v);
        Task<decimal> GetAvailableBalanceAsync(string userId);
    }
}