// ChargingStationManagement.Domain/Entities/Connector.cs
using System;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Events;
using ChargingStationManagement.Domain.ValueObjects;

namespace ChargingStationManagement.Domain.Entities
{
    /// <summary>
    /// 充电连接器实体（聚合根）
    /// </summary>
    public class Connector : AggregateRoot
    {
        // 标识信息
        public string ConnectorId { get; private set; }          // 连接器ID（在设备内唯一）
        public Guid EquipmentId { get; private set; }            // 所属设备ID
        public string ConnectorName { get; private set; }        // 连接器名称

        // 技术规格
        public ConnectorStandard Standard { get; private set; }  // 接口标准
        public decimal VoltageUpperLimits { get; private set; }  // 电压上限（V）
        public decimal VoltageLowerLimits { get; private set; }  // 电压下限（V）
        public decimal Current { get; private set; }             // 额定电流（A）
        public decimal Power { get; private set; }               // 额定功率（kW）
        public string ParkNo { get; private set; }               // 停车位编号

        // 状态信息
        public ConnectorStatus Status { get; private set; }      // 连接器状态
        public ParkStatus ParkStatus { get; private set; }       // 停车位状态
        public LockStatus LockStatus { get; private set; }       // 地锁状态
        public DateTime StatusUpdateTime { get; private set; }   // 状态更新时间
        public string StatusReason { get; private set; }         // 状态变更原因

        // 会话信息
        public Guid? CurrentSessionId { get; private set; }      // 当前充电会话ID
        public string CurrentUserId { get; private set; }        // 当前用户ID
        public DateTime? SessionStartTime { get; private set; }  // 会话开始时间

        // 统计信息
        public int TotalSessions { get; private set; }           // 总充电次数
        public decimal ConnectorElectricity { get; private set; } // 累计电量（kWh）
        public TimeSpan TotalChargingTime { get; private set; }  // 总充电时长
        public decimal TotalRevenue { get; private set; }        // 累计收入

        // 实时数据
        public decimal CurrentVoltage { get; private set; }      // 当前电压（V）
        public decimal CurrentCurrent { get; private set; }      // 当前电流（A）
        public decimal CurrentPower { get; private set; }        // 当前功率（kW）
        public decimal CurrentEnergy { get; private set; }       // 当前电量（kWh）
        public DateTime LastDataUpdate { get; private set; }     // 最后数据更新时间

        // 源信息
        public string Source { get; private set; }               // 数据来源
        public object Equipment { get; set; }
        public object Sessions { get; set; }
        public Guid StationId { get; set; }

        // 构造函数
        protected Connector() { }

        public Connector(
            string connectorId,
            Guid equipmentId,
            ConnectorStandard standard,
            decimal power,
            string connectorName = null,
            string source = "system",
            string createdBy = "system",
            int voltageUpperLimits = 0,
            int voltageLowerLimits = 0,
            int current = 0,
            string parkNo = null)
        {
            if (string.IsNullOrWhiteSpace(connectorId))
                throw new ArgumentException("Connector ID cannot be empty", nameof(connectorId));

            ConnectorId = connectorId;
            EquipmentId = equipmentId;
            Standard = standard;
            Power = power > 0 ? power : throw new ArgumentException("Power must be greater than 0", nameof(power));
            ConnectorName = connectorName ?? $"Connector-{connectorId}";

            // 设置默认电压和电流
            SetDefaultVoltageCurrent(standard, power);

            // 设置默认状态
            Status = ConnectorStatus.Idle;
            ParkStatus = ParkStatus.Unknown;
            LockStatus = LockStatus.Unknown;
            StatusUpdateTime = DateTime.UtcNow;

            Source = source;
            VoltageUpperLimits = voltageUpperLimits;
            VoltageLowerLimits = voltageLowerLimits;
            Current = current;
            ParkNo = parkNo;
            CreatedBy = createdBy;

            AddDomainEvent(new ConnectorCreatedEvent(Id, equipmentId, connectorId));
        }

        // 业务方法
        public void UpdateStatus(ConnectorStatus newStatus, string reason = null)
        {
            if (Status != newStatus)
            {
                var oldStatus = Status;
                Status = newStatus;
                StatusUpdateTime = DateTime.UtcNow;
                StatusReason = reason;

                Update();

                AddDomainEvent(new ConnectorStatusChangedEvent(Id, ConnectorId, oldStatus, newStatus, reason));
            }
        }

