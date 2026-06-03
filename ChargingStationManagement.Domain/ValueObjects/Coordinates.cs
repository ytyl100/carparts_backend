// ChargingStationManagement.Domain/ValueObjects/Coordinates.cs
namespace ChargingStationManagement.Domain.ValueObjects
{
    /// <summary>
    /// 地理坐标值对象
    /// </summary>
    public class Coordinates : ValueObject
    {
        public decimal Latitude { get; }
        public decimal Longitude { get; }
        public decimal? Altitude { get; }

        public Coordinates(decimal latitude, decimal longitude, decimal? altitude = null)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90");

            if (longitude < -180 || longitude > 180)
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180");

            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Latitude;
            yield return Longitude;
            yield return Altitude ?? 0;
        }

        public decimal DistanceTo(Coordinates other, string unit = "km")
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var R = unit.ToLower() == "km" ? 6371 : 3959; // 地球半径（km或mi）
            var dLat = ToRadians(other.Latitude - Latitude);
            var dLon = ToRadians(other.Longitude - Longitude);
            var lat1 = ToRadians(Latitude);
            var lat2 = ToRadians(other.Latitude);

            var a = Math.Sin((double)(dLat / 2)) * Math.Sin((double)(dLat / 2)) +
                    Math.Sin((double)(dLon / 2)) * Math.Sin((double)(dLon / 2)) *
                    Math.Cos((double)lat1) * Math.Cos((double)lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return (decimal)(R * c);
        }

        private decimal ToRadians(decimal degrees)
        {
            return degrees * (decimal)Math.PI / 180;
        }

        public override string ToString() => $"({Latitude}, {Longitude})";
    }
}