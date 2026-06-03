namespace ChargingStationManagement.Domain.ValueObjects;

public class Address
{
    public string FullAddress { get; private set; }
    public string? City { get; private set; }
    public string? Street { get; private set; }

    private Address() { } // EF Core

    public Address(string fullAddress)
    {
        FullAddress = fullAddress;
    }
}