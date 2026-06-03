using ChargingStationManagement.Services.Interfaces;

namespace ChargingStationManagement.API.BackgroundServices
{
    public class ThirdPartySyncService : BackgroundService
    {
        private readonly ILogger<ThirdPartySyncService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(1); // 每分钟同步一次

        public ThirdPartySyncService(
            ILogger<ThirdPartySyncService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ThirdPartySyncService is starting");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // 启动后延迟10秒再开始第一次同步
            using var timer = new PeriodicTimer(_syncInterval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var stationService = scope.ServiceProvider.GetRequiredService<IStationManagementService>();

                        _logger.LogInformation("Starting third party data sync at {Time}", DateTime.Now);

                        await stationService.SyncThirdPartyDataAsync();

                        _logger.LogInformation("Third party data sync completed at {Time}", DateTime.Now);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during third party data sync");
                }
            }

            _logger.LogInformation("ThirdPartySyncService is stopping");
        }
    }
}

