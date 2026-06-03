// ChargingStationManagement.Domain/Events/StationEvents.cs
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Interfaces;
using ChargingStationManagement.Domain.ValueObjects;
using System;

namespace ChargingStationManagement.Domain.Events
{
    // 充电站相关事件
    public class StationCreatedEvent : DomainEvent
    {
        public Guid StationId { get; }
        public string ExternalStationId { get; }
        public string StationName { get; }

        public StationCreatedEvent(Guid stationId, string externalStationId, string stationName)
        {
            StationId = stationId;
            ExternalStationId = externalStationId;
            StationName = stationName;
        }
    }

    public class StationStatusChangedEvent : DomainEvent
    {
        private Guid id;
        private string stationId;
        private StationStatus oldStatus;
        private StationStatus newStatus;

        public Guid StationId { get; }
        public string ExternalStationId { get; }
        public int OldStatus { get; }
        public int NewStatus { get; }
        public string Reason { get; }

        public StationStatusChangedEvent(Guid stationId, string externalStationId, int oldStatus, int newStatus, string reason = null)
        {
            StationId = stationId;
            ExternalStationId = externalStationId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            Reason = reason;
        }

        public StationStatusChangedEvent(Guid id, string stationId, StationStatus oldStatus, StationStatus newStatus, string reason)
        {
            this.id = id;
            this.stationId = stationId;
            this.oldStatus = oldStatus;
            this.newStatus = newStatus;
            Reason = reason;
        }
    }

    public class StationRatesUpdatedEvent : DomainEvent
    {
        private Guid id;
        private string stationId;
        private Rate? electricityRate;
        private Rate? serviceRate;
        private Rate? parkRate;

        public Guid StationId { get; }
        public string ExternalStationId { get; }
        public decimal ElectricityRate { get; }
        public decimal ServiceRate { get; }
        public decimal ParkRate { get; }

        public StationRatesUpdatedEvent(Guid stationId, string externalStationId,
            decimal electricityRate, decimal serviceRate, decimal parkRate)
        {
            StationId = stationId;
            ExternalStationId = externalStationId;
            ElectricityRate = electricityRate;
            ServiceRate = serviceRate;
            ParkRate = parkRate;
        }

        public StationRatesUpdatedEvent(Guid id, string stationId, Rate? electricityRate, Rate? serviceRate, Rate? parkRate)
        {
            this.id = id;
            this.stationId = stationId;
            this.electricityRate = electricityRate;
            this.serviceRate = serviceRate;
            this.parkRate = parkRate;
        }
    }

    public class StationInfoUpdatedEvent : DomainEvent
    {
        public Guid StationId { get; }
        public string ExternalStationId { get; }

        public StationInfoUpdatedEvent(Guid stationId, string externalStationId)
        {
            StationId = stationId;
            ExternalStationId = externalStationId;
        }
    }
}