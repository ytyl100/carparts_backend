// ChargingStationManagement.Domain/Interfaces/IChargingService.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.ValueObjects;
using System;
using System.Threading.Tasks;

namespace ChargingStationManagement.Domain.Interfaces
{
    /// <summary>
    /// 充电服务领域服务接口
    /// </summary>
    public interface IChargingService
    {
        Task<Session> StartChargingAsync(
            string sessionId,
            Guid userId,
            Guid connectorId,
            ChargingMode mode = ChargingMode.EnergyBased);

        Task UpdateChargingDataAsync(
            Guid sessionId,
            decimal voltage,
            decimal current,
            decimal power,
            decimal energy,
            decimal batteryLevel = 0);

        Task<Session> StopChargingAsync(
            Guid sessionId,
            string stoppedBy,
            string reason = null,
            decimal? endMeterValue = null);

        Task<Session> CompleteChargingAsync(
            Guid sessionId,
            decimal totalEnergy,
            Rate rates);

        Task<bool> CheckBalanceAndStopIfLowAsync(Guid sessionId, decimal threshold = 10);
        Task<bool> CheckScheduledEndTimeAsync(Guid sessionId);
        Task<decimal> CalculateSessionCostAsync(Guid sessionId);
    }
}