// ChargingStationManagement.Domain/Entities/Equipment.cs
using System;
using System.Collections.Generic;
using System.Linq;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Events;
using ChargingStationManagement.Domain.ValueObjects;

namespace ChargingStationManagement.Domain.Entities
{
    /// <summary>
    /// 充电设备实体（聚合根）
    /// </summary>
    public class Equipment : AggregateRoot
    {
        // 设备标识
        public string EquipmentId { get; private set; }          // 设备唯一ID
        public Guid StationId { get; private set; }              // 所属充电站ID
        public string ManufacturerId { get; private set; }       // 制造商ID
        public string ManufacturerName { get; private set; }     // 制造商名称

        // 设备信息
        public string EquipmentModel { get; private set; }       // 设备型号
        public DateTime ProductionDate { get; private set; }     // 生产日期
        public EquipmentType EquipmentType { get; private set; } // 设备类型
        public string EquipmentName { get; private set; }        // 设备名称

        // 位置信息
        public Coordinates? Location { get; private set; }       // 设备具体位置（可选）

        // 技术参数
        public decimal Power { get; private set; }               // 额定功率（kW）
        public decimal MaxPower { get; private set; }            // 最大功率（kW）
        public decimal MinPower { get; private set; }            // 最小功率（kW）
        public decimal Voltage { get; private set; }             // 额定电压（V）
        public decimal Current { get; private set; }             // 额定电流（A）

        // 状态信息
        public EquipmentStatus Status { get; private set; }      // 设备状态
        public DateTime StatusUpdateTime { get; private set; }   // 状态更新时间
        public string StatusReason { get; private set; }         // 状态变更原因

        // 统计信息
        public int TotalConnectors => _connectors.Count;
        public int AvailableConnectors => _connectors.Count(c => c.Status == ConnectorStatus.Idle);
        public decimal EquipmentElectricity { get; private set; } // 设备累计电量（kWh）
        public int TotalSessions { get; private set; }           // 总充电次数
        public TimeSpan TotalChargingTime { get; private set; }  // 总充电时长

        // 配置信息
        public PowerConfiguration PowerConfig { get; private set; } // 功率配置值对象
        public bool SupportsDynamicPower { get; private set; }   // 是否支持动态功率调整
        public string CommunicationProtocol { get; private set; } // 通信协议
        public string FirmwareVersion { get; private set; }      // 固件版本

        // 维护信息
        public DateTime? LastMaintenanceDate { get; private set; } // 最后维护日期
        public DateTime? NextMaintenanceDate { get; private set; } // 下次维护日期
        public string MaintenanceContact { get; private set; }    // 维护联系人

        // 源信息
        public string Source { get; private set; }               // 数据来源

        // 导航属性
        private readonly List<Connector> _connectors = new List<Connector>();
        private readonly List<EquipmentStatusHistory> _statusHistory = new List<EquipmentStatusHistory>();

        public IReadOnlyCollection<Connector> Connectors => _connectors.AsReadOnly();
        public IReadOnlyCollection<EquipmentStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

        public object Station { get; set; }
        public object Sessions { get; set; }
        public object EquipmentLng { get; set; }
        public object EquipmentLat { get; set; }

        // 构造函数
        protected Equipment() { }

        public Equipment(
            string equipmentId,
            Guid stationId,
            string equipmentName,
            EquipmentType equipmentType,
            decimal power,
            string source = "system",
            string createdBy = "system",
            string equipmentModel = null,
            string manufacturerId = null,
            string manufacturerName = null,
            DateTime productionDate = default,
            int voltage = 0,
            int current = 0,
            PowerConfiguration powerConfig = null,
            string communicationProtocol = null,
            string firmwareVersion = null)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
                throw new ArgumentException("Equipment ID cannot be empty", nameof(equipmentId));

            EquipmentId = equipmentId;
            StationId = stationId;
            EquipmentName = equipmentName ?? throw new ArgumentNullException(nameof(equipmentName));
            EquipmentType = equipmentType;

            // 设置默认功率值
            SetPower(power);

