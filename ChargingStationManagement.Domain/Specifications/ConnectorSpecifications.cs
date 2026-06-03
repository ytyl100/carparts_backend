// ChargingStationManagement.Domain/Specifications/ConnectorSpecifications.cs
using System;
using System.Linq.Expressions;
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;

namespace ChargingStationManagement.Domain.Specifications
{
    // 将 Criteria 的赋值方式改为通过构造函数参数传递给 BaseSpecification
    // 假设 BaseSpecification 有合适的构造函数或方法用于设置 Criteria

    public class AvailableConnectorsSpecification : BaseSpecification<Connector>
    {
        public AvailableConnectorsSpecification(bool includeEquipment = false, bool includeStation = false)
            : base(c => c.Status == ConnectorStatus.Idle &&
                        c.ParkStatus != ParkStatus.Occupied &&
                        c.LockStatus != LockStatus.Locked)
        {
            if (includeEquipment)
            {
                AddInclude(c => c.Equipment);

                if (includeStation)
                {
                    AddInclude("Equipment.Station");
                }
            }

            ApplyOrderBy(c => c.Power);
        }
    }

    public class ConnectorByEquipmentSpecification : BaseSpecification<Connector>
    {
        public ConnectorByEquipmentSpecification(Guid equipmentId, bool availableOnly = true)
            : base(availableOnly
                ? (Expression<Func<Connector, bool>>)(c => c.EquipmentId == equipmentId && c.Status == ConnectorStatus.Idle)
                : (c => c.EquipmentId == equipmentId))
        {
            ApplyOrderBy(c => c.ConnectorId);
        }
    }

    public class ConnectorByIdSpecification : BaseSpecification<Connector>
    {
        public ConnectorByIdSpecification(string connectorId, bool includeEquipment = true, bool includeStation = false)
            : base(c => c.ConnectorId == connectorId)
        {
            if (includeEquipment)
            {
                AddInclude(c => c.Equipment);

                if (includeStation)
                {
                    AddInclude("Equipment.Station");
                }
            }
        }
    }

    public class ChargingConnectorsSpecification : BaseSpecification<Connector>
    {
        public ChargingConnectorsSpecification(bool includeEquipment = true, bool includeSession = false)
            : base(c => c.Status == ConnectorStatus.OccupiedCharging)
        {
            if (includeEquipment)
            {
                AddInclude(c => c.Equipment);
            }

            if (includeSession)
            {
                // 需要通过其他方式获取当前会话信息
            }

            ApplyOrderBy(c => c.SessionStartTime);
        }
    }

    public class ConnectorByStandardSpecification : BaseSpecification<Connector>
    {
        public ConnectorByStandardSpecification(ConnectorStandard standard, bool availableOnly = true)
            : base(availableOnly
                ? (Expression<Func<Connector, bool>>)(c => c.Standard == standard &&
                                                          c.Status == ConnectorStatus.Idle &&
                                                          c.ParkStatus != ParkStatus.Occupied)
                : (c => c.Standard == standard))
        {
            ApplyOrderBy(c => c.Power);
        }
    }
}