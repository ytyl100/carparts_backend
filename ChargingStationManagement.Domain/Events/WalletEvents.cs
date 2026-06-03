// ChargingStationManagement.Domain/Events/WalletEvents.cs
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Interfaces;
using System;

namespace ChargingStationManagement.Domain.Events
{
    // 钱包相关事件
    public class WalletCreatedEvent : DomainEvent
    {
        public Guid WalletId { get; }
        public Guid UserId { get; }
        public string ExternalWalletId { get; }

        public WalletCreatedEvent(Guid walletId, Guid userId, string externalWalletId)
        {
            WalletId = walletId;
            UserId = userId;
            ExternalWalletId = externalWalletId;
        }
    }

    public class WalletRechargedEvent : DomainEvent
    {
        private Guid id;
        private string walletId;
        private PaymentMethod method;
        private decimal balance;

        public Guid WalletId { get; }
        public string ExternalWalletId { get; }
        public decimal Amount { get; }
        public string PaymentMethod { get; }
        public decimal NewBalance { get; }

        public WalletRechargedEvent(Guid walletId, string externalWalletId,
            decimal amount, string paymentMethod, decimal newBalance)
        {
            WalletId = walletId;
            ExternalWalletId = externalWalletId;
            Amount = amount;
            PaymentMethod = paymentMethod;
            NewBalance = newBalance;
        }

        public WalletRechargedEvent(Guid id, string walletId, decimal amount, PaymentMethod method, decimal balance)
        {
            this.id = id;
            this.walletId = walletId;
            Amount = amount;
            this.method = method;
            this.balance = balance;
        }
    }

    public class WalletConsumedEvent : DomainEvent
    {
        public Guid WalletId { get; }
        public string ExternalWalletId { get; }
        public decimal Amount { get; }
        public string SessionId { get; }
        public decimal NewBalance { get; }

        public WalletConsumedEvent(Guid walletId, string externalWalletId,
            decimal amount, string sessionId, decimal newBalance)
        {
            WalletId = walletId;
            ExternalWalletId = externalWalletId;
            Amount = amount;
            SessionId = sessionId;
            NewBalance = newBalance;
        }
    }

    public class WalletRefundedEvent : DomainEvent
    {
        public Guid WalletId { get; }
        public string ExternalWalletId { get; }
        public decimal Amount { get; }
        public string Reason { get; }
        public decimal NewBalance { get; }

        public WalletRefundedEvent(Guid walletId, string externalWalletId,
            decimal amount, string reason, decimal newBalance)
        {
            WalletId = walletId;
            ExternalWalletId = externalWalletId;
            Amount = amount;
            Reason = reason;
            NewBalance = newBalance;
        }
    }

    public class CommissionAddedEvent : DomainEvent
    {
        public Guid WalletId { get; }
        public string ExternalWalletId { get; }
        public decimal Amount { get; }
        public string SessionId { get; }
        public decimal NewBalance { get; }

        public CommissionAddedEvent(Guid walletId, string externalWalletId,
            decimal amount, string sessionId, decimal newBalance)
        {
            WalletId = walletId;
            ExternalWalletId = externalWalletId;
            Amount = amount;
            SessionId = sessionId;
            NewBalance = newBalance;
        }
    }

    public class BalanceFrozenEvent : DomainEvent
    {
        public Guid WalletId { get; }
        public string ExternalWalletId { get; }
        public decimal Amount { get; }
        public string Reason { get; }

        public BalanceFrozenEvent(Guid walletId, string externalWalletId, decimal amount, string reason)
        {
            WalletId = walletId;
            ExternalWalletId = externalWalletId;
            Amount = amount;
            Reason = reason;
        }
    }

    public class BalanceUnfrozenEvent : DomainEvent
    {
        public Guid WalletId { get; }
        public string ExternalWalletId { get; }
        public decimal Amount { get; }
        public string Reason { get; }

        public BalanceUnfrozenEvent(Guid walletId, string externalWalletId, decimal amount, string reason)
        {
            WalletId = walletId;
            ExternalWalletId = externalWalletId;
            Amount = amount;
            Reason = reason;
        }
    }

    public class SpendingLimitsUpdatedEvent : DomainEvent
    {
        public Guid WalletId { get; }
        public string ExternalWalletId { get; }
        public decimal DailyLimit { get; }
        public decimal SingleLimit { get; }

        public SpendingLimitsUpdatedEvent(Guid walletId, string externalWalletId,
            decimal dailyLimit, decimal singleLimit)
        {
            WalletId = walletId;
            ExternalWalletId = externalWalletId;
            DailyLimit = dailyLimit;
            SingleLimit = singleLimit;
        }
    }

    public class CreditLimitUpdatedEvent : DomainEvent
    {
        public Guid WalletId { get; }
        public string ExternalWalletId { get; }
        public decimal NewLimit { get; }

        public CreditLimitUpdatedEvent(Guid walletId, string externalWalletId, decimal newLimit)
        {
            WalletId = walletId;
            ExternalWalletId = externalWalletId;
            NewLimit = newLimit;
        }
    }

    public class AutoRechargeEnabledEvent : DomainEvent
    {
        public Guid WalletId { get; }
        public string ExternalWalletId { get; }
        public decimal Threshold { get; }
        public decimal Amount { get; }

        public AutoRechargeEnabledEvent(Guid walletId, string externalWalletId, decimal threshold, decimal amount)
        {
            WalletId = walletId;
            ExternalWalletId = externalWalletId;
            Threshold = threshold;
            Amount = amount;
        }
    }

    public class AutoRechargeDisabledEvent : DomainEvent
    {
        public Guid WalletId { get; }
        public string ExternalWalletId { get; }

        public AutoRechargeDisabledEvent(Guid walletId, string externalWalletId)
        {
            WalletId = walletId;
            ExternalWalletId = externalWalletId;
        }
    }

    public class AutoRechargeTriggeredEvent : DomainEvent
    {
        public Guid WalletId { get; }
        public string ExternalWalletId { get; }
        public decimal CurrentBalance { get; }
        public decimal RechargeAmount { get; }

        public AutoRechargeTriggeredEvent(Guid walletId, string externalWalletId,
            decimal currentBalance, decimal rechargeAmount)
        {
            WalletId = walletId;
            ExternalWalletId = externalWalletId;
            CurrentBalance = currentBalance;
            RechargeAmount = rechargeAmount;
        }
    }
}