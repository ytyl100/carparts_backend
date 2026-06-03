namespace ChargingStationManagement.Services.ApplicationServices
{
    internal class StartChargeRequest
    {
        public string ConnectorId { get; set; }
        public string UserId { get; set; }
    }
}