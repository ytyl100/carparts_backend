using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Infrastructure.Persistence;
using ChargingStationManagement.Services.DTOs;
using ChargingStationManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChargingStationManagement.Services.ApplicationServices;

public class WalletService : IWalletService
{
    private readonly AppDbContext _context;
    private readonly ILogger<WalletService> _logger;

    public WalletService(AppDbContext context, ILogger<WalletService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // 将 string userId 转换为 Guid 类型再进行比较
    public async Task<WalletDto> GetWalletByUserIdAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid userId format");
        }

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userGuid);

        if (wallet == null)
        {
            throw new KeyNotFoundException($"Wallet not found for user {userId}");
        }

        return MapToDto(wallet);
    }

    public async Task<TransactionDto> RechargeWalletAsync(string userId, decimal amount, string paymentMethod)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero");
        }

        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid userId format");
        }

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userGuid);

        if (wallet == null)
        {
            wallet = new Wallet(userGuid, 0, DateTime.UtcNow);
            //{
            //    UserId = userGuid,
            //    Balance = 0,
            //    CreatedAt = DateTime.UtcNow
            //};
            _context.Wallets.Add(wallet);
        }

        wallet.Balance += amount;
        wallet.LastUpdateTime = DateTime.UtcNow;

        var transaction = new Transaction(userId, wallet.Id, amount);
        //{
        //    WalletId = wallet.Id,
        //    Amount = amount,
        //    Type = "Recharge",
        //    PaymentMethod = paymentMethod,
        //    Status = "Completed",
        //    CreatedAt = DateTime.UtcNow
        //};

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Wallet recharged for user {UserId}, amount: {Amount}", userId, amount);

        return MapTransactionToDto(transaction);
    }

    public async Task<TransactionDto> ConsumeWalletAsync(string userId, decimal amount, string sessionId)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero");
        }

        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid userId format");
        }

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userGuid);

        if (wallet == null)
        {
            throw new KeyNotFoundException($"Wallet not found for user {userId}");
        }

        if (wallet.Balance < amount)
        {
            throw new InvalidOperationException("Insufficient balance");
        }

        wallet.Balance -= amount;
        wallet.LastUpdateTime = DateTime.UtcNow;

        var transaction = new Transaction(sessionId, wallet.Id, -amount);
        //{
        //    WalletId = wallet.Id,
        //    Amount = -amount,
        //    Type = "Consumption",
        //    SessionId = sessionId,
        //    Status = "Completed",
        //    CreatedAt = DateTime.UtcNow
        //};

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Wallet consumed for user {UserId}, amount: {Amount}, session: {SessionId}", 
            userId, amount, sessionId);

        return MapTransactionToDto(transaction);
    }

    public async Task<bool> CheckBalanceAsync(string userId, decimal amount)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid userId format");
        }

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userGuid);

        return wallet != null && wallet.Balance >= amount;
    }

    public async Task<decimal> GetAvailableBalanceAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid userId format");
        }

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userGuid);

        return wallet?.Balance ?? 0;
    }

    public async Task<TransactionDto> AdminRechargeAsync(string userId, decimal amount, string adminId)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero");
        }

        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid userId format");
        }

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userGuid);

        if (wallet == null)
        {
            wallet = new Wallet(userGuid, 0, DateTime.UtcNow);
            _context.Wallets.Add(wallet);
        }

        wallet.Balance += amount;
        wallet.LastUpdateTime = DateTime.UtcNow;

        var transaction = new Transaction(adminId, wallet.Id, amount);

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin {AdminId} recharged wallet for user {UserId}, amount: {Amount}", 
            adminId, userId, amount);

        return MapTransactionToDto(transaction);
    }

    public async Task<List<TransactionDto>> GetWalletTransactionsAsync(string userId, DateTime? startDate, DateTime? endDate)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId.ToString() == userId);

        if (wallet == null)
        {
            throw new KeyNotFoundException($"Wallet not found for user {userId}");
        }

        var query = _context.Transactions
            .Where(t => t.WalletId == wallet.Id);

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return transactions.Select(MapTransactionToDto).ToList();
    }

    private WalletDto MapToDto(Wallet wallet)
    {
        return new WalletDto
        {
            WalletId = wallet.Id.ToString(),
            UserId = wallet.UserId.ToString(),
            Balance = wallet.Balance,
            LastUpdateTime = wallet.LastUpdateTime
        };
    }

    private TransactionDto MapTransactionToDto(Transaction transaction)
    {
        return new TransactionDto
        {
            TransactionId = transaction.Id.ToString(),
            WalletId = transaction.WalletId.ToString(),
            Amount = transaction.Amount,
            Type = transaction.Type,
            PaymentMethod = transaction.PaymentMethod,
            SessionId = transaction.SessionId,
            Description = transaction.Description,
            TransactionTime = transaction.CreatedAt
        };
    }
}