            // 设置默认状态
            Status = EquipmentStatus.Idle;
            StatusUpdateTime = DateTime.UtcNow;

            // 设置默认配置
            PowerConfig = new PowerConfiguration(power * 0.8m, power, power * 1.2m); // 默认配置
            SupportsDynamicPower = false;

            Source = source;
            EquipmentModel = equipmentModel;
            ManufacturerId = manufacturerId;
            ManufacturerName = manufacturerName;
            ProductionDate = productionDate;
            Voltage = voltage;
            Current = current;
            PowerConfig = powerConfig;
            CommunicationProtocol = communicationProtocol;
            FirmwareVersion = firmwareVersion;
            CreatedBy = createdBy;
            ProductionDate = DateTime.UtcNow;

            AddStatusHistory(EquipmentStatus.Idle, "Equipment created");
        }

        // 业务方法
        public void SetPower(decimal power)
        {
            if (power <= 0)
                throw new ArgumentException("Power must be greater than 0", nameof(power));

            Power = power;

            // 根据设备类型设置默认的功率范围
            switch (EquipmentType)
            {
                case EquipmentType.TwoWheeler:
                    MinPower = 3.0m;     // 二轮车最低3kW
                    MaxPower = 7.0m;     // 二轮车最高7kW
                    break;
                case EquipmentType.FourWheeler:
                    MinPower = 7.0m;     // 四轮车最低7kW
                    MaxPower = 22.0m;    // 四轮车最高22kW（交流慢充）
                    break;
                case EquipmentType.FastCharger:
                    MinPower = 30.0m;    // 快充桩最低30kW
                    MaxPower = 120.0m;   // 快充桩最高120kW
                    break;
                default:
                    MinPower = power * 0.5m;
                    MaxPower = power * 1.5m;
                    break;
            }

            // 确保当前功率在范围内
            if (Power < MinPower) Power = MinPower;
            if (Power > MaxPower) Power = MaxPower;

            Update();

            AddDomainEvent(new EquipmentPowerUpdatedEvent(Id, EquipmentId, power));
        }

        public void UpdateStatus(EquipmentStatus newStatus, string reason = null)
        {
            if (Status != newStatus)
            {
                var oldStatus = Status;
                Status = newStatus;
                StatusUpdateTime = DateTime.UtcNow;
                StatusReason = reason;

                Update();

                AddStatusHistory(newStatus, reason);
                AddDomainEvent(new EquipmentStatusChangedEvent(Id, EquipmentId, oldStatus, newStatus, reason));

                // 如果设备故障或离线，将所有连接器设置为离线
                if (newStatus == EquipmentStatus.Fault || newStatus == EquipmentStatus.Offline)
                {
                    foreach (var connector in _connectors)
                    {
                        connector.UpdateStatus(ConnectorStatus.Offline, $"Equipment {newStatus}");
                    }
                }
            }
        }

        public void AddConnector(Connector connector)
        {
            if (connector == null)
                throw new ArgumentNullException(nameof(connector));

            if (_connectors.Any(c => c.ConnectorId == connector.ConnectorId))
                throw new InvalidOperationException($"Connector with ID {connector.ConnectorId} already exists in equipment {EquipmentId}");

            _connectors.Add(connector);
            Update();

            AddDomainEvent(new ConnectorAddedEvent(Id, EquipmentId, connector.Id, connector.ConnectorId));
        }

        public void RemoveConnector(string connectorId)
        {
            var connector = _connectors.FirstOrDefault(c => c.ConnectorId == connectorId);
            if (connector != null)
            {
                _connectors.Remove(connector);
                Update();

                AddDomainEvent(new ConnectorRemovedEvent(Id, EquipmentId, connector.Id, connectorId));
            }
        }

        public Connector GetConnector(string connectorId)
        {
            return _connectors.FirstOrDefault(c => c.ConnectorId == connectorId);
        }

