// ChargingStationManagement.Infrastructure/Persistence/Configurations/EquipmentConfiguration.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChargingStationManagement.Infrastructure.Persistence.Configurations
{
    public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            builder.ToTable("Equipment");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id)
                .ValueGeneratedNever();
            
            builder.Property(e => e.EquipmentId)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.StationId)
                .IsRequired();
                
            builder.Property(e => e.ManufacturerId)
                .HasMaxLength(9);
                
            builder.Property(e => e.ManufacturerName)
                .HasMaxLength(200);
                
            builder.Property(e => e.EquipmentModel)
                .HasMaxLength(100);
                
            builder.Property(e => e.ProductionDate)
                .IsRequired();
                
            builder.Property(e => e.EquipmentType)
                .IsRequired()
                .HasDefaultValue(EquipmentType.TwoWheeler);
                
            // EquipmentLng/EquipmentLat 在实体中为 object 类型，通过 DbContext 的 OwnedType Location 配置
            builder.Ignore(e => e.EquipmentLng);
            builder.Ignore(e => e.EquipmentLat);
                
            builder.Property(e => e.Power)
                .HasPrecision(10, 2)
                .IsRequired();
                
            builder.Property(e => e.MaxPower)
                .HasPrecision(10, 2)
                .IsRequired();
                
            builder.Property(e => e.MinPower)
                .HasPrecision(10, 2)
                .IsRequired();
                
            builder.Property(e => e.Voltage)
                .HasPrecision(10, 2)
                .HasDefaultValue(220);
                
            builder.Property(e => e.Current)
                .HasPrecision(10, 2)
                .HasDefaultValue(15);
                
            builder.Property(e => e.EquipmentName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(EquipmentStatus.Idle);
                
            builder.Property(e => e.StatusUpdateTime)
                .IsRequired();
                
            builder.Property(e => e.StatusReason)
                .HasMaxLength(500);
                
            builder.Property(e => e.EquipmentElectricity)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);
                
            builder.Property(e => e.TotalSessions)
                .HasDefaultValue(0);
                
            builder.Property(e => e.TotalChargingTime)
                .HasConversion(
                    v => v.Ticks,
                    v => TimeSpan.FromTicks(v))
                .HasDefaultValue(TimeSpan.Zero);
                
            builder.Property(e => e.SupportsDynamicPower)
                .HasDefaultValue(false);
                
            builder.Property(e => e.CommunicationProtocol)
                .HasMaxLength(50);
                
            builder.Property(e => e.FirmwareVersion)
                .HasMaxLength(50);
                
            builder.Property(e => e.LastMaintenanceDate)
                .IsRequired(false);
                
            builder.Property(e => e.NextMaintenanceDate)
                .IsRequired(false);
                
            builder.Property(e => e.MaintenanceContact)
                .HasMaxLength(100);
                
            builder.Property(e => e.Source)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property("StationType")
                .HasDefaultValue(1);

            builder.Property("StationStatus")
                .HasDefaultValue(50); // Normal

            // 导航属性配置
            // Connectors 关系在 DbContext.ConfigureRelationships 中配置

            // 忽略 object 类型的导航属性
            builder.Ignore(e => e.Station);
            builder.Ignore(e => e.Sessions);

            builder.HasMany(e => e.StatusHistory)
                .WithOne()
                .HasForeignKey("EquipmentId");
                
            // 忽略领域事件和计算属性
            builder.Ignore(e => e.DomainEvents);
            builder.Ignore(e => e.TotalConnectors);
            builder.Ignore(e => e.AvailableConnectors);
        }
    }
}