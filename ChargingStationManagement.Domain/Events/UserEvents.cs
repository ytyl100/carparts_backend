// ChargingStationManagement.Domain/Events/UserEvents.cs
using ChargingStationManagement.Domain.Interfaces;
using System;

namespace ChargingStationManagement.Domain.Events
{
    // 用户相关事件
    public class UserRegisteredEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string ExternalUserId { get; }
        public string PhoneNumber { get; }
        public string Name { get; }

        public UserRegisteredEvent(Guid userId, string externalUserId, string phoneNumber, string name)
        {
            UserId = userId;
            ExternalUserId = externalUserId;
            PhoneNumber = phoneNumber;
            Name = name;
        }
    }

    public class UserProfileUpdatedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string ExternalUserId { get; }

        public UserProfileUpdatedEvent(Guid userId, string externalUserId)
        {
            UserId = userId;
            ExternalUserId = externalUserId;
        }
    }

    public class UserVerifiedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string ExternalUserId { get; }
        public string Method { get; }

        public UserVerifiedEvent(Guid userId, string externalUserId, string method)
        {
            UserId = userId;
            ExternalUserId = externalUserId;
            Method = method;
        }
    }

    public class UserActivatedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string ExternalUserId { get; }

        public UserActivatedEvent(Guid userId, string externalUserId)
        {
            UserId = userId;
            ExternalUserId = externalUserId;
        }
    }

    public class UserDeactivatedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string ExternalUserId { get; }
        public string Reason { get; }

        public UserDeactivatedEvent(Guid userId, string externalUserId, string reason = null)
        {
            UserId = userId;
            ExternalUserId = externalUserId;
            Reason = reason;
        }
    }

    public class VehicleAddedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string ExternalUserId { get; }
        public Guid VehicleId { get; }
        public string LicensePlate { get; }

        public VehicleAddedEvent(Guid userId, string externalUserId, Guid vehicleId, string licensePlate)
        {
            UserId = userId;
            ExternalUserId = externalUserId;
            VehicleId = vehicleId;
            LicensePlate = licensePlate;
        }
    }

    public class VehicleRemovedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string ExternalUserId { get; }
        public Guid VehicleId { get; }
        public string LicensePlate { get; }

        public VehicleRemovedEvent(Guid userId, string externalUserId, Guid vehicleId, string licensePlate)
        {
            UserId = userId;
            ExternalUserId = externalUserId;
            VehicleId = vehicleId;
            LicensePlate = licensePlate;
        }
    }

    public class StationFavoritedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string ExternalUserId { get; }
        public string StationId { get; }
        public string StationName { get; }

        public StationFavoritedEvent(Guid userId, string externalUserId, string stationId, string stationName)
        {
            UserId = userId;
            ExternalUserId = externalUserId;
            StationId = stationId;
            StationName = stationName;
        }
    }

    public class StationUnfavoritedEvent : DomainEvent
    {
        public Guid UserId { get; }
        public string ExternalUserId { get; }
        public string StationId { get; }

        public StationUnfavoritedEvent(Guid userId, string externalUserId, string stationId)
        {
            UserId = userId;
            ExternalUserId = externalUserId;
            StationId = stationId;
        }
    }
}