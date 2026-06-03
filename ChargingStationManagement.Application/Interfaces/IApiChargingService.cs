using System.Threading.Tasks;
using ChargingStationManagement.Services.DTOs;

namespace ChargingStationManagement.Services.Interfaces
{
    public interface IApiChargingService
    {
        // 充电流程
        Task<StartSessionResultDto> StartChargingSessionAsync(string userId, string connectorId, ChargingMode mode);
        Task UpdateChargingSessionDataAsync(string sessionId, ChargingDataDto data);
        Task<StopSessionResultDto> StopChargingSessionAsync(string sessionId, string stoppedBy, string reason);
        Task<SessionDto> GetSessionAsync(string sessionId);

        // 实时监控
        Task<List<ActiveSessionDto>> GetActiveSessionsAsync();
        Task CheckAndHandleLowBalanceSessionsAsync();
        Task CheckScheduledEndSessionsAsync();

        // 订单处理
        Task<OrderDto> CompleteChargingOrderAsync(string sessionId);
        Task<bool> ProcessPaymentAsync(string orderId, string paymentMethod);
    }
}
