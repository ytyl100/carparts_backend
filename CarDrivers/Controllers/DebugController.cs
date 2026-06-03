using ChargingStationManagement.Services.DTOs;
using ChargingStationManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly IStationManagementService _stationService;
    private readonly ILogger<DebugController> _logger;

    public DebugController(
        IStationManagementService stationService,
        ILogger<DebugController> logger)
    {
        _stationService = stationService;
        _logger = logger;
    }

    /// <summary>
    /// 手动触发第三方数据同步（调试用）
    /// </summary>
    [HttpPost("trigger-sync")]
    public async Task<IActionResult> TriggerSync()
    {
        try
        {
            _logger.LogInformation("Manual sync triggered from API");
            await _stationService.SyncThirdPartyDataAsync();
            return Ok(new { message = "Sync completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manual sync failed");
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// 测试单个第三方接口
    /// </summary>
    [HttpGet("test-third-party/{name}")]
    public async Task<IActionResult> TestThirdParty(
        string name,
        [FromServices] IApiThirdPartyIntegrationService thirdPartyService)
    {
        try
        {
            // 测试获取 Token
            var token = await thirdPartyService.GetAccessTokenAsync(name);
            
            // 测试获取站点数据
            var stations = await thirdPartyService.SyncStationsAsync(name);
            
            return Ok(new
            {
                thirdParty = name,
                tokenReceived = !string.IsNullOrEmpty(token),
                stationsCount = stations?.Count ?? 0,
                stations = stations?.Take(5) // 只返回前5个
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }
}