// ChargingStationManagement.API/Controllers/ChargingController.cs
using ChargingStationManagement.Services.DTOs;
using ChargingStationManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using static ChargingStationManagement.API.Controllers.StationsController;

namespace ChargingStationManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 需要认证
    public class ChargingController : ControllerBase
    {
        private readonly IApiChargingService _chargingService;
        private readonly ILogger<ChargingController> _logger;

        public ChargingController(
            IApiChargingService chargingService,
            ILogger<ChargingController> logger)
        {
            _chargingService = chargingService;
            _logger = logger;
        }

        /// <summary>
        /// 启动充电会话
        /// </summary>
        [HttpPost("start")]
        [ProducesResponseType(typeof(StartSessionResultDto), 200)]
        public async Task<IActionResult> StartCharging([FromBody] StartChargingRequest request)
        {
            try
            {
                // 从Token获取用户ID
                var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _chargingService.StartChargingSessionAsync(
                    userId,
                    request.ConnectorId,
                    request.ChargingMode);

                if (!result.Success)
                    return BadRequest(new ApiResponse<StartSessionResultDto>
                    {
                        Success = false,
                        Message = result.Message
                    });

                return Ok(new ApiResponse<StartSessionResultDto>
                {
                    Success = true,
                    Data = result,
                    Message = "Charging started successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting charging for user");
                return BadRequest(new ApiResponse<StartSessionResultDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 更新充电数据（设备端调用）
        /// </summary>
        [HttpPost("sessions/{sessionId}/data")]
        [AllowAnonymous] // 允许设备端调用
        public async Task<IActionResult> UpdateChargingData(
            string sessionId,
            [FromBody] UpdateChargingDataRequest request,
            [FromHeader(Name = "X-Device-Key")] string deviceKey)  // 添加设备密钥
        {
            // 1. 验证输入
            if (string.IsNullOrWhiteSpace(sessionId))
                return BadRequest("Session ID is required");
            
            if (request?.Data == null)
                return BadRequest("Charging data is required");
            
            // 2. 验证设备
            //if (!await _deviceAuthService.ValidateDeviceKeyAsync(deviceKey))
            //    return Unauthorized("Invalid device credentials");
            
            try
            {
                await _chargingService.UpdateChargingSessionDataAsync(sessionId, request.Data);
                
                // 记录成功操作
                _logger.LogInformation(
                    "Charging data updated for session {SessionId} by device", 
                    sessionId);
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Charging data updated"
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating charging data for session {SessionId}", sessionId);
                return StatusCode(500, new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// 停止充电会话
        /// </summary>
        [HttpPost("sessions/{sessionId}/stop")]
        [ProducesResponseType(typeof(StopSessionResultDto), 200)]
        public async Task<IActionResult> StopCharging(
            string sessionId,
            [FromBody] StopChargingRequest request)
        {
            try
            {
                var result = await _chargingService.StopChargingSessionAsync(
                    sessionId,
                    request.StoppedBy,
                    request.Reason);

                if (!result.Success)
                    return BadRequest(new ApiResponse<StopSessionResultDto>
                    {
                        Success = false,
                        Message = result.Message
                    });

                return Ok(new ApiResponse<StopSessionResultDto>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping charging for session {SessionId}", sessionId);
                return BadRequest(new ApiResponse<StopSessionResultDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 获取充电会话详情
        /// </summary>
        [HttpGet("sessions/{sessionId}")]
        [ProducesResponseType(typeof(SessionDto), 200)]
        public async Task<IActionResult> GetSession(string sessionId)
        {
            try
            {
                var session = await _chargingService.GetSessionAsync(sessionId);

                if (session == null)
                    return NotFound(new ApiResponse<SessionDto>
                    {
                        Success = false,
                        Message = $"Session {sessionId} not found"
                    });

                return Ok(new ApiResponse<SessionDto>
                {
                    Success = true,
                    Data = session
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session {SessionId}", sessionId);
                return BadRequest(new ApiResponse<SessionDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 获取活跃充电会话
        /// </summary>
        [HttpGet("sessions/active")]
        [Authorize(Roles = "Admin,Operator")]
        [ProducesResponseType(typeof(List<ActiveSessionDto>), 200)]
        public async Task<IActionResult> GetActiveSessions()
        {
            try
            {
                var sessions = await _chargingService.GetActiveSessionsAsync();
                return Ok(new ApiResponse<List<ActiveSessionDto>>
                {
                    Success = true,
                    Data = sessions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions");
                return BadRequest(new ApiResponse<List<ActiveSessionDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 获取用户充电历史
        /// </summary>
        [HttpGet("history")]
        [ProducesResponseType(typeof(List<SessionDto>), 200)]
        public async Task<IActionResult> GetChargingHistory(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // TODO: 实现分页查询用户充电历史
                //var history = await _chargingService.GetUserChargingHistoryAsync(userId, startDate, endDate, page, pageSize);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Not implemented yet"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting charging history");
                return BadRequest(new ApiResponse<List<SessionDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 完成充电订单
        /// </summary>
        [HttpPost("sessions/{sessionId}/complete-order")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        public async Task<IActionResult> CompleteChargingOrder(string sessionId)
        {
            try
            {
                var order = await _chargingService.CompleteChargingOrderAsync(sessionId);
                return Ok(new ApiResponse<OrderDto>
                {
                    Success = true,
                    Data = order
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing order for session {SessionId}", sessionId);
                return BadRequest(new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // 请求DTO
        public class StartChargingRequest
        {
            public string ConnectorId { get; set; }
            public ChargingMode ChargingMode { get; set; } = ChargingMode.EnergyBased;
            public string VehicleLicensePlate { get; set; }
            public decimal? VehicleBatteryCapacity { get; set; }
            public decimal? StartBatteryLevel { get; set; }
            public DateTime? ScheduledEndTime { get; set; }
        }

        public class UpdateChargingDataRequest
        {
            public ChargingDataDto Data { get; set; }
        }

        public class StopChargingRequest
        {
            public string StoppedBy { get; set; } = "User";
            public string Reason { get; set; }
            public decimal? EndMeterValue { get; set; }
            public decimal? EndBatteryLevel { get; set; }
        }
    }
}
