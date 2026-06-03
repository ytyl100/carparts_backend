// ChargingStationManagement.Domain/ValueObjects/PowerConfiguration.cs
namespace ChargingStationManagement.Domain.ValueObjects
{
    /// <summary>
    /// 功率配置值对象
    /// </summary>
    public class PowerConfiguration : ValueObject
    {
        public decimal MinPower { get; }          // 最小功率（kW）
        public decimal RatedPower { get; }        // 额定功率（kW）
        public decimal MaxPower { get; }          // 最大功率（kW）

        public PowerConfiguration(decimal minPower, decimal ratedPower, decimal maxPower)
        {
            if (minPower <= 0)
                throw new ArgumentException("Minimum power must be greater than 0", nameof(minPower));

            if (ratedPower < minPower)
                throw new ArgumentException("Rated power must be greater than or equal to minimum power", nameof(ratedPower));

            if (maxPower < ratedPower)
                throw new ArgumentException("Maximum power must be greater than or equal to rated power", nameof(maxPower));

            MinPower = minPower;
            RatedPower = ratedPower;
            MaxPower = maxPower;
        }

        public bool IsPowerValid(decimal power)
        {
            return power >= MinPower && power <= MaxPower;
        }

        public decimal GetPowerPercentage(decimal power)
        {
            if (!IsPowerValid(power))
                return 0;

            return (power - MinPower) / (MaxPower - MinPower) * 100;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return MinPower;
            yield return RatedPower;
            yield return MaxPower;
        }

        public override string ToString()
        {
            return $"{MinPower:F1}-{RatedPower:F1}-{MaxPower:F1} kW";
        }
    }
}