// ChargingStationManagement.Domain/Specifications/EquipmentSpecifications.cs
using System;
using System.Linq.Expressions;
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;

namespace ChargingStationManagement.Domain.Specifications
{
    // 将 Criteria 的赋值改为通过构造函数传递给 BaseSpecification
    public class AvailableEquipmentSpecification : BaseSpecification<Equipment>
    {
        public AvailableEquipmentSpecification(bool includeConnectors = true)
            : base(e => e.Status == EquipmentStatus.Idle && e.AvailableConnectors > 0)
        {
            if (includeConnectors)
            {
                AddInclude(e => e.Connectors);
            }

            ApplyOrderBy(e => e.EquipmentName);
        }
    }

    public class EquipmentByTypeSpecification : BaseSpecification<Equipment>
    {
        public EquipmentByTypeSpecification(EquipmentType equipmentType, bool availableOnly = true)
            : base(availableOnly
                ? (Expression<Func<Equipment, bool>>)(e => e.EquipmentType == equipmentType &&
                                                          e.Status == EquipmentStatus.Idle &&
                                                          e.AvailableConnectors > 0)
                : (e => e.EquipmentType == equipmentType))
        {
            AddInclude(e => e.Connectors);
            ApplyOrderBy(e => e.Power);
        }
    }

    public class EquipmentByPowerRangeSpecification : BaseSpecification<Equipment>
    {
        public EquipmentByPowerRangeSpecification(decimal minPower, decimal maxPower, bool includeStation = false)
            : base(e => e.Power >= minPower && e.Power <= maxPower &&
                        e.Status == EquipmentStatus.Idle &&
                        e.AvailableConnectors > 0)
        {
            AddInclude(e => e.Connectors);

            if (includeStation)
            {
                AddInclude(e => e.Station);
            }

            ApplyOrderByDescending(e => e.Power);
        }
    }

    public class EquipmentByIdSpecification : BaseSpecification<Equipment>
    {
        public EquipmentByIdSpecification(string equipmentId, bool includeConnectors = true, bool includeStation = false)
            : base(e => e.EquipmentId == equipmentId)
        {
            if (includeConnectors)
            {
                AddInclude(e => e.Connectors);
            }

            if (includeStation)
            {
                AddInclude(e => e.Station);
            }
        }
    }

    public class FaultyEquipmentSpecification : BaseSpecification<Equipment>
    {
        public FaultyEquipmentSpecification(bool includeStation = true)
            : base(e => e.Status == EquipmentStatus.Fault || e.Status == EquipmentStatus.Offline)
        {
            if (includeStation)
            {
                AddInclude(e => e.Station);
            }

            ApplyOrderBy(e => e.EquipmentName);
        }
    }
}