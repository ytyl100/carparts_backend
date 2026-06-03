// ChargingStationManagement.Domain/Events/EquipmentEvents.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Interfaces;
using System;

namespace ChargingStationManagement.Domain.Events
{
    // 设备相关事件
    public class EquipmentAddedEvent : DomainEvent
    {
        public Guid StationId { get; }
        public Guid EquipmentId { get; }
        public string ExternalEquipmentId { get; }

        public EquipmentAddedEvent(Guid stationId, string stationId1, Guid equipmentId, string externalEquipmentId)
        {
            StationId = stationId;
            EquipmentId = equipmentId;
            ExternalEquipmentId = externalEquipmentId;
        }
    }

    public class EquipmentRemovedEvent : DomainEvent
    {
        public Guid StationId { get; }
        public Guid EquipmentId { get; }
        public string ExternalEquipmentId { get; }

        public EquipmentRemovedEvent(Guid stationId, string stationId1, Guid equipmentId, string externalEquipmentId)
        {
            StationId = stationId;
            EquipmentId = equipmentId;
            ExternalEquipmentId = externalEquipmentId;
        }
    }

    public class EquipmentStatusChangedEvent : DomainEvent
    {
        private Guid id;
        private string equipmentId;
        private EquipmentStatus oldStatus;
        private EquipmentStatus newStatus;

        public Guid EquipmentId { get; }
        public string ExternalEquipmentId { get; }
        public int OldStatus { get; }
        public int NewStatus { get; }
        public string Reason { get; }

        public EquipmentStatusChangedEvent(Guid equipmentId, string externalEquipmentId,
            int oldStatus, int newStatus, string reason = null)
        {
            EquipmentId = equipmentId;
            ExternalEquipmentId = externalEquipmentId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            Reason = reason;
        }

        public EquipmentStatusChangedEvent(Guid id, string equipmentId, EquipmentStatus oldStatus, EquipmentStatus newStatus, string reason)
        {
            this.id = id;
            this.equipmentId = equipmentId;
            this.oldStatus = oldStatus;
            this.newStatus = newStatus;
            Reason = reason;
        }
    }

    public class EquipmentPowerUpdatedEvent : DomainEvent
    {
        public Guid EquipmentId { get; }
        public string ExternalEquipmentId { get; }
        public decimal NewPower { get; }

        public EquipmentPowerUpdatedEvent(Guid equipmentId, string externalEquipmentId, decimal newPower)
        {
            EquipmentId = equipmentId;
            ExternalEquipmentId = externalEquipmentId;
            NewPower = newPower;
        }
    }

    public class PowerConfigurationUpdatedEvent : DomainEvent
    {
        public Guid EquipmentId { get; }
        public string ExternalEquipmentId { get; }
        public decimal MinPower { get; }
        public decimal MaxPower { get; }
        public bool SupportsDynamic { get; }

        public PowerConfigurationUpdatedEvent(Guid equipmentId, string externalEquipmentId,
            decimal minPower, decimal maxPower, bool supportsDynamic)
        {
            EquipmentId = equipmentId;
            ExternalEquipmentId = externalEquipmentId;
            MinPower = minPower;
            MaxPower = maxPower;
            SupportsDynamic = supportsDynamic;
        }
    }

    public class EquipmentPowerAdjustedEvent : DomainEvent
    {
        public Guid EquipmentId { get; }
        public string ExternalEquipmentId { get; }
        public decimal OldPower { get; }
        public decimal NewPower { get; }
        public string AdjustedBy { get; }

        public EquipmentPowerAdjustedEvent(Guid equipmentId, string externalEquipmentId,
            decimal oldPower, decimal newPower, string adjustedBy)
        {
            EquipmentId = equipmentId;
            ExternalEquipmentId = externalEquipmentId;
            OldPower = oldPower;
            NewPower = newPower;
            AdjustedBy = adjustedBy;
        }
    }
}