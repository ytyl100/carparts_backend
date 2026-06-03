namespace ChargingStationManagement.Domain.Entities;

public class Wallet
{
    public Guid? Id { get; private set; }
    public string WalletId { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public decimal Balance { get; set; }
    public decimal AvailableBalance { get; private set; }
    public decimal FrozenBalance { get; private set; }
    public decimal CreditLimit { get; private set; }
    public decimal CreditUsed { get; private set; }
    public decimal TotalRecharge { get; private set; }
    public decimal TotalConsumption { get; private set; }
    public decimal TotalRefund { get; private set; }
    public decimal TotalCommission { get; private set; }
    public decimal DailySpendingLimit { get; private set; }
    public decimal SingleSpendingLimit { get; private set; }
    public bool AutoRechargeEnabled { get; private set; }
    public decimal AutoRechargeThreshold { get; private set; }
    public decimal AutoRechargeAmount { get; private set; }
    public DateTime LastUpdateTime { get; set; }

    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive");
        Balance += amount;
        AvailableBalance += amount;
        TotalRecharge += amount;
        LastUpdateTime = DateTime.UtcNow;
    }

    // Optional: Consume, Freeze, etc. – you can add later as needed

    public Wallet() { }

    public Wallet(string walletId, Guid userId)
    {
        Id = Guid.NewGuid();
        WalletId = walletId;
        UserId = userId;
        Balance = 0;
        AvailableBalance = 0;
        FrozenBalance = 0;
        LastUpdateTime = DateTime.UtcNow;
    }

    public Wallet(Guid userId, decimal balance, DateTime updateAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Balance = balance;
        LastUpdateTime = updateAt;
    }
}