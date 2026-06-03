using ChargingStationManagement.Services.Interfaces;

namespace ChargingStationManagement.API.BackgroundServices
{
    public class SessionMonitorService : BackgroundService
    {
        private readonly ILogger<SessionMonitorService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _monitorInterval = TimeSpan.FromSeconds(30); // 每30秒监控一次

        public SessionMonitorService(
            ILogger<SessionMonitorService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SessionMonitorService is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var chargingService = scope.ServiceProvider.GetRequiredService<IApiChargingService>();

                        // 检查低余额会话
                        await chargingService.CheckAndHandleLowBalanceSessionsAsync();

                        // 检查计划结束会话
                        await chargingService.CheckScheduledEndSessionsAsync();

                        _logger.LogDebug("Session monitoring completed at {Time}", DateTime.Now);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during session monitoring");
                }

                await Task.Delay(_monitorInterval, stoppingToken);
            }

            _logger.LogInformation("SessionMonitorService is stopping");
        }
    }
}