        public void UpdateParkStatus(ParkStatus newStatus)
        {
            if (ParkStatus != newStatus)
            {
                var oldStatus = ParkStatus;
                ParkStatus = newStatus;
                StatusUpdateTime = DateTime.UtcNow;

                Update();

                AddDomainEvent(new ParkStatusChangedEvent(Id, ConnectorId, oldStatus, newStatus));
            }
        }

        public void UpdateLockStatus(LockStatus newStatus)
        {
            if (LockStatus != newStatus)
            {
                var oldStatus = LockStatus;
                LockStatus = newStatus;
                StatusUpdateTime = DateTime.UtcNow;

                Update();

                AddDomainEvent(new LockStatusChangedEvent(Id, ConnectorId, oldStatus, newStatus));
            }
        }

        public void StartSession(Guid sessionId, string userId)
        {
            if (Status != ConnectorStatus.Idle)
                throw new InvalidOperationException($"Connector is not idle. Current status: {Status}");

            CurrentSessionId = sessionId;
            CurrentUserId = userId;
            SessionStartTime = DateTime.UtcNow;
            UpdateStatus(ConnectorStatus.OccupiedCharging, "Session started");

            AddDomainEvent(new SessionStartedEvent(Id, ConnectorId, sessionId, userId));
        }

        public void EndSession()
        {
            if (Status != ConnectorStatus.OccupiedCharging)
                throw new InvalidOperationException($"Connector is not charging. Current status: {Status}");

            var sessionId = CurrentSessionId;
            var userId = CurrentUserId;

            CurrentSessionId = null;
            CurrentUserId = null;
            SessionStartTime = null;
            UpdateStatus(ConnectorStatus.Idle, "Session ended");

            AddDomainEvent(new SessionEndedEvent(Id, ConnectorId, sessionId.Value, userId));
        }

        public void UpdateRealTimeData(decimal voltage, decimal current, decimal power, decimal energy)
        {
            CurrentVoltage = voltage;
            CurrentCurrent = current;
            CurrentPower = power;
            CurrentEnergy = energy;
            LastDataUpdate = DateTime.UtcNow;

            // 不触发领域事件，因为这是高频更新
        }

        public void UpdateStatistics(decimal electricity, TimeSpan chargingTime, decimal revenue)
        {
            TotalSessions += 1;
            ConnectorElectricity += electricity;
            TotalChargingTime = TotalChargingTime.Add(chargingTime);
            TotalRevenue += revenue;

            Update();
        }

        public void NotifyPowerChange(decimal newPower)
        {
            // 如果当前正在充电，并且功率发生了变化
            if (Status == ConnectorStatus.OccupiedCharging && Math.Abs(Power - newPower) > 0.1m)
            {
                Power = newPower;
                Update();

                AddDomainEvent(new ConnectorPowerChangedEvent(Id, ConnectorId, newPower));
            }
        }

        public void SetTechnicalSpecs(
            decimal voltageUpper,
            decimal voltageLower,
            decimal current,
            string parkNo = null)
        {
            if (voltageUpper <= voltageLower)
                throw new ArgumentException("Voltage upper limit must be greater than lower limit");

            VoltageUpperLimits = voltageUpper;
            VoltageLowerLimits = voltageLower;
            Current = current;
            ParkNo = parkNo;

            Update();
        }

        private void SetDefaultVoltageCurrent(ConnectorStandard standard, decimal power)
        {
            switch (standard)
            {
                case ConnectorStandard.GB_T:
                case ConnectorStandard.CCS:
                case ConnectorStandard.CHAdeMO:
                    // 直流快充
                    VoltageUpperLimits = 1000;
                    VoltageLowerLimits = 200;
                    Current = power * 1000 / 500; // 假设平均电压500V
                    break;
                case ConnectorStandard.Tesla:
                    // 特斯拉专用
                    VoltageUpperLimits = 480;
                    VoltageLowerLimits = 200;
                    Current = power * 1000 / 400; // 假设平均电压400V
                    break;
                case ConnectorStandard.AC:
                default:
                    // 交流慢充
                    VoltageUpperLimits = 250;
                    VoltageLowerLimits = 200;
                    Current = power * 1000 / 220; // 220V交流
                    break;
            }
        }

        // 验证方法
        public bool CanStartCharging()
        {
            return Status == ConnectorStatus.Idle &&
                   ParkStatus != ParkStatus.Occupied &&
                   LockStatus != LockStatus.Locked;
        }

        public bool IsCharging()
        {
            return Status == ConnectorStatus.OccupiedCharging;
        }

        public TimeSpan? GetCurrentSessionDuration()
        {
            if (SessionStartTime.HasValue)
            {
                return DateTime.UtcNow - SessionStartTime.Value;
            }
            return null;
        }
    }
}