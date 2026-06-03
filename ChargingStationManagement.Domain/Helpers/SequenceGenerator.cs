// ChargingStationManagement.Domain/Helpers/SequenceGenerator.cs
using System;

namespace ChargingStationManagement.Domain.Helpers
{
    /// <summary>
    /// 序列号生成器
    /// </summary>
    public static class SequenceGenerator
    {
        /// <summary>
        /// 生成充电流水号
        /// 格式：运营商ID(9) + 时间戳(14) + 随机数(4)
        /// </summary>
        public static string GenerateStartChargeSeq(string operatorId)
        {
            if (string.IsNullOrWhiteSpace(operatorId))
                throw new ArgumentException("Operator ID cannot be empty", nameof(operatorId));

            if (operatorId.Length != 9)
                throw new ArgumentException("Operator ID must be 9 characters", nameof(operatorId));

            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999).ToString();

            return $"{operatorId}{timestamp}{random}";
        }

        /// <summary>
        /// 生成订单号
        /// 格式：ORD + 时间戳(14) + 随机数(5)
        /// </summary>
        public static string GenerateOrderId()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(10000, 99999).ToString();

            return $"ORD{timestamp}{random}";
        }

        /// <summary>
        /// 生成交易流水号
        /// 格式：TXN + 时间戳(14) + 随机数(5)
        /// </summary>
        public static string GenerateTransactionId()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(10000, 99999).ToString();

            return $"TXN{timestamp}{random}";
        }

        /// <summary>
        /// 生成用户ID
        /// 格式：USR + 时间戳(10) + 随机数(6)
        /// </summary>
        public static string GenerateUserId()
        {
            var timestamp = DateTime.Now.ToString("yyMMddHHmm");
            var random = new Random().Next(100000, 999999).ToString();

            return $"USR{timestamp}{random}";
        }
    }
}