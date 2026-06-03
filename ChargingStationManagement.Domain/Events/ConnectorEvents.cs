// ChargingStationManagement.Domain/Events/ConnectorEvents.cs
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Interfaces;
using System;

namespace ChargingStationManagement.Domain.Events
{
    // 连接器相关事件
    public class ConnectorCreatedEvent : DomainEvent
    {
        public Guid ConnectorId { get; }
        public Guid EquipmentId { get; }
        public string ExternalConnectorId { get; }

        public ConnectorCreatedEvent(Guid connectorId, Guid equipmentId, string externalConnectorId)
        {
            ConnectorId = connectorId;
            EquipmentId = equipmentId;
            ExternalConnectorId = externalConnectorId;
        }
    }

    public class ConnectorAddedEvent : DomainEvent
    {
        public Guid EquipmentId { get; }
        public Guid ConnectorId { get; }
        public string ExternalConnectorId { get; }

        public ConnectorAddedEvent(Guid equipmentId, string equipmentId1, Guid connectorId, string externalConnectorId)
        {
            EquipmentId = equipmentId;
            ConnectorId = connectorId;
            ExternalConnectorId = externalConnectorId;
        }
    }

    public class ConnectorRemovedEvent : DomainEvent
    {
        public Guid EquipmentId { get; }
        public Guid ConnectorId { get; }
        public string ExternalConnectorId { get; }

        public ConnectorRemovedEvent(Guid equipmentId, string equipmentId1, Guid connectorId, string externalConnectorId)
        {
            EquipmentId = equipmentId;
            ConnectorId = connectorId;
            ExternalConnectorId = externalConnectorId;
        }
    }

    public class ConnectorStatusChangedEvent : DomainEvent
    {
        private Guid id;
        private string connectorId;
        private ConnectorStatus oldStatus;
        private ConnectorStatus newStatus;

        public Guid ConnectorId { get; }
        public string ExternalConnectorId { get; }
        public int OldStatus { get; }
        public int NewStatus { get; }
        public string Reason { get; }

        public ConnectorStatusChangedEvent(Guid connectorId, string externalConnectorId,
            int oldStatus, int newStatus, string reason = null)
        {
            ConnectorId = connectorId;
            ExternalConnectorId = externalConnectorId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            Reason = reason;
        }

        public ConnectorStatusChangedEvent(Guid id, string connectorId, ConnectorStatus oldStatus, ConnectorStatus newStatus, string reason)
        {
            this.id = id;
            this.connectorId = connectorId;
            this.oldStatus = oldStatus;
            this.newStatus = newStatus;
            Reason = reason;
        }
    }

    public class ParkStatusChangedEvent : DomainEvent
    {
        private Guid id;
        private string connectorId;
        private ParkStatus oldStatus;
        private ParkStatus newStatus;

        public Guid ConnectorId { get; }
        public string ExternalConnectorId { get; }
        public int OldStatus { get; }
        public int NewStatus { get; }

        public ParkStatusChangedEvent(Guid connectorId, string externalConnectorId, int oldStatus, int newStatus)
        {
            ConnectorId = connectorId;
            ExternalConnectorId = externalConnectorId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }

        public ParkStatusChangedEvent(Guid id, string connectorId, ParkStatus oldStatus, ParkStatus newStatus)
        {
            this.id = id;
            this.connectorId = connectorId;
            this.oldStatus = oldStatus;
            this.newStatus = newStatus;
        }
    }

    public class LockStatusChangedEvent : DomainEvent
    {
        private Guid id;
        private string connectorId;
        private LockStatus oldStatus;
        private LockStatus newStatus;

        public Guid ConnectorId { get; }
        public string ExternalConnectorId { get; }
        public int OldStatus { get; }
        public int NewStatus { get; }

        public LockStatusChangedEvent(Guid connectorId, string externalConnectorId, int oldStatus, int newStatus)
        {
            ConnectorId = connectorId;
            ExternalConnectorId = externalConnectorId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }

        public LockStatusChangedEvent(Guid id, string connectorId, LockStatus oldStatus, LockStatus newStatus)
        {
            this.id = id;
            this.connectorId = connectorId;
            this.oldStatus = oldStatus;
            this.newStatus = newStatus;
        }
    }

    public class ConnectorPowerChangedEvent : DomainEvent
    {
        public Guid ConnectorId { get; }
        public string ExternalConnectorId { get; }
        public decimal NewPower { get; }

        public ConnectorPowerChangedEvent(Guid connectorId, string externalConnectorId, decimal newPower)
        {
            ConnectorId = connectorId;
            ExternalConnectorId = externalConnectorId;
            NewPower = newPower;
        }
    }
}