        public void UpdatePowerConfiguration(decimal minPower, decimal maxPower, bool supportsDynamic = false)
        {
            if (minPower <= 0)
                throw new ArgumentException("Minimum power must be greater than 0", nameof(minPower));

            if (maxPower <= minPower)
                throw new ArgumentException("Maximum power must be greater than minimum power", nameof(maxPower));

            PowerConfig = new PowerConfiguration(minPower, Power, maxPower);
            SupportsDynamicPower = supportsDynamic;

            Update();

            AddDomainEvent(new PowerConfigurationUpdatedEvent(Id, EquipmentId, minPower, maxPower, supportsDynamic));
        }

        public void UpdateTechnicalSpecs(
            string model,
            string manufacturerName,
            decimal voltage,
            decimal current,
            string protocol,
            string firmwareVersion)
        {
            EquipmentModel = model ?? EquipmentModel;
            ManufacturerName = manufacturerName ?? ManufacturerName;
            Voltage = voltage > 0 ? voltage : Voltage;
            Current = current > 0 ? current : Current;
            CommunicationProtocol = protocol ?? CommunicationProtocol;
            FirmwareVersion = firmwareVersion ?? FirmwareVersion;

            Update();
        }

        public void UpdateMaintenanceInfo(DateTime? nextMaintenanceDate, string contact)
        {
            LastMaintenanceDate = DateTime.UtcNow;
            NextMaintenanceDate = nextMaintenanceDate;
            MaintenanceContact = contact;

            Update();
        }

        public void AdjustPower(decimal newPower, string adjustedBy)
        {
            if (!SupportsDynamicPower)
                throw new InvalidOperationException("This equipment does not support dynamic power adjustment");

            if (newPower < PowerConfig.MinPower || newPower > PowerConfig.MaxPower)
                throw new ArgumentOutOfRangeException(nameof(newPower),
                    $"Power must be between {PowerConfig.MinPower} and {PowerConfig.MaxPower}");

            var oldPower = Power;
            Power = newPower;

            Update(adjustedBy);

            AddDomainEvent(new EquipmentPowerAdjustedEvent(Id, EquipmentId, oldPower, newPower, adjustedBy));

            // 如果正在充电，需要通知所有连接器功率变化
            if (Status == EquipmentStatus.Charging)
            {
                foreach (var connector in _connectors.Where(c => c.Status == ConnectorStatus.OccupiedCharging))
                {
                    connector.NotifyPowerChange(newPower);
                }
            }
        }

        public void UpdateStatistics(decimal electricity, TimeSpan chargingTime)
        {
            EquipmentElectricity += electricity;
            TotalSessions += 1;
            TotalChargingTime = TotalChargingTime.Add(chargingTime);

            Update();
        }

        private void AddStatusHistory(EquipmentStatus status, string reason)
        {
            var history = new EquipmentStatusHistory(Id, status, reason);
            _statusHistory.Add(history);
        }

        // 验证方法
        public bool CanStartCharging()
        {
            return Status == EquipmentStatus.Idle || Status == EquipmentStatus.Standby;
        }

        public bool IsOperational()
        {
            return Status == EquipmentStatus.Idle ||
                   Status == EquipmentStatus.Standby ||
                   Status == EquipmentStatus.Charging;
        }
    }

    /// <summary>
    /// 设备状态枚举
    /// </summary>
    public enum EquipmentStatus
    {
        Unknown = 0,        // 未知
        Idle = 1,           // 空闲
        Standby = 2,        // 待机
        Charging = 3,       // 充电中
        Fault = 4,          // 故障
        Offline = 5,        // 离线
        Maintenance = 6,    // 维护中
        Upgrading = 7       // 升级中
    }

    /// <summary>
    /// 设备状态历史记录
    /// </summary>
    public class EquipmentStatusHistory : BaseEntity
    {
        public Guid EquipmentId { get; private set; }
        public EquipmentStatus Status { get; private set; }
        public string Reason { get; private set; }
        public DateTime ChangeTime { get; private set; }

        public EquipmentStatusHistory(Guid equipmentId, EquipmentStatus status, string reason = null)
        {
            EquipmentId = equipmentId;
            Status = status;
            Reason = reason;
            ChangeTime = DateTime.UtcNow;
        }
    }
}