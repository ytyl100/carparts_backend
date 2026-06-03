// ChargingStationManagement.Domain/Entities/Session.cs
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Events;
using ChargingStationManagement.Domain.ValueObjects;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChargingStationManagement.Domain.Entities
{
    /// <summary>
    /// 充电会话实体（聚合根）
    /// </summary>
    public class Session : AggregateRoot
    {
        // 会话标识
        public string SessionId { get; private set; }            // 会话唯一ID
        public string StartChargeSeq { get; private set; }       // 充电流水号（第三方）

        // 参与方信息
        public Guid UserId { get; private set; }                 // 用户ID
        public Guid ConnectorId { get; private set; }            // 连接器ID
        public Guid EquipmentId { get; private set; }            // 设备ID
        public Guid StationId { get; private set; }              // 充电站ID

        // 时间信息
        public DateTime StartTime { get; private set; }          // 开始时间
        public DateTime? EndTime { get; private set; }           // 结束时间
        public DateTime? ScheduledEndTime { get; private set; }  // 计划结束时间（用于预约）

        // 状态信息
        public ChargeStatus Status { get; private set; }         // 充电状态
        public OrderStatus OrderStatus { get; private set; }     // 订单状态
        public ChargingMode ChargingMode { get; private set; }   // 充电模式

        // 计量信息
        public decimal StartMeterValue { get; private set; }     // 起始电表值（kWh）
        public decimal EndMeterValue { get; private set; }       // 结束电表值（kWh）
        public decimal TotalEnergy { get; private set; }         // 总电量（kWh）
        public decimal PeakPower { get; private set; }           // 峰值功率（kW）
        public decimal AveragePower { get; private set; }        // 平均功率（kW）

        // 费用信息
        public Rate AppliedRates { get; private set; }           // 应用费率值对象
        public decimal ElectricityFee { get; private set; }      // 电费
        public decimal ServiceFee { get; private set; }          // 服务费
        public decimal ParkFee { get; private set; }             // 停车费
        public decimal TotalAmount { get; private set; }         // 总金额
        public bool IsPaid { get; private set; }                 // 是否已支付
        public DateTime? PaymentTime { get; private set; }       // 支付时间
        public string PaymentTransactionId { get; private set; } // 支付交易ID

        // 车辆信息
        public Guid? VehicleId { get; private set; }             // 车辆ID
        public string VehicleLicensePlate { get; private set; }  // 车牌号
        public decimal VehicleBatteryCapacity { get; private set; } // 车辆电池容量（kWh）
        public decimal StartBatteryLevel { get; private set; }   // 起始电量百分比
        public decimal EndBatteryLevel { get; private set; }     // 结束电量百分比

        // 实时数据（最后记录的值）
        public decimal CurrentVoltage { get; private set; }      // 当前电压（V）
        public decimal CurrentCurrent { get; private set; }      // 当前电流（A）
        public decimal CurrentPower { get; private set; }        // 当前功率（kW）
        public decimal CurrentEnergy { get; private set; }       // 当前电量（kWh）
        public DateTime LastDataUpdate { get; private set; }     // 最后数据更新时间

        // 操作信息
        public string StartedBy { get; private set; }            // 启动方式（User/Device/System）
        public string StoppedBy { get; private set; }            // 停止方式（User/Device/System/Auto）
        public string StopReason { get; private set; }           // 停止原因

        // 扩展信息
        public string QRCode { get; private set; }               // 启动二维码
        public string ReservationId { get; private set; }        // 预约ID（如果有）
        public string Notes { get; private set; }                // 备注
        public object User { get; set; }
        public object Connector { get; set; }
        public object Station { get; set; }

        // 构造函数
        protected Session() { }

        public Session(
            string sessionId,
            Guid userId,
            Guid connectorId,
            Guid equipmentId,
            Guid stationId,
            string startChargeSeq,
            ChargingMode mode = ChargingMode.EnergyBased,
            string startedBy = "User",
            string createdBy = "system")
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));

            if (string.IsNullOrWhiteSpace(startChargeSeq))
                throw new ArgumentException("Start charge sequence cannot be empty", nameof(startChargeSeq));

            SessionId = sessionId;
            StartChargeSeq = startChargeSeq;
            UserId = userId;
            ConnectorId = connectorId;
            EquipmentId = equipmentId;
            StationId = stationId;
            ChargingMode = mode;
            StartedBy = startedBy;

            StartTime = DateTime.UtcNow;
            Status = ChargeStatus.Starting;
            OrderStatus = OrderStatus.Created;

            // 默认费率
            AppliedRates = new Rate(1.5m, 0.2m, 5m, 0.5m);

            CreatedBy = createdBy;

            AddDomainEvent(new SessionCreatedEvent(Id, sessionId, userId, connectorId, stationId));
        }

        // 业务方法
        public void StartCharging(decimal startMeterValue, decimal batteryLevel = 0)
        {
            if (Status != ChargeStatus.Starting)
                throw new InvalidOperationException($"Cannot start charging from status {Status}");

            StartMeterValue = startMeterValue;
            StartBatteryLevel = batteryLevel;
            Status = ChargeStatus.Charging;
            OrderStatus = OrderStatus.Charging;

            Update();

            AddDomainEvent(new ChargingStartedEvent(Id, SessionId, ConnectorId, UserId));
        }

        public void UpdateRealTimeData(
            decimal voltage,
            decimal current,
            decimal power,
            decimal energy,
            decimal batteryLevel = 0)
        {
            CurrentVoltage = voltage;
            CurrentCurrent = current;
            CurrentPower = power;
            CurrentEnergy = energy;
            LastDataUpdate = DateTime.UtcNow;

            // 更新峰值功率
            if (power > PeakPower)
                PeakPower = power;

            // 计算平均功率
            if (StartTime != default)
            {
                var duration = (DateTime.UtcNow - StartTime).TotalHours;
                if (duration > 0)
                {
                    AveragePower = energy / (decimal)duration;
                }
            }

            // 不触发领域事件，因为这是高频更新
        }

        public void StopCharging(
            string stoppedBy,
            string reason = null,
            decimal? endMeterValue = null,
            decimal? batteryLevel = null)
        {
            if (Status != ChargeStatus.Charging && Status != ChargeStatus.Starting)
                throw new InvalidOperationException($"Cannot stop charging from status {Status}");

            StoppedBy = stoppedBy;
            StopReason = reason;
            Status = ChargeStatus.Stopping;

            if (endMeterValue.HasValue)
                EndMeterValue = endMeterValue.Value;

            if (batteryLevel.HasValue)
                EndBatteryLevel = batteryLevel.Value;

            Update();

            AddDomainEvent(new ChargingStoppingEvent(Id, SessionId, stoppedBy, reason));
        }

        public void CompleteCharging(
            decimal totalEnergy,
            decimal electricityFee,
            decimal serviceFee,
            decimal parkFee,
            Rate rates = null)
        {
            if (Status != ChargeStatus.Stopping)
                throw new InvalidOperationException($"Cannot complete charging from status {Status}");

            EndTime = DateTime.UtcNow;
            Status = ChargeStatus.Finished;
            OrderStatus = OrderStatus.Completed;

            TotalEnergy = totalEnergy;
            ElectricityFee = electricityFee;
            ServiceFee = serviceFee;
            ParkFee = parkFee;
            TotalAmount = electricityFee + serviceFee + parkFee;

            if (rates != null)
                AppliedRates = rates;

            // 计算结束电表值
            EndMeterValue = StartMeterValue + totalEnergy;

            // 如果没有设置结束电量，则根据充电量估算
            if (EndBatteryLevel <= 0 && VehicleBatteryCapacity > 0)
            {
                EndBatteryLevel = StartBatteryLevel + (totalEnergy / VehicleBatteryCapacity * 100);
                if (EndBatteryLevel > 100) EndBatteryLevel = 100;
            }

            Update();

            AddDomainEvent(new ChargingCompletedEvent(
                Id, SessionId, totalEnergy, TotalAmount, EndTime.Value));
        }

        public void Cancel(string cancelledBy, string reason = null)
        {
            if (Status == ChargeStatus.Finished || OrderStatus == OrderStatus.Completed)
                throw new InvalidOperationException("Cannot cancel a completed session");

            EndTime = DateTime.UtcNow;
            Status = ChargeStatus.Finished;
            OrderStatus = OrderStatus.Cancelled;
            StoppedBy = cancelledBy;
            StopReason = reason ?? "Cancelled by user";

            Update();

            AddDomainEvent(new SessionCancelledEvent(Id, SessionId, cancelledBy, reason));
        }

        public void MarkAsPaid(string paymentTransactionId, DateTime paymentTime)
        {
            if (IsPaid)
                throw new InvalidOperationException("Session is already paid");

            IsPaid = true;
            PaymentTime = paymentTime;
            PaymentTransactionId = paymentTransactionId;

            Update();

            AddDomainEvent(new SessionPaidEvent(Id, SessionId, paymentTransactionId, TotalAmount));
        }

        public void SetVehicleInfo(
            Guid? vehicleId,
            string licensePlate,
            decimal batteryCapacity,
            decimal startBatteryLevel)
        {
            VehicleId = vehicleId;
            VehicleLicensePlate = licensePlate;
            VehicleBatteryCapacity = batteryCapacity;
            StartBatteryLevel = startBatteryLevel;

            Update();
        }

        public void SetRates(Rate rates)
        {
            AppliedRates = rates ?? throw new ArgumentNullException(nameof(rates));
            Update();
        }

        public void SetScheduledEndTime(DateTime scheduledTime)
        {
            if (scheduledTime <= StartTime)
                throw new ArgumentException("Scheduled end time must be after start time");

            ScheduledEndTime = scheduledTime;
            Update();
        }

        // 计算方法和属性
        public TimeSpan? GetDuration()
        {
            if (StartTime == default) return null;

            var endTime = EndTime ?? DateTime.UtcNow;
            return endTime - StartTime;
        }

        public decimal? GetEnergyPerMinute()
        {
            var duration = GetDuration();
            if (!duration.HasValue || duration.Value.TotalMinutes <= 0) return null;

            return TotalEnergy / (decimal)duration.Value.TotalMinutes;
        }

        public decimal? GetCostPerMinute()
        {
            var duration = GetDuration();
            if (!duration.HasValue || duration.Value.TotalMinutes <= 0) return null;

            return TotalAmount / (decimal)duration.Value.TotalMinutes;
        }

        public bool IsActive()
        {
            return Status == ChargeStatus.Charging || Status == ChargeStatus.Starting;
        }

        public bool IsScheduledToEnd()
        {
            return ScheduledEndTime.HasValue &&
                   DateTime.UtcNow >= ScheduledEndTime.Value &&
                   Status == ChargeStatus.Charging;
        }

        // 验证方法
        public bool CanBeStopped()
        {
            return Status == ChargeStatus.Charging || Status == ChargeStatus.Starting;
        }

        public bool RequiresPayment()
        {
            return !IsPaid && OrderStatus == OrderStatus.Completed && TotalAmount > 0;
        }
    }
}