using System.Text;
using System.Text.Json;

namespace ChargingStationManagement.API.Middleware
{
    public class EvcsRequestWrapper
    {
        public string OperatorID { get; set; }
        public string Data { get; set; }          // Base64 编码的加密数据（实际是明文的Base64）
        public string TimeStamp { get; set; }     // yyyyMMdd HHmmss
        public string Seq { get; set; }           // 4位自增序列
        public string Sig { get; set; }           // 签名
    }

    public class EvcsResponseWrapper<T>
    {
        public int Ret { get; set; }
        public string Msg { get; set; }
        public T Data { get; set; }
        public string Sig { get; set; }   // 可选的响应签名（如果前端校验则可实现，否则可省略）
    }

    public class EvcsProtocolMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<EvcsProtocolMiddleware> _logger;

        public EvcsProtocolMiddleware(RequestDelegate next, ILogger<EvcsProtocolMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 明确的路径白名单
            var evcsApiPaths = new[]
            {
                "/evcs/query_stations_info",
                "/evcs/query_station_status", 
                "/evcs/query_equip_charge_status",
                "/evcs/query_start_charge",
                "/evcs/query_stop_charge",
                "/evcs/query_charge_order_info"
            };

            bool shouldProcess = context.Request.Method == HttpMethods.Post &&
                                 evcsApiPaths.Any(p => context.Request.Path.StartsWithSegments(p));

            if (shouldProcess)
            {
                await ProcessEvcsRequest(context);
            }
            else
            {
                await _next(context);
            }
        }

        private async Task ProcessEvcsRequest(HttpContext context)
        {
            context.Request.EnableBuffering();

            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            // 添加请求日志（生产环境建议脱敏）
            _logger.LogInformation("EVCS Request: {Path}, Body: {Body}", 
                context.Request.Path, 
                body.Substring(0, Math.Min(100, body.Length)));

            var wrapper = JsonSerializer.Deserialize<EvcsRequestWrapper>(body, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (wrapper == null || string.IsNullOrEmpty(wrapper.Data))
            {
                await WriteErrorResponse(context, 4003, "Invalid request format");
                return;
            }

            // 验证时间戳（防止重放攻击）
            if (!ValidateTimestamp(wrapper.TimeStamp))
            {
                await WriteErrorResponse(context, 4002, "Request expired");
                return;
            }

            // 验证签名
            if (!ValidateSignature(wrapper))
            {
                await WriteErrorResponse(context, 4001, "Signature error");
                return;
            }

            // 解密Data
            string decryptedDataJson;
            try
            {
                var dataBytes = Convert.FromBase64String(wrapper.Data);
                decryptedDataJson = Encoding.UTF8.GetString(dataBytes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Data decoding failed for OperatorID: {OperatorID}", wrapper.OperatorID);
                await WriteErrorResponse(context, 4004, "Data decoding failed");
                return;
            }

            // 存入上下文
            context.Items["EvcsDecryptedData"] = decryptedDataJson;
            context.Items["EvcsOperatorId"] = wrapper.OperatorID;
            context.Items["EvcsTimeStamp"] = wrapper.TimeStamp;
            context.Items["EvcsSeq"] = wrapper.Seq;

            await _next(context);
        }

        private bool ValidateTimestamp(string timestamp)
        {
            // yyyyMMddHHmmss 格式验证
            if (!DateTime.TryParseExact(timestamp, "yyyyMMddHHmmss", 
                null, System.Globalization.DateTimeStyles.None, out var requestTime))
            {
                return false;
            }

            // 允许5分钟时间差
            var timeDiff = Math.Abs((DateTime.Now - requestTime).TotalMinutes);
            return timeDiff <= 5;
        }

        private bool ValidateSignature(EvcsRequestWrapper wrapper)
        {
            // 前端算法：Sig = 'SIG_' + btoa(opId + data + ts + seq).substring(0, 12).toUpperCase()
            var combined = wrapper.OperatorID + wrapper.Data + wrapper.TimeStamp + wrapper.Seq;
            var combinedBytes = Encoding.UTF8.GetBytes(combined);
            var base64 = Convert.ToBase64String(combinedBytes);
            var expectedSig = "SIG_" + base64.Substring(0, 12).ToUpperInvariant();
            return wrapper.Sig == expectedSig;
        }

        private async Task WriteErrorResponse(HttpContext context, int ret, string msg)
        {
            context.Response.StatusCode = 200; // 业务错误也返回200，由Ret标识
            context.Response.ContentType = "application/json";
            var response = new EvcsResponseWrapper<object>
            {
                Ret = ret,
                Msg = msg,
                Data = null
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
