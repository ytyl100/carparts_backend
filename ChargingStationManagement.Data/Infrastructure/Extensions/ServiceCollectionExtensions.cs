using ChargingStationManagement.Domain.Interfaces;
using ChargingStationManagement.Infrastructure.Persistence;
using ChargingStationManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChargingStationManagement.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChargingDataLayer(this IServiceCollection services, string sqliteConnectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(sqliteConnectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}