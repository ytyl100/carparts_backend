using System.Threading.Tasks;
using ChargingStationManagement.Services.DTOs;

namespace ChargingStationManagement.Services.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDto> GetWalletByUserIdAsync(string userId);
        Task<TransactionDto> RechargeWalletAsync(string userId, decimal amount, string paymentMethod);
        Task<TransactionDto> ConsumeWalletAsync(string userId, decimal amount, string sessionId);
        Task<bool> CheckBalanceAsync(string userId, decimal amount);
        Task<decimal> GetAvailableBalanceAsync(string userId);

        // 管理员功能
        Task<TransactionDto> AdminRechargeAsync(string userId, decimal amount, string adminId);
        Task<List<TransactionDto>> GetWalletTransactionsAsync(string userId, DateTime? startDate, DateTime? endDate);
    }
}