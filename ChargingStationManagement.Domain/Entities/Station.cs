// ChargingStationManagement.Domain/Entities/Station.cs
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Events;
using ChargingStationManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChargingStationManagement.Domain.Entities
{
    /// <summary>
    /// 充电站实体（聚合根）
    /// </summary>
    public class Station : AggregateRoot
    {
        // 基本信息
        public string StationId { get; private set; }           // 充电站唯一ID
        public string OperatorId { get; private set; }          // 运营商ID
        public string EquipmentOwnerId { get; private set; }    // 设备所属方ID
        public string StationName { get; private set; }         // 充电站名称
        public string CountryCode { get; private set; }         // 国家代码
        public string AreaCode { get; private set; }            // 区域代码

        // 位置信息
        public Address Address { get; private set; }            // 地址值对象
        public Coordinates Location { get; private set; }       // 坐标值对象

        // 联系信息
        public string StationTel { get; private set; }          // 充电站电话
        public string ServiceTel { get; private set; }          // 服务电话

        // 状态信息
        public StationStatus Status { get; private set; }       // 充电站状态
        public int Construction { get; private set; }           // 建设状态
        public int ParkNums { get; private set; }               // 停车位数量
        public DateTime? LastMaintenanceDate { get; private set; } // 最后维护日期

        // 扩展信息
        public string SiteGuide { get; private set; }           // 站点引导
        public string Pictures { get; private set; }            // 图片URLs（分号分隔）
        public string MatchCars { get; private set; }           // 适配车型
        public string ParkInfo { get; private set; }            // 停车信息
        public string BusinessHours { get; private set; }       // 营业时间

        // 费率信息
        public Rate ElectricityRate { get; private set; }       // 电费率值对象
        public Rate ServiceRate { get; private set; }           // 服务费率值对象
        public Rate ParkRate { get; private set; }              // 停车费率值对象

        // 统计信息
        public int TotalConnectors => _equipment.Sum(e => e.Connectors.Count);
        public int AvailableConnectors => _equipment.Sum(e => e.AvailableConnectors);
        public decimal TotalPower => _equipment.Sum(e => e.Power);
        public decimal StationElectricity { get; private set; } // 累计总电量（kWh）
        public decimal TotalRevenue { get; private set; }       // 累计总收入

        // 时间戳
        public DateTime LastSyncTime { get; private set; }      // 最后同步时间
        public DateTime? LastStatusChangeTime { get; private set; } // 最后状态变更时间

        // 源信息
        public string Source { get; private set; }              // 数据来源（第三方名称）

        // 导航属性
        private readonly List<Equipment> _equipment = new List<Equipment>();
        private readonly List<StationStatusHistory> _statusHistory = new List<StationStatusHistory>();
        private int stationType;
        private int stationStatus;
        private Rate electricityRate;
        private Rate serviceRate;
        private Rate parkRate;

        public IReadOnlyCollection<Equipment> Equipment => _equipment.AsReadOnly();
        public IReadOnlyCollection<StationStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

        public object Operator { get; set; }
        public int StationLat { get; set; }
        public int StationLng { get; set; }        

        // 构造函数
        protected Station() { }

        public Station(
            string stationId,
            string operatorId,
            string stationName,
            Address address,
            Coordinates location,
            string source,
            string createdBy = "system")
        {
            if (string.IsNullOrWhiteSpace(stationId))
                throw new ArgumentException("Station ID cannot be empty", nameof(stationId));

            if (stationId.Length > 20)
                throw new ArgumentException("Station ID cannot exceed 20 characters", nameof(stationId));

            StationId = stationId;
            OperatorId = operatorId ?? throw new ArgumentNullException(nameof(operatorId));
            StationName = stationName ?? throw new ArgumentNullException(nameof(stationName));
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Location = location ?? throw new ArgumentNullException(nameof(location));
            Source = source ?? throw new ArgumentNullException(nameof(source));

            Status = StationStatus.Normal;
            Construction = 1; // 已建成
            LastSyncTime = DateTime.UtcNow;
            CreatedBy = createdBy;

            // 设置默认费率
            ElectricityRate = new Rate(1.5m, 0.2m, 0m, 0m); // 1.5元/kWh，服务费0.2元/kWh
            ServiceRate = new Rate(0m, 0.2m, 0m, 0m);
            ParkRate = new Rate(0m, 0m, 5m, 0m); // 5元/小时

            AddStatusHistory(StationStatus.Normal, "Station created");
        }

        public Station(string stationId, string operatorId, string stationName, Address address, Coordinates location, string source, string createdBy = "system", int stationType = 0, int stationStatus = 0, int parkNums = 0, string siteGuide = null, string pictures = null, string matchCars = null, string parkInfo = null, string businessHours = null, Rate electricityRate = null, Rate serviceRate = null, Rate parkRate = null, string stationTel = null) : this(stationId, operatorId, stationName, address, location, source, createdBy)
        {
            this.stationType = stationType;
            this.stationStatus = stationStatus;
            ParkNums = parkNums;
            SiteGuide = siteGuide;
            Pictures = pictures;
            MatchCars = matchCars;
            ParkInfo = parkInfo;
            BusinessHours = businessHours;
            this.electricityRate = electricityRate;
            this.serviceRate = serviceRate;
            this.parkRate = parkRate;
            StationTel = stationTel;
        }

        public Station(string stationId, string operatorId, string stationName, Address address, Coordinates location, string source, string createdBy = "system", int stationType = 0, int stationStatus = 0, int parkNums = 0, string siteGuide = null, string pictures = null, string matchCars = null, string parkInfo = null, string businessHours = null, Rate electricityRate = null, Rate serviceRate = null, Rate parkRate = null, string stationTel = null, string serviceTel = null) : this(stationId, operatorId, stationName, address, location, source, createdBy, stationType, stationStatus, parkNums, siteGuide, pictures, matchCars, parkInfo, businessHours, electricityRate, serviceRate, parkRate, stationTel)
        {
        }

        // 业务方法
        public void UpdateBasicInfo(
            string stationName,
            Address address,
            string stationTel,
            string serviceTel,
            string siteGuide,
            string businessHours)
        {
            StationName = stationName ?? StationName;
            Address = address ?? Address;
            StationTel = stationTel;
            ServiceTel = serviceTel;
            SiteGuide = siteGuide;
            BusinessHours = businessHours;

            Update();

            AddDomainEvent(new StationInfoUpdatedEvent(Id, StationId));
        }

        public void UpdateStatus(StationStatus newStatus, string reason = null)
        {
            if (Status != newStatus)
            {
                var oldStatus = Status;
                Status = newStatus;
                LastStatusChangeTime = DateTime.UtcNow;

                Update();

                AddStatusHistory(newStatus, reason);
                AddDomainEvent(new StationStatusChangedEvent(Id, StationId, oldStatus, newStatus, reason));
            }
        }

        public void UpdateRates(Rate electricityRate, Rate serviceRate, Rate parkRate)
        {
            ElectricityRate = electricityRate ?? ElectricityRate;
            ServiceRate = serviceRate ?? ServiceRate;
            ParkRate = parkRate ?? ParkRate;

            Update();

            AddDomainEvent(new StationRatesUpdatedEvent(Id, StationId, electricityRate, serviceRate, parkRate));
        }

        public void AddEquipment(Equipment equipment)
        {
            if (equipment == null)
                throw new ArgumentNullException(nameof(equipment));

            if (_equipment.Any(e => e.EquipmentId == equipment.EquipmentId))
                throw new InvalidOperationException($"Equipment with ID {equipment.EquipmentId} already exists in station {StationId}");

            _equipment.Add(equipment);
            Update();

            AddDomainEvent(new EquipmentAddedEvent(Id, StationId, equipment.Id, equipment.EquipmentId));
        }

        public void RemoveEquipment(string equipmentId)
        {
            var equipment = _equipment.FirstOrDefault(e => e.EquipmentId == equipmentId);
            if (equipment != null)
            {
                _equipment.Remove(equipment);
                Update();

                AddDomainEvent(new EquipmentRemovedEvent(Id, StationId, equipment.Id, equipmentId));
            }
        }

        public Equipment GetEquipment(string equipmentId)
        {
            return _equipment.FirstOrDefault(e => e.EquipmentId == equipmentId);
        }

        public void UpdateStatistics(decimal electricity, decimal revenue)
        {
            StationElectricity += electricity;
            TotalRevenue += revenue;

            Update();
        }

        public void SyncCompleted(string source = null)
        {
            LastSyncTime = DateTime.UtcNow;
            Source = source ?? Source;
            Update();
        }

        private void AddStatusHistory(StationStatus status, string reason)
        {
            var history = new StationStatusHistory(Id, status, reason);
            _statusHistory.Add(history);
        }

        // 查询方法
        public IEnumerable<Connector> GetAllConnectors()
        {
            return _equipment.SelectMany(e => e.Connectors);
        }

        public IEnumerable<Connector> GetAvailableConnectors()
        {
            return GetAllConnectors().Where(c => c.Status == ConnectorStatus.Idle);
        }

        public decimal CalculateChargingCost(decimal energyKwh, TimeSpan duration, bool includeParking = true)
        {
            var electricityCost = energyKwh * ElectricityRate.ElectricityRate;
            var serviceCost = energyKwh * ServiceRate.ServiceRate;
            var parkCost = includeParking ? (decimal)duration.TotalHours * ParkRate.ParkRate : 0;

            return electricityCost + serviceCost + parkCost;
        }
    }

    /// <summary>
    /// 充电站状态历史记录
    /// </summary>
    public class StationStatusHistory : BaseEntity
    {
        public Guid StationId { get; private set; }
        public StationStatus Status { get; private set; }
        public string Reason { get; private set; }
        public DateTime ChangeTime { get; private set; }

        public StationStatusHistory(Guid stationId, StationStatus status, string reason = null)
        {
            StationId = stationId;
            Status = status;
            Reason = reason;
            ChangeTime = DateTime.UtcNow;
        }
    }
}