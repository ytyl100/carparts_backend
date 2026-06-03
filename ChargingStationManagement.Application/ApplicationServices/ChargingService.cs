using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.Interfaces;
using ChargingStationManagement.Domain.ValueObjects;
using ChargingStationManagement.Infrastructure.Persistence.Repositories;
using ChargingStationManagement.Services.DTOs;
using ChargingStationManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore; // 🔥 添加这个 using
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ChargingMode = ChargingStationManagement.Services.DTOs.ChargingMode;
using IApiThirdPartyIntegrationService = ChargingStationManagement.Services.Interfaces.IApiThirdPartyIntegrationService;
using IWalletService = ChargingStationManagement.Services.Interfaces.IWalletService;
using Transaction = ChargingStationManagement.Domain.Entities.Transaction;

namespace ChargingStationManagement.Services.ApplicationServices
{
    public class ChargingService : IApiChargingService
    {
        private readonly IRepository<Session> _sessionRepository;
        private readonly IRepository<Equipment> _equipmentRepository;
        private readonly IRepository<Connector> _connectorRepository;
        private readonly IRepository<Station> _stationRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly IApiThirdPartyIntegrationService _thirdPartyService;
        private readonly IWalletService _walletService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ChargingService> _logger;

        public ChargingService(
            IRepository<Session> sessionRepository,
            IRepository<Equipment> equipmentRepository,
            IRepository<Connector> connectorRepository,
            IRepository<Station> stationRepository,
            IRepository<User> userRepository,
            IRepository<Wallet> walletRepository,
            IRepository<Transaction> transactionRepository,
            IApiThirdPartyIntegrationService thirdPartyService,
            IWalletService walletService,
            ICacheService cacheService,
            ILogger<ChargingService> logger)
        {
            _sessionRepository = sessionRepository;
            _equipmentRepository = equipmentRepository;
            _connectorRepository = connectorRepository;
            _stationRepository = stationRepository;
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
            _thirdPartyService = thirdPartyService;
            _walletService = walletService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<StartSessionResultDto> StartChargingSessionAsync(string userId, string connectorId, ChargingMode mode)
        {
            try
            {
                // 🔥 修复 1: 使用 Query().FirstOrDefaultAsync()
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.UserId == userId);
                
                if (user == null)
                    throw new ArgumentException($"User {userId} not found");

                if (!user.CanStartCharging())
                    throw new InvalidOperationException("User cannot start charging");

                // 🔥 修复 2: 使用 Query().FirstOrDefaultAsync()
                var connector = await _connectorRepository.Query()
                    .FirstOrDefaultAsync(c => c.ConnectorId == connectorId);
                
                if (connector == null)
                    throw new ArgumentException($"Connector {connectorId} not found");

                if (!connector.CanStartCharging())
                    throw new InvalidOperationException($"Connector is not available. Status: {connector.Status}");
                
                var equipment = await _equipmentRepository.GetByIdAsync(connector.EquipmentId);
                var station = await _stationRepository.GetByIdAsync(equipment.StationId);

                // 获取第三方名称
                var thirdPartyName = station.Source;

                // 生成会话ID
                var sessionId = GenerateSessionId();
                var startChargeSeq = GenerateStartChargeSeq();

                // 创建会话实体
                var session = new Session(
                    sessionId,
                    user.Id,
                    connector.Id,
                    equipment.Id,
                    station.Id,
                    startChargeSeq,
                    (Domain.Enums.ChargingMode)mode,
                    "User");

                // 获取连接器所属的设备
                await _thirdPartyService.StopChargingAsync(thirdPartyName, session.StartChargeSeq);
               
                // 检查余额
                var hasBalance = await _walletService.CheckBalanceAsync(userId, 1); // 预检查1元

                // 构建 StartChargeRequest
                var startChargeRequest = new StartChargeRequest
                {
                    ConnectorId = connectorId,
                    UserId = userId,
                    // 根据 StartChargeRequest 的实际属性补充其它必需字段
                };
                
                // 调用第三方API启动充电
                await _thirdPartyService.StartChargingAsync(thirdPartyName, connectorId, userId);

                if (!hasBalance)
                    throw new InvalidOperationException("Insufficient balance");
                
                // 保存会话
                await _sessionRepository.AddAsync(session);

                // 更新连接器状态
                connector.StartSession(session.Id.ToString(), userId);
                await _connectorRepository.UpdateAsync(connector);

                // 返回结果
                return new StartSessionResultDto
                {
                    Success = true,
                    SessionId = sessionId,
                    StartChargeSeq = startChargeSeq,
                    Message = "Charging started successfully",
                    QRCode = GenerateQRCode(userId, connectorId),
                    StartTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting charging session for user {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateChargingSessionDataAsync(string sessionId, ChargingDataDto data)
        {
            try
            {
                // 🔥 修复 3: 使用 Query().FirstOrDefaultAsync()
                var session = await _sessionRepository.Query()
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId);
                
                if (session == null)
                    throw new ArgumentException($"Session {sessionId} not found");

                // 更新实时数据
                session.UpdateRealTimeData(
                    data.Voltage,
                    data.Current,
                    data.Power,
                    data.Energy,
                    data.BatteryLevel);

                await _sessionRepository.UpdateAsync(session);

                // 更新连接器实时数据
                var connector = await _connectorRepository.GetByIdAsync(session.ConnectorId);
                if (connector != null)
                {
                    connector.UpdateRealTimeData(data.Voltage, data.Current, data.Power, data.Energy);
                    await _connectorRepository.UpdateAsync(connector);
                }

                // 检查余额（每5分钟检查一次）
                if (DateTime.UtcNow.Minute % 5 == 0)
                {
                    await CheckAndHandleLowBalanceForSessionAsync(session);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating charging session data for {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<StopSessionResultDto> StopChargingSessionAsync(string sessionId, string stoppedBy, string reason)
        {
            try
            {
                // 🔥 修复 4: 使用 Query().FirstOrDefaultAsync()
                var session = await _sessionRepository.Query()
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId);
                
                if (session == null)
                    throw new ArgumentException($"Session {sessionId} not found");

                // 停止充电
                session.StopCharging(stoppedBy, reason);

                // 获取第三方名称
                var connector = await _connectorRepository.GetByIdAsync(session.ConnectorId);
                var equipment = await _equipmentRepository.GetByIdAsync(connector.EquipmentId);
                var station = await _stationRepository.GetByIdAsync(equipment.StationId);
                var thirdPartyName = station.Source;

                // 调用第三方API停止充电
                var stopResult = await _thirdPartyService.StopChargingAsync(thirdPartyName, session.StartChargeSeq);
                if (!stopResult.Success)
                    throw new Exception($"Failed to stop charging: {stopResult.Message}");

                // 完成充电会话
                var rates = new Rate(
                    session.AppliedRates.ElectricityRate,
                    session.AppliedRates.ServiceRate,
                    session.AppliedRates.ParkRate,
                    session.AppliedRates.TimeRate);

                session.CompleteCharging(
                    stopResult.TotalPower,
                    stopResult.TotalPower * session.AppliedRates.ElectricityRate,
                    stopResult.TotalPower * session.AppliedRates.ServiceRate,
                    0, // 停车费需要根据实际计算
                    rates);

                await _sessionRepository.UpdateAsync(session);

                // 更新连接器状态
                connector.EndSession();
                await _connectorRepository.UpdateAsync(connector);

                // 从钱包扣款
                var user = await _userRepository.GetByIdAsync(session.UserId);
                await _walletService.ConsumeWalletAsync(user.UserId, stopResult.TotalMoney, sessionId);

                // 更新用户统计
                user.UpdateStatistics(stopResult.TotalPower, stopResult.TotalMoney);
                await _userRepository.UpdateAsync(user);

                // 更新设备统计
                equipment.UpdateStatistics(stopResult.TotalPower, session.GetDuration() ?? TimeSpan.Zero);
                await _equipmentRepository.UpdateAsync(equipment);

                // 更新站点统计
                station.UpdateStatistics(stopResult.TotalPower, stopResult.TotalMoney);
                await _stationRepository.UpdateAsync(station);

                return new StopSessionResultDto
                {
                    Success = true,
                    SessionId = sessionId,
                    TotalEnergy = stopResult.TotalPower,
                    TotalAmount = stopResult.TotalMoney,
                    Duration = session.GetDuration() ?? TimeSpan.Zero,
                    EndTime = session.EndTime ?? DateTime.UtcNow,
                    Message = "Charging stopped successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping charging session for {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<SessionDto> GetSessionAsync(string sessionId)
        {
            try
            {
                // 🔥 修复 5: 使用 Query().FirstOrDefaultAsync()
                var session = await _sessionRepository.Query()
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId);
                
                if (session == null)
                    return null;

                var user = await _userRepository.GetByIdAsync(session.UserId);
                var connector = await _connectorRepository.GetByIdAsync(session.ConnectorId);
                var equipment = await _equipmentRepository.GetByIdAsync(connector.EquipmentId);
                var station = await _stationRepository.GetByIdAsync(equipment.StationId);

                return new SessionDto
                {
                    SessionId = session.SessionId,
                    UserId = user.UserId,
                    UserName = user.Name,
                    ConnectorId = connector.ConnectorId,
                    EquipmentId = equipment.EquipmentId.ToString(),
                    StationId = station.StationId,
                    StationName = station.StationName,
                    StartTime = session.StartTime,
                    EndTime = session.EndTime,
                    Duration = session.GetDuration(),
                    Status = (int)session.Status,
                    StatusText = GetChargeStatusText((int)session.Status),
                    OrderStatus = (int)session.OrderStatus,
                    OrderStatusText = GetOrderStatusText((int)session.OrderStatus),
                    TotalEnergy = session.TotalEnergy,
                    TotalAmount = session.TotalAmount,
                    IsPaid = session.IsPaid,
                    VehicleLicensePlate = session.VehicleLicensePlate,
                    StartBatteryLevel = session.StartBatteryLevel,
                    EndBatteryLevel = session.EndBatteryLevel,
                    Rates = new RateDto
                    {
                        ElectricityRate = session.AppliedRates.ElectricityRate,
                        ServiceRate = session.AppliedRates.ServiceRate,
                        ParkRate = session.AppliedRates.ParkRate,
                        TimeRate = session.AppliedRates.TimeRate
                    },
                    CurrentData = new ChargingDataDto
                    {
                        Voltage = session.CurrentVoltage,
                        Current = session.CurrentCurrent,
                        Power = session.CurrentPower,
                        Energy = session.CurrentEnergy,
                        BatteryLevel = 0, // 需要从实时数据获取
                        Timestamp = session.LastDataUpdate
                    },
                    StartedBy = session.StartedBy,
                    StoppedBy = session.StoppedBy,
                    StopReason = session.StopReason
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<List<ActiveSessionDto>> GetActiveSessionsAsync()
        {
            try
            {
                // 🔥 修复 6: 使用 FindAsync() 替代 GetAsync()
                var sessions = await _sessionRepository.FindAsync(s =>
                    s.Status == Domain.Enums.ChargeStatus.Charging ||
                    s.Status == Domain.Enums.ChargeStatus.Starting);

                var activeSessions = new List<ActiveSessionDto>();

                foreach (var session in sessions)
                {
                    var user = await _userRepository.GetByIdAsync(session.UserId);
                    var connector = await _connectorRepository.GetByIdAsync(session.ConnectorId);
                    var equipment = await _equipmentRepository.GetByIdAsync(connector.EquipmentId);
                    var station = await _stationRepository.GetByIdAsync(equipment.StationId);

                    activeSessions.Add(new ActiveSessionDto
                    {
                        SessionId = session.SessionId,
                        UserId = user.UserId,
                        UserName = user.Name,
                        ConnectorId = connector.ConnectorId,
                        EquipmentId = equipment.EquipmentId.ToString(),
                        StationId = station.StationId,
                        StationName = station.StationName,
                        StartTime = session.StartTime,
                        Status = (int)session.Status,
                        StatusText = GetChargeStatusText((int)session.Status),
                        TotalEnergy = session.TotalEnergy,
                        TotalAmount = session.TotalAmount,
                        VehicleLicensePlate = session.VehicleLicensePlate,
                        StartBatteryLevel = session.StartBatteryLevel,
                        EndBatteryLevel = session.EndBatteryLevel,
                        CurrentPower = session.CurrentPower,
                        CurrentEnergy = session.CurrentEnergy,
                        LastDataUpdate = session.LastDataUpdate,
                        EstimatedRemainingTime = CalculateEstimatedRemainingTime(session),
                        EstimatedRemainingCost = CalculateEstimatedRemainingCost(session)
                    });
                }

                return activeSessions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions");
                throw;
            }
        }

        public async Task CheckAndHandleLowBalanceSessionsAsync()
        {
            try
            {
                var activeSessions = await GetActiveSessionsAsync();

                foreach (var session in activeSessions)
                {
                    await CheckAndHandleLowBalanceForSessionAsync(session);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking low balance sessions");
            }
        }

        public async Task CheckScheduledEndSessionsAsync()
        {
            try
            {
                var activeSessions = await GetActiveSessionsAsync();

                foreach (var session in activeSessions)
                {
                    if (session.IsScheduledToEnd())
                    {
                        await StopChargingSessionAsync(session.SessionId, "System", "Scheduled end time reached");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking scheduled end sessions");
            }
        }

        public async Task<OrderDto> CompleteChargingOrderAsync(string sessionId)
        {
            try
            {
                var session = await GetSessionAsync(sessionId);
                if (session == null)
                    throw new ArgumentException($"Session {sessionId} not found");

                if (session.OrderStatus != (int)OrderStatus.Completed)
                    throw new InvalidOperationException("Session is not completed");

                // 创建订单
                var order = new OrderDto
                {
                    OrderId = GenerateOrderId(),
                    SessionId = sessionId,
                    UserId = session.UserId,
                    UserName = session.UserName,
                    StationId = session.StationId,
                    StationName = session.StationName,
                    ConnectorId = session.ConnectorId,
                    StartTime = session.StartTime,
                    EndTime = session.EndTime ?? DateTime.UtcNow,
                    Duration = session.Duration ?? TimeSpan.Zero,
                    TotalEnergy = session.TotalEnergy,
                    TotalAmount = session.TotalAmount,
                    ElectricityCost = session.TotalAmount * 0.8m, // 假设80%是电费
                    ServiceCost = session.TotalAmount * 0.2m, // 假设20%是服务费
                    ParkCost = 0,
                    Rates = session.Rates,
                    PaymentStatus = session.IsPaid ? "Paid" : "Unpaid",
                    CreatedAt = DateTime.UtcNow
                };

                // 如果未支付，尝试支付
                if (!session.IsPaid)
                {
                    var paymentSuccess = await ProcessPaymentAsync(order.OrderId, "Wallet");
                    order.PaymentStatus = paymentSuccess ? "Paid" : "Payment Failed";
                    order.PaidAt = paymentSuccess ? DateTime.UtcNow : (DateTime?)null;
                }

                // 保存订单到数据库
                // TODO: 实现订单仓储

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing charging order for {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<bool> ProcessPaymentAsync(string orderId, string paymentMethod)
        {
            try
            {
                // TODO: 实现支付处理逻辑
                // 1. 获取订单信息
                // 2. 调用支付网关
                // 3. 更新订单状态
                // 4. 更新会话状态

                await Task.Delay(100); // 模拟支付处理
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for order {OrderId}", orderId);
                return false;
            }
        }

        private async Task CheckAndHandleLowBalanceForSessionAsync(SessionDto session)
        {
            try
            {
                var balance = await _walletService.GetAvailableBalanceAsync(session.UserId);

                // 计算预估剩余费用
                var estimatedRemainingCost = CalculateEstimatedRemainingCost(session);

                if (balance < estimatedRemainingCost * 1.2m) // 余额小于预估费用的120%
                {
                    // 发送低余额通知
                    // TODO: 实现通知服务

                    // 如果余额严重不足，停止充电
                    if (balance < estimatedRemainingCost * 0.5m)
                    {
                        await StopChargingSessionAsync(session.SessionId, "System", "Insufficient balance");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking balance for session {SessionId}", session.SessionId);
            }
        }

        private async Task CheckAndHandleLowBalanceForSessionAsync(Domain.Entities.Session session)
        {
            // 重载方法，处理领域实体
            try
            {
                var user = await _userRepository.GetByIdAsync(session.UserId);
                var balance = await _walletService.GetAvailableBalanceAsync(user.UserId);

                // 计算预估剩余费用
                var estimatedRemainingCost = CalculateEstimatedRemainingCost(session);

                if (balance < estimatedRemainingCost * 1.2m)
                {
                    if (balance < estimatedRemainingCost * 0.5m)
                    {
                        await StopChargingSessionAsync(session.SessionId, "System", "Insufficient balance");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking balance for session {SessionId}", session.SessionId);
            }
        }

        private decimal CalculateEstimatedRemainingCost(SessionDto session)
        {
            // 简单估算：基于当前功率和费率
            if (session.CurrentPower <= 0 || session.Rates == null)
                return 0;

            // 假设剩余充电时间为30分钟
            var remainingHours = 0.5m;
            var estimatedEnergy = session.CurrentPower * remainingHours;

            return estimatedEnergy * session.Rates.ElectricityRate +
                   estimatedEnergy * session.Rates.ServiceRate;
        }

        private decimal CalculateEstimatedRemainingCost(Domain.Entities.Session session)
        {
            if (session.CurrentPower <= 0)
                return 0;

            var remainingHours = 0.5m;
            var estimatedEnergy = session.CurrentPower * remainingHours;

            return estimatedEnergy * session.AppliedRates.ElectricityRate +
                   estimatedEnergy * session.AppliedRates.ServiceRate;
        }

        private decimal CalculateEstimatedRemainingTime(SessionDto session)
        {
            if (session.CurrentPower <= 0)
                return 0;

            // 假设车辆需要再充10kWh
            var remainingEnergy = 10m;
            return remainingEnergy / session.CurrentPower;
        }

        private decimal CalculateEstimatedRemainingTime(Domain.Entities.Session session)
        {
            if (session.CurrentPower <= 0)
                return 0;

            var remainingEnergy = 10m;
            return remainingEnergy / session.CurrentPower;
        }

        private string GenerateSessionId()
        {
            return $"SESS{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }

        private string GenerateStartChargeSeq()
        {
            return $"SCS{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(10000, 99999)}";
        }

        private string GenerateOrderId()
        {
            return $"ORD{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(10000, 99999)}";
        }

        private string GenerateQRCode(string userId, string connectorId)
        {
            return $"CHG:{userId}:{connectorId}:{DateTime.Now.Ticks}";
        }

        private string GetChargeStatusText(int status) => status switch
        {
            1 => "启动中",
            2 => "充电中",
            3 => "停止中",
            4 => "已结束",
            5 => "未知",
            _ => "未知状态"
        };

        private string GetOrderStatusText(int status) => status switch
        {
            1 => "已创建",
            2 => "充电中",
            3 => "已完成",
            4 => "已取消",
            5 => "已退款",
            _ => "未知状态"
        };
    }
}