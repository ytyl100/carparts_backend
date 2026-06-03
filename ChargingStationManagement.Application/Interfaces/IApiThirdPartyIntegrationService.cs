using ChargingStationManagement.Domain.Interfaces;
using ChargingStationManagement.Services.ApplicationServices;
using ChargingStationManagement.Services.DTOs.ThirdParty;

namespace ChargingStationManagement.Services.Interfaces
{
    public interface IApiThirdPartyIntegrationService
    {
        Task<List<ThirdPartyStationDto>> SyncStationsAsync(string thirdPartyName);
        Task<List<StationStatusDto>> GetStationsStatusAsync(string thirdPartyName, List<string> stationIds);
        Task<StationStatsDto> GetStationStatsAsync(string thirdPartyName, string stationId, DateTime startDate, DateTime endDate);

        // 充电业务接口
        Task<EquipmentAuthResultDto> RequestEquipmentAuthAsync(string thirdPartyName, string connectorId);
        Task<StartChargeResultDto> StartChargingAsync(string thirdPartyName, string connectorId, string userId);
        Task<StopChargeResultDto> StopChargingAsync(string thirdPartyName, string startChargeSeq);
        Task<ChargeStatusDto> GetChargeStatusAsync(string thirdPartyName, string startChargeSeq);

        // Token管理
        Task<string> GetAccessTokenAsync(string thirdPartyName);
        Task StartChargingAsync(string thirdPartyName, string connectorId, StartChargeRequest startChargeRequest);
    }
}