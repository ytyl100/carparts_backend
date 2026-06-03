// ChargingStationManagement.Infrastructure/DependencyInjection.cs
using ChargingStationManagement.Domain.Interfaces;
using ChargingStationManagement.Infrastructure.Cache;
using ChargingStationManagement.Infrastructure.External;
using ChargingStationManagement.Infrastructure.FileStorage;
using ChargingStationManagement.Infrastructure.Identity;
using ChargingStationManagement.Infrastructure.Persistence;
using ChargingStationManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ChargingStationManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 使用 SQLite
            services.AddDbContext<ChargingStationDbContext>(options =>
                options.UseSqlite(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqliteOptions =>
                    {
                        sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }));

            // 注册配置
            services.Configure<ThirdPartyApiSettings>(configuration.GetSection("ThirdPartyApi"));
            services.Configure<JsonFileStorageSettings>(configuration.GetSection("JsonFileStorage"));
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            // 注册仓储
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<StationRepository>();
            services.AddScoped<EquipmentRepository>();
            services.AddScoped<ConnectorRepository>();
            services.AddScoped<UserRepository>();
            services.AddScoped<WalletRepository>();
            services.AddScoped<SessionRepository>();
            services.AddScoped<OperatorRepository>();

            // 注册缓存服务
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();

            // 注册文件存储服务
            services.AddSingleton<IJsonFileStorage, JsonFileStorage>();
            services.AddSingleton<IStationDataStorage, StationDataStorage>();
            services.AddSingleton<IUserDataStorage, UserDataStorage>();

            // 注册第三方API客户端
            services.AddHttpClient();
            services.AddSingleton<GuangqiApiClient>();
            services.AddSingleton<OrangeApiClient>();
            services.AddSingleton<TeslaApiClient>();
            services.AddSingleton<XiaojuApiClient>();
            services.AddSingleton<IThirdPartyApiClientFactory, ThirdPartyApiClientFactory>();

            // 注册身份认证服务
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IUserManager, UserManager>();

            // 注册健康检查
            //services.AddHealthChecks()
            //    .AddDbContextCheck<ChargingStationDbContext>()
            //    .AddCheck<CacheHealthCheck>("cache")
            //    .AddCheck<ExternalApiHealthCheck>("external_api");

            return services;
        }
    }
}