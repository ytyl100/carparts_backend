namespace ChargingStationManagement.Domain.ValueObjects;

public class Rate
{
    public decimal ElectricityRate { get; private set; } // 元/kWh
    public decimal ServiceRate { get; private set; }     // 元/kWh
    public decimal ParkRate { get; private set; }        // 元/小时
    public decimal TimeRate { get; private set; }        // 元/分钟

    private Rate() { }

    public Rate(decimal electricityRate, decimal serviceRate, decimal parkRate, decimal timeRate)
    {
        ElectricityRate = electricityRate;
        ServiceRate = serviceRate;
        ParkRate = parkRate;
        TimeRate = timeRate;
    }
}