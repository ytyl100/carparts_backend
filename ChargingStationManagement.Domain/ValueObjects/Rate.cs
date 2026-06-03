// ChargingStationManagement.Domain/ValueObjects/Rate.cs
namespace ChargingStationManagement.Domain.ValueObjects
{
    /// <summary>
    /// 费率值对象
    /// </summary>
    public class Rate : ValueObject
    {
        public decimal ElectricityRate { get; }       // 电费率（元/kWh）
        public decimal ServiceRate { get; }           // 服务费率（元/kWh）
        public decimal ParkRate { get; }              // 停车费率（元/小时）
        public decimal TimeRate { get; }              // 时间费率（元/分钟，用于时段计费）

        public Rate(
            decimal electricityRate = 0,
            decimal serviceRate = 0,
            decimal parkRate = 0,
            decimal timeRate = 0)
        {
            if (electricityRate < 0 || serviceRate < 0 || parkRate < 0 || timeRate < 0)
                throw new ArgumentException("All rates must be non-negative");

            ElectricityRate = electricityRate;
            ServiceRate = serviceRate;
            ParkRate = parkRate;
            TimeRate = timeRate;
        }

        public decimal CalculateElectricityCost(decimal energyKwh)
        {
            return energyKwh * ElectricityRate;
        }

        public decimal CalculateServiceCost(decimal energyKwh)
        {
            return energyKwh * ServiceRate;
        }

        public decimal CalculateParkingCost(decimal hours)
        {
            return hours * ParkRate;
        }

        public decimal CalculateTimeCost(decimal minutes)
        {
            return minutes * TimeRate;
        }

        public decimal CalculateTotalCost(decimal energyKwh, decimal hours = 0, decimal minutes = 0)
        {
            return CalculateElectricityCost(energyKwh) +
                   CalculateServiceCost(energyKwh) +
                   CalculateParkingCost(hours) +
                   CalculateTimeCost(minutes);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return ElectricityRate;
            yield return ServiceRate;
            yield return ParkRate;
            yield return TimeRate;
        }

        public override string ToString()
        {
            return $"电费:{ElectricityRate:F2}元/kWh, 服务费:{ServiceRate:F2}元/kWh, 停车费:{ParkRate:F2}元/小时";
        }
    }
}