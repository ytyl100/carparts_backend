// ChargingStationManagement.Domain/Entities/Wallet.cs
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace ChargingStationManagement.Domain.Entities
{
    /// <summary>
    /// 钱包实体（聚合根）
    /// </summary>
    public class Wallet : AggregateRoot
    {
        // 钱包标识
        public string WalletId { get; private set; }              // 钱包唯一ID
        public Guid UserId { get; private set; }                  // 所属用户ID

        // 余额信息
        public decimal Balance { get; private set; }              // 当前余额
        public decimal AvailableBalance { get; private set; }     // 可用余额（扣除冻结金额）
        public decimal FrozenBalance { get; private set; }        // 冻结金额
        public decimal CreditLimit { get; private set; }          // 信用额度
        public decimal CreditUsed { get; private set; }           // 已用信用额度

        // 统计信息
        public decimal TotalRecharge { get; private set; }        // 累计充值金额
        public decimal TotalConsumption { get; private set; }     // 累计消费金额
        public decimal TotalRefund { get; private set; }          // 累计退款金额
        public decimal TotalCommission { get; private set; }      // 累计佣金收入
        public int TotalTransactions { get; private set; }        // 总交易次数

        // 安全设置
        public decimal DailySpendingLimit { get; private set; }   // 每日消费限额
        public decimal SingleSpendingLimit { get; private set; }  // 单笔消费限额
        public bool AutoRechargeEnabled { get; private set; }     // 是否启用自动充值
        public decimal AutoRechargeThreshold { get; private set; } // 自动充值阈值
        public decimal AutoRechargeAmount { get; private set; }   // 自动充值金额

        // 时间戳
        public DateTime LastRechargeTime { get; private set; }    // 最后充值时间
        public DateTime LastConsumptionTime { get; private set; } // 最后消费时间
        public DateTime LastUpdateTime { get; private set; }      // 最后更新时间

        // 导航属性
        private readonly List<Transaction> _transactions = new List<Transaction>();
        private readonly List<DailySpending> _dailySpending = new List<DailySpending>();

        public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();
        public IReadOnlyCollection<DailySpending> DailySpendingRecords => _dailySpending.AsReadOnly();

        // 构造函数
        protected Wallet() { }

        public Wallet(Guid userId, string walletId = null, string createdBy = "system")
        {
            UserId = userId;
            WalletId = walletId ?? $"WALLET-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

            Balance = 0;
            AvailableBalance = 0;
            FrozenBalance = 0;
            CreditLimit = 0;
            CreditUsed = 0;

            // 默认安全设置
            DailySpendingLimit = 5000;     // 每日限额5000元
            SingleSpendingLimit = 2000;    // 单笔限额2000元
            AutoRechargeEnabled = false;
            AutoRechargeThreshold = 50;    // 余额低于50元时自动充值
            AutoRechargeAmount = 200;      // 自动充值200元

            LastUpdateTime = DateTime.UtcNow;
            CreatedBy = createdBy;

            AddDomainEvent(new WalletCreatedEvent(Id, userId, WalletId));
        }

        // 业务方法
        public Transaction Recharge(
            decimal amount,
            PaymentMethod method,
            string referenceId,
            string operatorId = null,
            string notes = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Recharge amount must be greater than 0", nameof(amount));

            if (amount > 10000)
                throw new ArgumentException("Single recharge amount cannot exceed 10000", nameof(amount));

            var beforeBalance = Balance;
            Balance += amount;
            AvailableBalance += amount;
            TotalRecharge += amount;
            TotalTransactions += 1;
            LastRechargeTime = DateTime.UtcNow;
            LastUpdateTime = DateTime.UtcNow;

            var transaction = new Transaction(
                Id,
                TransactionType.Recharge,
                amount,
                beforeBalance,
                Balance,
                method,
                referenceId,
                operatorId,
                notes);

            _transactions.Add(transaction);
            Update();

            AddDomainEvent(new WalletRechargedEvent(Id, WalletId, amount, method, Balance));

            return transaction;
        }

        public Transaction Consume(
            decimal amount,
            string sessionId,
            string description,
            PaymentMethod method = PaymentMethod.Wallet)
        {
            if (amount <= 0)
                throw new ArgumentException("Consumption amount must be greater than 0", nameof(amount));

            if (amount > SingleSpendingLimit)
                throw new InvalidOperationException($"Single consumption amount {amount} exceeds limit {SingleSpendingLimit}");

            // 检查每日限额
            var todaySpending = GetTodaySpending();
            if (todaySpending + amount > DailySpendingLimit)
                throw new InvalidOperationException($"Daily spending limit {DailySpendingLimit} exceeded");

            // 检查余额是否充足
            if (AvailableBalance < amount)
            {
                // 尝试使用信用额度
                var creditAvailable = CreditLimit - CreditUsed;
                if (creditAvailable < amount - AvailableBalance)
                    throw new InvalidOperationException("Insufficient balance and credit");

                // 使用部分信用额度
                var creditUsed = amount - AvailableBalance;
                CreditUsed += creditUsed;
                AvailableBalance = 0;
                Balance -= AvailableBalance;
            }
            else
            {
                AvailableBalance -= amount;
                Balance -= amount;
            }

            TotalConsumption += amount;
            TotalTransactions += 1;
            LastConsumptionTime = DateTime.UtcNow;
            LastUpdateTime = DateTime.UtcNow;

            var transaction = new Transaction(
                Id,
                TransactionType.Consumption,
                -amount,
                Balance + amount,
                Balance,
                method,
                sessionId,
                description: description);

            _transactions.Add(transaction);
            UpdateDailySpending(amount);
            Update();

            AddDomainEvent(new WalletConsumedEvent(Id, WalletId, amount, sessionId, Balance));

            // 检查是否需要自动充值
            CheckAutoRecharge();

            return transaction;
        }

        public Transaction Refund(
            decimal amount,
            string originalTransactionId,
            string reason,
            string operatorId = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Refund amount must be greater than 0", nameof(amount));

            var beforeBalance = Balance;
            Balance += amount;
            AvailableBalance += amount;
            TotalRefund += amount;
            TotalTransactions += 1;
            LastUpdateTime = DateTime.UtcNow;

            var transaction = new Transaction(
                Id,
                TransactionType.Refund,
                amount,
                beforeBalance,
                Balance,
                PaymentMethod.Wallet,
                originalTransactionId,
                operatorId,
                $"Refund: {reason}");

            _transactions.Add(transaction);
            Update();

            AddDomainEvent(new WalletRefundedEvent(Id, WalletId, amount, reason, Balance));

            return transaction;
        }

        public Transaction AddCommission(
            decimal amount,
            string sessionId,
            string description)
        {
            if (amount <= 0)
                throw new ArgumentException("Commission amount must be greater than 0", nameof(amount));

            var beforeBalance = Balance;
            Balance += amount;
            AvailableBalance += amount;
            TotalCommission += amount;
            TotalTransactions += 1;
            LastUpdateTime = DateTime.UtcNow;

            var transaction = new Transaction(
                Id,
                TransactionType.Commission,
                amount,
                beforeBalance,
                Balance,
                PaymentMethod.Wallet,
                sessionId,
                description: description);

            _transactions.Add(transaction);
            Update();

            AddDomainEvent(new CommissionAddedEvent(Id, WalletId, amount, sessionId, Balance));

            return transaction;
        }

        public void FreezeBalance(decimal amount, string reason)
        {
            if (amount <= 0)
                throw new ArgumentException("Freeze amount must be greater than 0", nameof(amount));

            if (AvailableBalance < amount)
                throw new InvalidOperationException("Insufficient available balance to freeze");

            AvailableBalance -= amount;
            FrozenBalance += amount;
            LastUpdateTime = DateTime.UtcNow;

            Update();

            AddDomainEvent(new BalanceFrozenEvent(Id, WalletId, amount, reason));
        }

        public void UnfreezeBalance(decimal amount, string reason)
        {
            if (amount <= 0)
                throw new ArgumentException("Unfreeze amount must be greater than 0", nameof(amount));

            if (FrozenBalance < amount)
                throw new InvalidOperationException("Insufficient frozen balance to unfreeze");

            FrozenBalance -= amount;
            AvailableBalance += amount;
            LastUpdateTime = DateTime.UtcNow;

            Update();

            AddDomainEvent(new BalanceUnfrozenEvent(Id, WalletId, amount, reason));
        }

        public void SetSpendingLimits(decimal dailyLimit, decimal singleLimit)
        {
            if (dailyLimit <= 0 || singleLimit <= 0)
                throw new ArgumentException("Spending limits must be greater than 0");

            if (singleLimit > dailyLimit)
                throw new ArgumentException("Single spending limit cannot exceed daily limit");

            DailySpendingLimit = dailyLimit;
            SingleSpendingLimit = singleLimit;

            Update();

            AddDomainEvent(new SpendingLimitsUpdatedEvent(Id, WalletId, dailyLimit, singleLimit));
        }

        public void SetCreditLimit(decimal limit)
        {
            if (limit < 0)
                throw new ArgumentException("Credit limit cannot be negative");

            CreditLimit = limit;

            Update();

            AddDomainEvent(new CreditLimitUpdatedEvent(Id, WalletId, limit));
        }

        public void EnableAutoRecharge(decimal threshold, decimal amount)
        {
            if (threshold <= 0 || amount <= 0)
                throw new ArgumentException("Threshold and amount must be greater than 0");

            AutoRechargeEnabled = true;
            AutoRechargeThreshold = threshold;
            AutoRechargeAmount = amount;

            Update();

            AddDomainEvent(new AutoRechargeEnabledEvent(Id, WalletId, threshold, amount));
        }

        public void DisableAutoRecharge()
        {
            AutoRechargeEnabled = false;

            Update();

            AddDomainEvent(new AutoRechargeDisabledEvent(Id, WalletId));
        }

        private void UpdateDailySpending(decimal amount)
        {
            var today = DateTime.UtcNow.Date;
            var dailySpending = _dailySpending.FirstOrDefault(d => d.Date == today);

            if (dailySpending == null)
            {
                dailySpending = new DailySpending(Id, today, amount);
                _dailySpending.Add(dailySpending);
            }
            else
            {
                dailySpending.AddSpending(amount);
            }
        }

        private decimal GetTodaySpending()
        {
            var today = DateTime.UtcNow.Date;
            var dailySpending = _dailySpending.FirstOrDefault(d => d.Date == today);
            return dailySpending?.Amount ?? 0;
        }

        private void CheckAutoRecharge()
        {
            if (AutoRechargeEnabled && Balance < AutoRechargeThreshold)
            {
                // 触发自动充值事件，由应用服务处理具体充值逻辑
                AddDomainEvent(new AutoRechargeTriggeredEvent(Id, WalletId, Balance, AutoRechargeAmount));
            }
        }

        // 验证方法
        public bool HasSufficientBalance(decimal amount)
        {
            return AvailableBalance + (CreditLimit - CreditUsed) >= amount;
        }

        public bool CanConsume(decimal amount)
        {
            if (amount <= 0) return false;
            if (amount > SingleSpendingLimit) return false;
            if (GetTodaySpending() + amount > DailySpendingLimit) return false;

            return HasSufficientBalance(amount);
        }
    }

    /// <summary>
    /// 每日消费记录
    /// </summary>
    public class DailySpending : BaseEntity
    {
        public Guid WalletId { get; private set; }
        public DateTime Date { get; private set; }
        public decimal Amount { get; private set; }

        public DailySpending(Guid walletId, DateTime date, decimal initialAmount = 0)
        {
            WalletId = walletId;
            Date = date.Date;
            Amount = initialAmount;
        }

        public void AddSpending(decimal amount)
        {
            Amount += amount;
            Update();
        }
    }
}