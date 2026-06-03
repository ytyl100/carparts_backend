using ChargingStationManagement.API.Controllers;
using ChargingStationManagement.Services.DTOs;
using ChargingStationManagement.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;
using static ChargingStationManagement.API.Controllers.StationsController;

namespace ChargingStationManagement.API.Controllers
{
    
    [Route("evcs/station")]
    public class EvcStationController : EvcsBaseController
    {
        private readonly IStationManagementService _stationService;
        private readonly ILogger<StationsController> _logger;

        public EvcStationController(
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
                // 获取解密后的请求数据
                var requestData = GetDecryptedData<StationListRequest>();
                var stations = await _stationService.GetAvailableStationsAsync(requestData.Latitude, requestData.Latitude, requestData.Radius);
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
        public class StationListRequest
        {
            public int PageNo { get; set; }
            public int PageSize { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public decimal Radius { get; set; } = 5;
            // other filters like city, etc.
        }
    }
}
