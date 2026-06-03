// ChargingStationManagement.Domain/Interfaces/IStationService.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingStationManagement.Domain.Interfaces
{
    /// <summary>
    /// 充电站领域服务接口
    /// </summary>
    public interface IStationService
    {
        Task<Station> CreateStationAsync(
            string stationId,
            string operatorId,
            string stationName,
            Address address,
            Coordinates location,
            string source);

        Task UpdateStationStatusAsync(Guid stationId, StationStatus status, string reason = null);
        Task UpdateStationRatesAsync(Guid stationId, Rate electricityRate, Rate serviceRate, Rate parkRate);
        Task AddEquipmentToStationAsync(Guid stationId, Equipment equipment);
        Task<IEnumerable<Station>> GetAvailableStationsAsync(
            Coordinates userLocation,
            decimal radiusKm,
            decimal? minPower = null);
        Task<decimal> CalculateChargingCostAsync(
            Guid stationId,
            decimal energyKwh,
            TimeSpan duration,
            bool includeParking = true);
        Task UpdateStationStatisticsAsync(Guid stationId, decimal electricity, decimal revenue);
    }
}