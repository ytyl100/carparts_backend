// ChargingStationManagement.Domain/Entities/Transaction.cs
using System;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.ValueObjects;

namespace ChargingStationManagement.Domain.Entities
{
    /// <summary>
    /// 交易实体（聚合根）
    /// </summary>
    public class Transaction : AggregateRoot
    {
        // 交易标识
        public string TransactionId { get; private set; }        // 交易唯一ID
        public Guid WalletId { get; private set; }               // 钱包ID
        public Guid? SessionId { get; private set; }             // 相关会话ID（如果是充电消费）

        // 交易信息
        public TransactionType Type { get; private set; }        // 交易类型
        public decimal Amount { get; private set; }              // 交易金额（正数为收入，负数为支出）
        public decimal BalanceBefore { get; private set; }       // 交易前余额
        public decimal BalanceAfter { get; private set; }        // 交易后余额

        // 支付信息
        public PaymentMethod PaymentMethod { get; private set; } // 支付方式
        public string PaymentProvider { get; private set; }      // 支付提供商
        public string PaymentReference { get; private set; }     // 支付参考号（第三方）
        public string PaymentStatus { get; private set; }        // 支付状态

        // 业务信息
        public string Description { get; private set; }          // 交易描述
        public string OperatorId { get; private set; }           // 操作员ID（如果是人工操作）
        public string ReferenceId { get; private set; }          // 业务参考ID

        // 时间信息
        public DateTime TransactionTime { get; private set; }    // 交易时间
        public DateTime? SettlementTime { get; private set; }    // 结算时间
        public DateTime? ReversalTime { get; private set; }      // 冲正时间

        // 状态信息
        public bool IsSettled { get; private set; }              // 是否已结算
        public bool IsReversed { get; private set; }             // 是否已冲正
        public string ReversalReason { get; private set; }       // 冲正原因

        // 手续费信息
        public decimal ServiceFee { get; private set; }          // 手续费
        public decimal Tax { get; private set; }                 // 税费

        // 扩展信息
        public string Metadata { get; private set; }             // 元数据（JSON格式）
        public string Notes { get; private set; }                // 备注

        // 构造函数
        protected Transaction() { }

        public Transaction(
            Guid walletId,
            TransactionType type,
            decimal amount,
            decimal balanceBefore,
            decimal balanceAfter,
            PaymentMethod paymentMethod = PaymentMethod.Wallet,
            string referenceId = null,
            string operatorId = null,
            string description = null)
        {
            if (walletId == Guid.Empty)
                throw new ArgumentException("Wallet ID cannot be empty", nameof(walletId));

            WalletId = walletId;
            Type = type;
            Amount = amount;
            BalanceBefore = balanceBefore;
            BalanceAfter = balanceAfter;
            PaymentMethod = paymentMethod;
            ReferenceId = referenceId;
            OperatorId = operatorId;
            Description = description ?? GetDefaultDescription(type, amount);

            TransactionId = GenerateTransactionId();
            TransactionTime = DateTime.UtcNow;
            PaymentStatus = "Completed";

            CreatedBy = operatorId ?? "system";
        }

        // 业务方法
        public void SetPaymentDetails(
            string paymentProvider,
            string paymentReference,
            string paymentStatus = "Completed")
        {
            PaymentProvider = paymentProvider;
            PaymentReference = paymentReference;
            PaymentStatus = paymentStatus;

            Update();
        }

        public void MarkAsSettled(DateTime settlementTime)
        {
            if (IsSettled)
                throw new InvalidOperationException("Transaction is already settled");

            IsSettled = true;
            SettlementTime = settlementTime;

            Update();
        }

        public void Reverse(string reason, string reversedBy)
        {
            if (IsReversed)
                throw new InvalidOperationException("Transaction is already reversed");

            IsReversed = true;
            ReversalTime = DateTime.UtcNow;
            ReversalReason = reason;

            Update(reversedBy);
        }

        public void SetSession(Guid sessionId)
        {
            if (SessionId.HasValue)
                throw new InvalidOperationException("Session is already set for this transaction");

            SessionId = sessionId;
            Update();
        }

        public void UpdateMetadata(string metadata)
        {
            Metadata = metadata;
            Update();
        }

        // 辅助方法
        private string GenerateTransactionId()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"TXN{timestamp}{random}";
        }

        private string GetDefaultDescription(TransactionType type, decimal amount)
        {
            return type switch
            {
                TransactionType.Recharge => $"充值 ¥{amount:F2}",
                TransactionType.Consumption => $"消费 ¥{-amount:F2}",
                TransactionType.Refund => $"退款 ¥{amount:F2}",
                TransactionType.Commission => $"佣金收入 ¥{amount:F2}",
                _ => $"交易 ¥{amount:F2}"
            };
        }

        // 查询方法
        public bool IsCredit()
        {
            return Amount > 0;
        }

        public bool IsDebit()
        {
            return Amount < 0;
        }

        public string GetFormattedAmount()
        {
            return IsCredit() ? $"+¥{Amount:F2}" : $"-¥{-Amount:F2}";
        }
    }
}