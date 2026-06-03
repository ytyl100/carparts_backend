namespace ChargingStationManagement.Domain.Entities;

public class Transaction
{
    public Guid? Id { get; private set; }
    public string TransactionId { get; private set; } = null!;
    public Guid? WalletId { get; private set; }
    public string? UserId { get; private set; }
    public string? SessionId { get; private set; }
    public int Type { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceBefore { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public int PaymentMethod { get; private set; }
    public string? Description { get; private set; }
    public DateTime TransactionTime { get; private set; }
    public DateTime CreatedAt { get; set; }

    public Transaction() { }

    public Transaction(string transactionId, Guid? walletId, decimal amount)
    {
        Id = Guid.NewGuid();
        TransactionId = transactionId;
        WalletId = walletId;
        Amount = amount;
        TransactionTime = DateTime.UtcNow;
    }
    public Transaction(string transactionId, Guid walletId, decimal amount, decimal before, decimal after, int type)
    {
        Id = Guid.NewGuid();
        TransactionId = transactionId;
        WalletId = walletId;
        Amount = amount;
        BalanceBefore = before;
        BalanceAfter = after;
        Type = type;
        TransactionTime = DateTime.UtcNow;
    }
}