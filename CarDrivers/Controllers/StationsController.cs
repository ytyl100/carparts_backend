// ChargingStationManagement.API/Controllers/StationsController.cs
using ChargingStationManagement.Services.DTOs;
using ChargingStationManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingStationManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly IStationManagementService _stationService;
        private readonly ILogger<StationsController> _logger;

        public StationsController(
            IStationManagementService stationService,
            ILogger<StationsController> logger)
        {
            _stationService = stationService;
            _logger = logger;
        }

        /// <summary>
        /// 获取可用充电站列表
        /// </summary>
        [HttpGet("available")]
        [ProducesResponseType(typeof(List<StationDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetAvailableStations(
            [FromQuery] decimal latitude,
            [FromQuery] decimal longitude,
            [FromQuery] decimal radius = 5) // 默认5公里半径
        {
            try
            {
                var stations = await _stationService.GetAvailableStationsAsync(latitude, longitude, radius);
                return Ok(new ApiResponse<List<StationDto>>
                {
                    Success = true,
                    Data = stations,
                    Message = stations.Count > 0 ? "Found available stations" : "No available stations found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available stations");
                return BadRequest(new ApiResponse<List<StationDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 获取充电站详情
        /// </summary>
        [HttpGet("{stationId}")]
        [ProducesResponseType(typeof(StationDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetStationDetail(string stationId)
        {
            try
            {
                var station = await _stationService.GetStationDetailAsync(stationId);

                if (station == null)
                    return NotFound(new ApiResponse<StationDetailDto>
                    {
                        Success = false,
                        Message = $"Station {stationId} not found"
                    });

                return Ok(new ApiResponse<StationDetailDto>
                {
                    Success = true,
                    Data = station
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting station detail for {StationId}", stationId);
                return BadRequest(new ApiResponse<StationDetailDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 获取充电站设备列表
        /// </summary>
        [HttpGet("{stationId}/equipment")]
        [ProducesResponseType(typeof(List<EquipmentDto>), 200)]
        public async Task<IActionResult> GetStationEquipment(string stationId)
        {
            try
            {
                var equipment = await _stationService.GetStationEquipmentAsync(stationId);
                return Ok(new ApiResponse<List<EquipmentDto>>
                {
                    Success = true,
                    Data = equipment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equipment for station {StationId}", stationId);
                return BadRequest(new ApiResponse<List<EquipmentDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 获取可用连接器列表
        /// </summary>
        [HttpGet("{stationId}/connectors/available")]
        [ProducesResponseType(typeof(List<ConnectorDto>), 200)]
        public async Task<IActionResult> GetAvailableConnectors(string stationId)
        {
            try
            {
                var connectors = await _stationService.GetAvailableConnectorsAsync(stationId);
                return Ok(new ApiResponse<List<ConnectorDto>>
                {
                    Success = true,
                    Data = connectors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available connectors for station {StationId}", stationId);
                return BadRequest(new ApiResponse<List<ConnectorDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 计算充电费用
        /// </summary>
        [HttpPost("{stationId}/calculate-cost")]
        [ProducesResponseType(typeof(ChargingCostDto), 200)]
        public async Task<IActionResult> CalculateChargingCost(
            string stationId,
            [FromBody] CalculateCostRequest request)
        {
            try
            {
                var cost = await _stationService.CalculateChargingCostAsync(
                    stationId,
                    request.EnergyKwh,
                    TimeSpan.FromMinutes(request.DurationMinutes),
                    request.IncludeParking);

                return Ok(new ApiResponse<ChargingCostDto>
                {
                    Success = true,
                    Data = cost
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating cost for station {StationId}", stationId);
                return BadRequest(new ApiResponse<ChargingCostDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 更新连接器状态（第三方推送接口）
        /// </summary>
        [HttpPost("connectors/{connectorId}/status")]
        [AllowAnonymous] // 允许第三方调用
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateConnectorStatus(
            string connectorId,
            [FromBody] UpdateConnectorStatusRequest request)
        {
            try
            {
                await _stationService.UpdateConnectorStatusAsync(
                    connectorId,
                    request.Status,
                    request.ParkStatus,
                    request.LockStatus);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Connector status updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating connector status for {ConnectorId}", connectorId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 获取连接器状态（批量查询）
        /// </summary>
        [HttpPost("connectors/status")]
        [ProducesResponseType(typeof(List<ConnectorStatusDto>), 200)]
        public async Task<IActionResult> GetConnectorsStatus([FromBody] GetConnectorStatusRequest request)
        {
            try
            {
                var status = await _stationService.GetConnectorsStatusAsync(request.ConnectorIds);
                return Ok(new ApiResponse<List<ConnectorStatusDto>>
                {
                    Success = true,
                    Data = status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connectors status");
                return BadRequest(new ApiResponse<List<ConnectorStatusDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 同步第三方数据（手动触发）
        /// </summary>
        [HttpPost("sync")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> SyncThirdPartyData()
        {
            try
            {
                await _stationService.SyncThirdPartyDataAsync();
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Third party data sync completed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing third party data");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // 请求DTO
        public class CalculateCostRequest
        {
            public decimal EnergyKwh { get; set; }
            public int DurationMinutes { get; set; }
            public bool IncludeParking { get; set; }
        }

        public class UpdateConnectorStatusRequest
        {
            public int Status { get; set; }
            public int? ParkStatus { get; set; }
            public int? LockStatus { get; set; }
        }

        public class GetConnectorStatusRequest
        {
            public List<string> ConnectorIds { get; set; }
        }

        // 通用API响应
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }
    }
}


