// ChargingStationManagement.Domain/ValueObjects/Address.cs
namespace ChargingStationManagement.Domain.ValueObjects
{
    /// <summary>
    /// 地址值对象
    /// </summary>
    public class Address : ValueObject
    {
        public string Street { get; }
        public string City { get; }
        public string Province { get; }
        public string Country { get; }
        public string PostalCode { get; }
        public string FullAddress { get; }

        public Address(string street, string city, string province, string country, string postalCode)
        {
            Street = street;
            City = city;
            Province = province;
            Country = country;
            PostalCode = postalCode;

            FullAddress = $"{Country}{Province}{City}{Street}";
        }

        public Address(string fullAddress)
        {
            FullAddress = fullAddress ?? string.Empty;

            // 简单解析，实际应用中可能需要更复杂的地址解析
            var parts = fullAddress?.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            if (parts.Length >= 4)
            {
                Country = parts[0];
                Province = parts[1];
                City = parts[2];
                Street = string.Join(" ", parts.Skip(3));
            }
            else
            {
                Street = fullAddress;
            }
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return Province;
            yield return Country;
            yield return PostalCode;
        }

        public override string ToString() => FullAddress;
    }
}