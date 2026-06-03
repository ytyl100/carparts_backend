// ChargingStationManagement.Domain/Events/OperatorEvents.cs
using ChargingStationManagement.Domain.Interfaces;
using System;

namespace ChargingStationManagement.Domain.Events
{
    // 运营商相关事件
    public class OperatorActivatedEvent : DomainEvent
    {
        public Guid OperatorId { get; }
        public string ExternalOperatorId { get; }

        public OperatorActivatedEvent(Guid operatorId, string externalOperatorId)
        {
            OperatorId = operatorId;
            ExternalOperatorId = externalOperatorId;
        }
    }

    public class OperatorDeactivatedEvent : DomainEvent
    {
        public Guid OperatorId { get; }
        public string ExternalOperatorId { get; }
        public string Reason { get; }

        public OperatorDeactivatedEvent(Guid operatorId, string externalOperatorId, string reason = null)
        {
            OperatorId = operatorId;
            ExternalOperatorId = externalOperatorId;
            Reason = reason;
        }
    }

    public class ApiCredentialsUpdatedEvent : DomainEvent
    {
        public Guid OperatorId { get; }
        public string ExternalOperatorId { get; }

        public ApiCredentialsUpdatedEvent(Guid operatorId, string externalOperatorId)
        {
            OperatorId = operatorId;
            ExternalOperatorId = externalOperatorId;
        }
    }
}