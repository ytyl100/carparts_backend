// ChargingStationManagement.Infrastructure/ServiceExtensions.cs
using ChargingStationManagement.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace ChargingStationManagement.Infrastructure
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 配置日志
            services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
                logging.AddConfiguration(configuration.GetSection("Logging"));
            });

            // 注册基础设施服务
            services.AddInfrastructure(configuration);

            // 注册HttpClient工厂（用于第三方API调用）
            services.AddHttpClient("ThirdPartyApi", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "ChargingStationManagement/1.0");
            });

            // 注册Polly策略（用于重试和断路器）
            //services.AddPolicyRegistry();

            // 注册AutoMapper（如果需要）
            services.AddAutoMapper(typeof(ServiceExtensions).Assembly);

            // 注册MediatR（用于领域事件）
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(ServiceExtensions).Assembly);
            });

            // 注册FluentValidation（如果需要）
            //services.AddValidatorsFromAssembly(typeof(ServiceExtensions).Assembly);

            return services;
        }

        public static void ConfigureInfrastructure(this IServiceProvider serviceProvider)
        {
            // 初始化数据库
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ChargingStationDbContext>();

                // 确保数据库创建
                dbContext.Database.EnsureCreated();

                // 应用迁移
                //dbContext.Database.Migrate();

                // 种子数据
                SeedData.Initialize(dbContext);
            }
        }
    }
}