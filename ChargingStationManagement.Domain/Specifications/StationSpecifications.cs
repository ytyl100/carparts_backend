// ChargingStationManagement.Domain/Specifications/StationSpecifications.cs
using System;
using System.Linq.Expressions;
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;

namespace ChargingStationManagement.Domain.Specifications
{
    /// <summary>
    /// 充电站规约
    /// </summary>
    public class AvailableStationsSpecification : BaseSpecification<Station>
    {
        public AvailableStationsSpecification(
            decimal userLat,
            decimal userLng,
            decimal radiusKm,
            bool includeEquipment = true,
            bool includeConnectors = true)
            : base(s =>
                s.Status == StationStatus.Normal &&
s.StationLat >= userLat - (radiusKm / 111m) &&
s.StationLat <= userLat + (radiusKm / 111m) &&
s.StationLng >= userLng - (decimal)((double)radiusKm / (111.0 * Math.Cos((double)userLat * Math.PI / 180))) &&
s.StationLng <= userLng + (decimal)((double)radiusKm / (111.0 * Math.Cos((double)userLat * Math.PI / 180))) &&
                s.AvailableConnectors > 0)
        {
            if (includeEquipment)
            {
                AddInclude(s => s.Equipment);

                if (includeConnectors)
                {
                    AddInclude("Equipment.Connectors");
                }
            }

            ApplyOrderBy(s => s.StationName);
        }
    }

    public class StationByOperatorSpecification : BaseSpecification<Station>
    {
        public StationByOperatorSpecification(string operatorId, bool includeEquipment = false)
            : base(s => s.OperatorId == operatorId && s.Status != StationStatus.Offline)
        {
            if (includeEquipment)
            {
                AddInclude(s => s.Equipment);
                AddInclude("Equipment.Connectors");
            }

            ApplyOrderBy(s => s.StationName);
        }
    }

    // 将 Criteria 的赋值改为通过构造函数传递给基类（假设 BaseSpecification 有合适的构造函数）
    // 或者通过 protected/internal set 访问器暴露 Criteria 属性
    // 这里假设 BaseSpecification<T> 有 protected/internal set;，否则请调整 BaseSpecification<T> 的实现

    public class StationWithFaultyEquipmentSpecification : BaseSpecification<Station>
    {
        public StationWithFaultyEquipmentSpecification()
            : base(s => s.Equipment.Any(e => e.Status == EquipmentStatus.Fault))
        {
            AddInclude(s => s.Equipment);
            AddInclude("Equipment.Connectors");

            ApplyOrderBy(s => s.StationName);
        }
    }

    public class StationByIdSpecification : BaseSpecification<Station>
    {
        public StationByIdSpecification(string stationId, bool includeEquipment = true, bool includeOperator = false)
            : base(s => s.StationId == stationId)
        {
            if (includeEquipment)
            {
                AddInclude(s => s.Equipment);
                AddInclude("Equipment.Connectors");
            }

            if (includeOperator)
            {
                AddInclude(s => s.Operator);
            }
        }
    }

    public class StationsByStatusSpecification : BaseSpecification<Station>
    {
        public StationsByStatusSpecification(StationStatus status, int page = 1, int pageSize = 20)
            : base(s => s.Status == status)
        {
            AddInclude(s => s.Equipment);
            AddInclude("Equipment.Connectors");

            ApplyOrderBy(s => s.StationName);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }
    }
}