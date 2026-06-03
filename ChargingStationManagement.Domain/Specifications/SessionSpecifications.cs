// ChargingStationManagement.Domain/Specifications/SessionSpecifications.cs
using System;
using System.Linq.Expressions;
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;

namespace ChargingStationManagement.Domain.Specifications
{
    // 替换所有直接赋值 Criteria = ... 的地方，改为调用基类的构造函数传递表达式

    public class ActiveSessionsSpecification : BaseSpecification<Session>
    {
        public ActiveSessionsSpecification(bool includeUser = false, bool includeConnector = false)
            : base(s => s.Status == ChargeStatus.Charging || s.Status == ChargeStatus.Starting)
        {
            if (includeUser)
            {
                AddInclude(s => s.User);
            }

            if (includeConnector)
            {
                AddInclude(s => s.Connector);
                AddInclude("Connector.Equipment");
                AddInclude("Connector.Equipment.Station");
            }

            ApplyOrderBy(s => s.StartTime);
        }
    }

    public class UserSessionsSpecification : BaseSpecification<Session>
    {
        public UserSessionsSpecification(Guid userId, int page = 1, int pageSize = 20, bool includeConnector = true)
            : base(s => s.UserId == userId)
        {
            if (includeConnector)
            {
                AddInclude(s => s.Connector);
                AddInclude("Connector.Equipment");
                AddInclude("Connector.Equipment.Station");
            }

            ApplyOrderByDescending(s => s.StartTime);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }
    }

    public class StationSessionsSpecification : BaseSpecification<Session>
    {
        public StationSessionsSpecification(Guid stationId, DateTime? startDate = null, DateTime? endDate = null)
            : base(
                startDate.HasValue && endDate.HasValue
                    ? (Expression<Func<Session, bool>>)(s => s.StationId == stationId && s.StartTime >= startDate.Value && s.StartTime <= endDate.Value)
                    : startDate.HasValue
                        ? (Expression<Func<Session, bool>>)(s => s.StationId == stationId && s.StartTime >= startDate.Value)
                        : endDate.HasValue
                            ? (Expression<Func<Session, bool>>)(s => s.StationId == stationId && s.StartTime <= endDate.Value)
                            : (Expression<Func<Session, bool>>)(s => s.StationId == stationId)
            )
        {
            AddInclude(s => s.User);
            AddInclude(s => s.Connector);
            ApplyOrderByDescending(s => s.StartTime);
        }
    }

    public class SessionByIdSpecification : BaseSpecification<Session>
    {
        public SessionByIdSpecification(string sessionId, bool includeAll = true)
            : base(s => s.SessionId == sessionId)
        {
            if (includeAll)
            {
                AddInclude(s => s.User);
                AddInclude(s => s.Connector);
                AddInclude("Connector.Equipment");
                AddInclude("Connector.Equipment.Station");
                AddInclude(s => s.Station);
            }
        }
    }

    public class UnpaidSessionsSpecification : BaseSpecification<Session>
    {
        public UnpaidSessionsSpecification(DateTime? olderThan = null)
            : base(
                olderThan.HasValue
                    ? (Expression<Func<Session, bool>>)(s => !s.IsPaid && s.OrderStatus == OrderStatus.Completed && s.TotalAmount > 0 && s.EndTime <= olderThan.Value)
                    : (Expression<Func<Session, bool>>)(s => !s.IsPaid && s.OrderStatus == OrderStatus.Completed && s.TotalAmount > 0)
            )
        {
            AddInclude(s => s.User);
            ApplyOrderBy(s => s.EndTime);
        }
    }

    public class SessionsByDateRangeSpecification : BaseSpecification<Session>
    {
        public SessionsByDateRangeSpecification(DateTime startDate, DateTime endDate, bool includeUser = false)
            : base(s => s.StartTime >= startDate && s.StartTime <= endDate)
        {
            if (includeUser)
            {
                AddInclude(s => s.User);
            }

            AddInclude(s => s.Station);
            ApplyOrderByDescending(s => s.StartTime);
        }
    }
}