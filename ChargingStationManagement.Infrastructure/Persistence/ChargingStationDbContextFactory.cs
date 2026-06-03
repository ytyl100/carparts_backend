using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ChargingStationManagement.Infrastructure.Persistence
{
    /// <summary>
    /// EF Core 设计时 DbContext 工厂，用于 dotnet ef 迁移命令
    /// </summary>
    public class ChargingStationDbContextFactory : IDesignTimeDbContextFactory<ChargingStationDbContext>
    {
        public ChargingStationDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            // 尝试定位 CarDrivers 项目中的 appsettings.json
            string[] searchPaths =
            [
                Path.Combine(basePath, "CarDrivers"),
                Path.Combine(basePath, "..", "CarDrivers"),
                basePath
            ];

            var configPath = searchPaths
                .Select(p => Path.Combine(p, "appsettings.json"))
                .FirstOrDefault(File.Exists)
                ?? Path.Combine(basePath, "appsettings.json");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(configPath)!)
                .AddJsonFile(Path.GetFileName(configPath), optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=charging_station.db";

            var optionsBuilder = new DbContextOptionsBuilder<ChargingStationDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            return new ChargingStationDbContext(optionsBuilder.Options);
        }
    }
}

