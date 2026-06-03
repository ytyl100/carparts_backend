// ChargingStationManagement.Domain/Entities/Vehicle.cs
using System;

namespace ChargingStationManagement.Domain.Entities
{
    /// <summary>
    /// 车辆实体
    /// </summary>
    public class Vehicle : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string LicensePlate { get; private set; }
        public string VIN { get; private set; }                  // 车辆识别码
        public string Brand { get; private set; }                // 品牌
        public string Model { get; private set; }                // 型号
        public int Year { get; private set; }                    // 年份
        public string Color { get; private set; }                // 颜色

        // 电池信息
        public decimal BatteryCapacity { get; private set; }     // 电池容量（kWh）
        public BatteryType BatteryType { get; private set; }     // 电池类型
        public decimal MaxChargingPower { get; private set; }    // 最大充电功率（kW）

        // 充电偏好
        public decimal PreferredChargingLimit { get; private set; } // 偏好充电限制（%）
        public bool AutoStopAtLimit { get; private set; }        // 达到限制自动停止

        // 状态信息
        public bool IsDefault { get; private set; }              // 是否默认车辆
        public bool IsActive { get; private set; }               // 是否激活

        protected Vehicle() { }

        public Vehicle(
            Guid userId,
            string licensePlate,
            string brand,
            string model,
            decimal batteryCapacity,
            BatteryType batteryType = BatteryType.LithiumIon)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(licensePlate))
                throw new ArgumentException("License plate cannot be empty", nameof(licensePlate));

            if (batteryCapacity <= 0)
                throw new ArgumentException("Battery capacity must be greater than 0", nameof(batteryCapacity));

            UserId = userId;
            LicensePlate = licensePlate;
            Brand = brand ?? "Unknown";
            Model = model ?? "Unknown";
            BatteryCapacity = batteryCapacity;
            BatteryType = batteryType;

            // 根据电池容量设置最大充电功率
            MaxChargingPower = CalculateMaxChargingPower(batteryCapacity);

            // 默认设置
            PreferredChargingLimit = 80; // 默认充到80%
            AutoStopAtLimit = true;
            IsActive = true;

            CreatedBy = "system";
        }

        private decimal CalculateMaxChargingPower(decimal capacity)
        {
            // 简单规则：电池容量的1C充电率，但不超过常见最大值
            var maxPower = capacity; // 1C充电率

            // 限制最大值
            if (maxPower > 150) return 150;    // 最大150kW
            if (maxPower > 120) return 120;    // 常见快充桩最大值
            if (maxPower > 22) return 22;      // 交流慢充最大值
            if (maxPower > 7) return 7;        // 二轮车充电桩最大值

            return maxPower;
        }

        public void UpdateInfo(
            string brand,
            string model,
            int year,
            string color,
            string vin = null)
        {
            Brand = brand ?? Brand;
            Model = model ?? Model;
            Year = year > 0 ? year : Year;
            Color = color ?? Color;
            VIN = vin ?? VIN;

            Update();
        }

        public void SetBatteryInfo(decimal capacity, BatteryType type, decimal maxPower)
        {
            if (capacity <= 0)
                throw new ArgumentException("Battery capacity must be greater than 0", nameof(capacity));

            if (maxPower <= 0)
                throw new ArgumentException("Max charging power must be greater than 0", nameof(maxPower));

            BatteryCapacity = capacity;
            BatteryType = type;
            MaxChargingPower = maxPower;

            Update();
        }

        public void SetChargingPreferences(decimal limit, bool autoStop)
        {
            if (limit < 20 || limit > 100)
                throw new ArgumentException("Charging limit must be between 20 and 100 percent", nameof(limit));

            PreferredChargingLimit = limit;
            AutoStopAtLimit = autoStop;

            Update();
        }

        public void SetAsDefault()
        {
            IsDefault = true;
            Update();
        }

        public void Activate()
        {
            IsActive = true;
            Update();
        }

        public void Deactivate()
        {
            IsActive = false;
            Update();
        }
    }

    public enum BatteryType
    {
        LithiumIon = 1,      // 锂离子电池
        LithiumPolymer = 2,  // 锂聚合物电池
        LeadAcid = 3,        // 铅酸电池
        NickelMetalHydride = 4, // 镍氢电池
        SolidState = 5       // 固态电池
    }
}