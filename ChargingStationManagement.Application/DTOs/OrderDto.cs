using ChargingStationManagement.Services.DTOs;

public class OrderDto
{
    public string OrderId { get; set; }
    public string SessionId { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string StationId { get; set; }
    public string StationName { get; set; }
    public string ConnectorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public decimal TotalEnergy { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ElectricityCost { get; set; }
    public decimal ServiceCost { get; set; }
    public int ParkCost { get; set; }
    public RateDto Rates { get; set; }
    public string PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; internal set; }
}