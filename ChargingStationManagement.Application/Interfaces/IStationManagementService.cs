using ChargingStationManagement.Services.DTOs;

namespace ChargingStationManagement.Services.Interfaces
{
    public interface IStationManagementService
    {
        // 充电站管理
        Task<List<StationDto>> GetAvailableStationsAsync(decimal latitude, decimal longitude, decimal radiusKm);
        Task<StationDetailDto> GetStationDetailAsync(string stationId);
        Task<List<EquipmentDto>> GetStationEquipmentAsync(string stationId);
        Task<List<ConnectorDto>> GetAvailableConnectorsAsync(string stationId);

        // 实时状态管理
        Task UpdateConnectorStatusAsync(string connectorId, int status, int? parkStatus, int? lockStatus);
        Task<List<ConnectorStatusDto>> GetConnectorsStatusAsync(List<string> connectorIds);

        // 费率计算
        Task<ChargingCostDto> CalculateChargingCostAsync(string stationId, decimal energyKwh, TimeSpan duration, bool includeParking);

        // 数据同步
        Task SyncThirdPartyDataAsync();
        Task MergeMultipleThirdPartyDataAsync(List<string> thirdPartyNames);
    }
}