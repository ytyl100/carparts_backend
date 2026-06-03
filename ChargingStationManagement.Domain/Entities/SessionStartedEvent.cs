// ChargingStationManagement.Domain/Entities/Connector.cs
using ChargingStationManagement.Domain.Interfaces;

namespace ChargingStationManagement.Domain.Entities
{
    internal class SessionStartedEvent : IDomainEvent
    {
        private Guid id;
        private string connectorId;
        private Guid sessionId;
        private string userId;

        public SessionStartedEvent(Guid id, string connectorId, Guid sessionId, string userId)
        {
            this.id = id;
            this.connectorId = connectorId;
            this.sessionId = sessionId;
            this.userId = userId;
        }

        public DateTime OccurredOn => throw new NotImplementedException();

        public string EventType => throw new NotImplementedException();
    }
}