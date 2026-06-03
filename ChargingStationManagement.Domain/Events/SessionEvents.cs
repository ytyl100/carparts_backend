// ChargingStationManagement.Domain/Events/SessionEvents.cs
using ChargingStationManagement.Domain.Interfaces;
using System;

namespace ChargingStationManagement.Domain.Events
{
    // 充电会话相关事件
    public class SessionCreatedEvent : DomainEvent
    {
        public Guid SessionId { get; }
        public string ExternalSessionId { get; }
        public Guid UserId { get; }
        public Guid ConnectorId { get; }
        public Guid StationId { get; }

        public SessionCreatedEvent(Guid sessionId, string externalSessionId, Guid userId, Guid connectorId, Guid stationId)
        {
            SessionId = sessionId;
            ExternalSessionId = externalSessionId;
            UserId = userId;
            ConnectorId = connectorId;
            StationId = stationId;
        }
    }

    public class ChargingStartedEvent : DomainEvent
    {
        public Guid SessionId { get; }
        public string ExternalSessionId { get; }
        public Guid ConnectorId { get; }
        public Guid UserId { get; }

        public ChargingStartedEvent(Guid sessionId, string externalSessionId, Guid connectorId, Guid userId)
        {
            SessionId = sessionId;
            ExternalSessionId = externalSessionId;
            ConnectorId = connectorId;
            UserId = userId;
        }
    }

    public class ChargingStoppingEvent : DomainEvent
    {
        public Guid SessionId { get; }
        public string ExternalSessionId { get; }
        public string StoppedBy { get; }
        public string Reason { get; }

        public ChargingStoppingEvent(Guid sessionId, string externalSessionId, string stoppedBy, string reason = null)
        {
            SessionId = sessionId;
            ExternalSessionId = externalSessionId;
            StoppedBy = stoppedBy;
            Reason = reason;
        }
    }

    public class ChargingCompletedEvent : DomainEvent
    {
        public Guid SessionId { get; }
        public string ExternalSessionId { get; }
        public decimal TotalEnergy { get; }
        public decimal TotalAmount { get; }
        public DateTime EndTime { get; }

        public ChargingCompletedEvent(Guid sessionId, string externalSessionId,
            decimal totalEnergy, decimal totalAmount, DateTime endTime)
        {
            SessionId = sessionId;
            ExternalSessionId = externalSessionId;
            TotalEnergy = totalEnergy;
            TotalAmount = totalAmount;
            EndTime = endTime;
        }
    }

    public class SessionEndedEvent : DomainEvent
    {
        public Guid ConnectorId { get; }
        public string ExternalConnectorId { get; }
        public Guid SessionId { get; }
        public string UserId { get; }

        public SessionEndedEvent(Guid connectorId, string externalConnectorId, Guid sessionId, string userId)
        {
            ConnectorId = connectorId;
            ExternalConnectorId = externalConnectorId;
            SessionId = sessionId;
            UserId = userId;
        }
    }

    public class SessionCancelledEvent : DomainEvent
    {
        public Guid SessionId { get; }
        public string ExternalSessionId { get; }
        public string CancelledBy { get; }
        public string Reason { get; }

        public SessionCancelledEvent(Guid sessionId, string externalSessionId, string cancelledBy, string reason = null)
        {
            SessionId = sessionId;
            ExternalSessionId = externalSessionId;
            CancelledBy = cancelledBy;
            Reason = reason;
        }
    }

    public class SessionPaidEvent : DomainEvent
    {
        public Guid SessionId { get; }
        public string ExternalSessionId { get; }
        public string PaymentTransactionId { get; }
        public decimal Amount { get; }

        public SessionPaidEvent(Guid sessionId, string externalSessionId, string paymentTransactionId, decimal amount)
        {
            SessionId = sessionId;
            ExternalSessionId = externalSessionId;
            PaymentTransactionId = paymentTransactionId;
            Amount = amount;
        }
    }
}