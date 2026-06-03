// ChargingStationManagement.Domain/ValueObjects/TimeSlot.cs
namespace ChargingStationManagement.Domain.ValueObjects
{
    /// <summary>
    /// 时间段值对象（用于预约和时段计费）
    /// </summary>
    public class TimeSlot : ValueObject
    {
        public TimeSpan StartTime { get; }        // 开始时间
        public TimeSpan EndTime { get; }          // 结束时间
        public decimal RateMultiplier { get; }    // 费率倍率（1.0 = 正常费率）
        public bool IsPeak { get; }               // 是否高峰期

        public TimeSlot(TimeSpan startTime, TimeSpan endTime, decimal rateMultiplier = 1.0m, bool isPeak = false)
        {
            if (startTime >= endTime)
                throw new ArgumentException("Start time must be before end time");

            if (rateMultiplier <= 0)
                throw new ArgumentException("Rate multiplier must be greater than 0");

            StartTime = startTime;
            EndTime = endTime;
            RateMultiplier = rateMultiplier;
            IsPeak = isPeak;
        }

        public bool Contains(TimeSpan time)
        {
            if (StartTime < EndTime)
            {
                // 同一天内的时间段
                return time >= StartTime && time <= EndTime;
            }
            else
            {
                // 跨天的时间段（如 22:00-06:00）
                return time >= StartTime || time <= EndTime;
            }
        }

        public bool Overlaps(TimeSlot other)
        {
            if (other == null) return false;

            if (StartTime < EndTime && other.StartTime < other.EndTime)
            {
                // 两个都是同一天的时间段
                return StartTime < other.EndTime && other.StartTime < EndTime;
            }
            else if (StartTime >= EndTime && other.StartTime >= other.EndTime)
            {
                // 两个都是跨天的时间段，总是重叠
                return true;
            }
            else
            {
                // 一个跨天，一个不跨天
                if (StartTime >= EndTime)
                {
                    // this是跨天的
                    return other.StartTime < EndTime || other.EndTime > StartTime;
                }
                else
                {
                    // other是跨天的
                    return StartTime < other.EndTime || EndTime > other.StartTime;
                }
            }
        }

        public TimeSpan Duration => StartTime <= EndTime
            ? EndTime - StartTime
            : TimeSpan.FromHours(24) - StartTime + EndTime;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StartTime;
            yield return EndTime;
            yield return RateMultiplier;
            yield return IsPeak;
        }

        public override string ToString()
        {
            return $"{StartTime:hh\\:mm}-{EndTime:hh\\:mm} (x{RateMultiplier:F1})";
        }
    }
}