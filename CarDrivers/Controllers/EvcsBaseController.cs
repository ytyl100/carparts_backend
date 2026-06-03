using ChargingStationManagement.API.Middleware;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ChargingStationManagement.API.Controllers
{
    [ApiController]
    public abstract class EvcsBaseController : ControllerBase
    {
        protected T GetDecryptedData<T>()
        {
            if (HttpContext.Items.TryGetValue("EvcsDecryptedData", out var json) && json is string jsonStr)
            {
                return JsonSerializer.Deserialize<T>(jsonStr);
            }
            throw new InvalidOperationException("No decrypted data available");
        }

        protected IActionResult EvcsOk<T>(T data, string msg = "Success")
        {
            var response = new EvcsResponseWrapper<T>
            {
                Ret = 0,
                Msg = msg,
                Data = data
            };
            // 如果需要响应签名，可在此生成（按前端算法反向生成）
            return Ok(response);
        }

        protected IActionResult EvcsError(int ret, string msg)
        {
            var response = new EvcsResponseWrapper<object>
            {
                Ret = ret,
                Msg = msg,
                Data = null
            };
            return Ok(response);
        }
    }
}
