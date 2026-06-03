// ChargingStationManagement.Domain/Events/IDomainEvent.cs
using MediatR;

namespace ChargingStationManagement.Domain.Interfaces
{
    /// <summary>
    /// 领域事件接口
    /// </summary>
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
        string EventType { get; }
    }

    /// <summary>
    /// 领域事件基类
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        public DateTime OccurredOn { get; }
        public string EventType => GetType().Name;

        protected DomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
        }
    }
}