// ChargingStationManagement.Infrastructure/Persistence/Repositories/WalletRepository.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingStationManagement.Infrastructure.Persistence.Repositories
{
    public class WalletRepository : Repository<Wallet>, IRepository<Wallet>
    {
        public WalletRepository(ChargingStationDbContext context) : base(context)
        {
        }

        public override async Task<Wallet> GetByIdAsync(string externalId)
        {
            return await _dbSet
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.WalletId == externalId);
        }

        public async Task<Wallet> GetWalletByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(w => w.Transactions)
                .Include(w => w.DailySpendingRecords)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<Wallet> GetWalletByUserIdAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return null;

            return await GetWalletByUserIdAsync(user.Id);
        }

        public async Task<IReadOnlyList<Transaction>> GetWalletTransactionsAsync(Guid walletId, int page = 1, int pageSize = 20)
        {
            return await _context.Transactions
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.TransactionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<decimal> GetWalletBalanceAsync(Guid walletId)
        {
            var wallet = await GetByIdAsync(walletId);
            return wallet?.Balance ?? 0;
        }

        public async Task<bool> HasSufficientBalanceAsync(Guid walletId, decimal amount)
        {
            var wallet = await GetByIdAsync(walletId);
            return wallet?.CanConsume(amount) ?? false;
        }

        public async Task<Transaction> AddTransactionAsync(Guid walletId, Transaction transaction)
        {
            var wallet = await GetByIdAsync(walletId);
            if (wallet == null)
                return null;

            await _context.Transactions.AddAsync(transaction);
            return transaction;
        }

        public async Task<IReadOnlyList<Transaction>> GetTransactionsByDateRangeAsync(
            Guid walletId,
            DateTime startDate,
            DateTime endDate,
            TransactionType? type = null)
        {
            var query = _context.Transactions
                .Where(t => t.WalletId == walletId &&
                           t.TransactionTime >= startDate &&
                           t.TransactionTime <= endDate);

            if (type.HasValue)
            {
                query = query.Where(t => t.Type == type.Value);
            }

            return await query
                .OrderByDescending(t => t.TransactionTime)
                .ToListAsync();
        }

        public async Task<decimal> GetTodaySpendingAsync(Guid walletId)
        {
            var today = DateTime.UtcNow.Date;
            var dailySpending = await _context.DailySpendings
                .FirstOrDefaultAsync(d => d.WalletId == walletId && d.Date == today);

            return dailySpending?.Amount ?? 0;
        }
    }
}