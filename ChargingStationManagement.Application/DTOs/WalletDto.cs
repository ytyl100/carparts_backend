// ChargingStationManagement.Services/DTOs/WalletDto.cs
using System;
using System.Collections.Generic;

namespace ChargingStationManagement.Services.DTOs
{
    public class WalletDto
    {
        public string WalletId { get; set; }
        public string UserId { get; set; }
        public decimal Balance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal FrozenBalance { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CreditUsed { get; set; }
        public decimal TotalRecharge { get; set; }
        public decimal TotalConsumption { get; set; }
        public decimal TotalRefund { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal DailySpendingLimit { get; set; }
        public decimal SingleSpendingLimit { get; set; }
        public bool AutoRechargeEnabled { get; set; }
        public decimal AutoRechargeThreshold { get; set; }
        public decimal AutoRechargeAmount { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }

    public class TransactionDto
    {
        public string TransactionId { get; set; }
        public string WalletId { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public int Type { get; set; }
        public string TypeText { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public int PaymentMethod { get; set; }
        public string PaymentMethodText { get; set; }
        public string Description { get; set; }
        public DateTime TransactionTime { get; set; }
        public bool IsSettled { get; set; }
        public bool IsReversed { get; set; }
        public string ReferenceId { get; set; }
    }

    public class RechargeRequestDto
    {
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public int PaymentMethod { get; set; }
        public string PaymentReference { get; set; }
        public string OperatorId { get; set; }
    }

    public class ConsumeRequestDto
    {
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string SessionId { get; set; }
        public string Description { get; set; }
    }
}
