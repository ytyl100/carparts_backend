// ChargingStationManagement.Infrastructure/Persistence/Configurations/StationConfiguration.cs
using ChargingStationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChargingStationManagement.Infrastructure.Persistence.Configurations
{
    public class StationConfiguration : IEntityTypeConfiguration<Station>
    {
        public void Configure(EntityTypeBuilder<Station> builder)
        {
            builder.ToTable("Stations");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedNever();

            builder.Property(s => s.StationId)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.OperatorId)
                .IsRequired()
                .HasMaxLength(9);

            builder.Property(s => s.EquipmentOwnerId)
                .HasMaxLength(9);

            builder.Property(s => s.StationName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.CountryCode)
                .IsRequired()
                .HasMaxLength(2)
                .HasDefaultValue("CN");

            builder.Property(s => s.AreaCode)
                .HasMaxLength(6);

            builder.Property(s => s.StationTel)
                .HasMaxLength(50);

            builder.Property(s => s.ServiceTel)
                .HasMaxLength(50);

            // 不再注册 StationType/StationStatus 为影子属性
            // 让 EF Core 自动通过后备字段约定发现 private int stationType
            // 仅通过 Fluent API 设置默认值即可

            builder.Property(s => s.ParkNums)
                .HasDefaultValue(0);

            builder.Property(s => s.StationLng)
                .HasPrecision(18, 6)
                .IsRequired();

            builder.Property(s => s.StationLat)
                .HasPrecision(18, 6)
                .IsRequired();

            builder.Property(s => s.SiteGuide)
                .HasMaxLength(1000);

            builder.Property(s => s.Pictures)
                .HasMaxLength(2000); // 存储多个URL，用分号分隔

            builder.Property(s => s.MatchCars)
                .HasMaxLength(500);

            builder.Property(s => s.ParkInfo)
                .HasMaxLength(500);

            builder.Property(s => s.BusinessHours)
                .HasMaxLength(200);

            builder.Property(s => s.StationElectricity)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(s => s.TotalRevenue)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(s => s.LastSyncTime)
                .IsRequired();

            builder.Property(s => s.LastStatusChangeTime)
                .IsRequired(false);

            builder.Property(s => s.Source)
                .IsRequired()
                .HasMaxLength(50);

            // 导航属性关系在 DbContext.ConfigureRelationships 中统一配置

            // 忽略领域事件和计算属性
            builder.Ignore(s => s.DomainEvents);
            builder.Ignore(s => s.TotalConnectors);
            builder.Ignore(s => s.AvailableConnectors);
            builder.Ignore(s => s.TotalPower);
        }
    }
}