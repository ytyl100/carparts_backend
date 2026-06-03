// ChargingStationManagement.Infrastructure/Persistence/Configurations/ConnectorConfiguration.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChargingStationManagement.Infrastructure.Persistence.Configurations
{
    public class ConnectorConfiguration : IEntityTypeConfiguration<Connector>
    {
        public void Configure(EntityTypeBuilder<Connector> builder)
        {
            builder.ToTable("Connectors");
            
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Id)
                .ValueGeneratedNever();
            
            builder.Property(c => c.ConnectorId)
                .IsRequired()
                .HasMaxLength(26);
                
            builder.Property(c => c.EquipmentId)
                .IsRequired();
                
            builder.Property(c => c.ConnectorName)
                .HasMaxLength(100)
                .HasDefaultValue("");
                
            builder.Property(c => c.Standard)
                .IsRequired()
                .HasDefaultValue(ConnectorStandard.GBT_AC);
                
            builder.Property(c => c.VoltageUpperLimits)
                .HasPrecision(10, 2)
                .HasDefaultValue(1000);
                
            builder.Property(c => c.VoltageLowerLimits)
                .HasPrecision(10, 2)
                .HasDefaultValue(200);
                
            builder.Property(c => c.Current)
                .HasPrecision(10, 2)
                .HasDefaultValue(15);
                
            builder.Property(c => c.Power)
                .HasPrecision(10, 2)
                .IsRequired();
                
            builder.Property(c => c.ParkNo)
                .HasMaxLength(50);
                
            builder.Property(c => c.Status)
                .IsRequired()
                .HasDefaultValue(ConnectorStatus.Idle);
                
            builder.Property(c => c.ParkStatus)
                .HasDefaultValue(ParkStatus.Unknown);
                
            builder.Property(c => c.LockStatus)
                .HasDefaultValue(LockStatus.Unknown);
                
            builder.Property(c => c.StatusUpdateTime)
                .IsRequired();
                
            builder.Property(c => c.StatusReason)
                .HasMaxLength(500);
                
            builder.Property(c => c.CurrentSessionId)
                .IsRequired(false);
                
            builder.Property(c => c.CurrentUserId)
                .HasMaxLength(50)
                .IsRequired(false);
                
            builder.Property(c => c.SessionStartTime)
                .IsRequired(false);
                
            builder.Property(c => c.TotalSessions)
                .HasDefaultValue(0);
                
            builder.Property(c => c.ConnectorElectricity)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);
                
            builder.Property(c => c.TotalChargingTime)
                .HasConversion(
                    v => v.Ticks,
                    v => TimeSpan.FromTicks(v))
                .HasDefaultValue(TimeSpan.Zero);
                
            builder.Property(c => c.TotalRevenue)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);
                
            builder.Property(c => c.CurrentVoltage)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);
                
            builder.Property(c => c.CurrentCurrent)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);
                
            builder.Property(c => c.CurrentPower)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);
                
            builder.Property(c => c.CurrentEnergy)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);
                
            builder.Property(c => c.LastDataUpdate)
                .IsRequired();
                
            builder.Property(c => c.Source)
                .IsRequired()
                .HasMaxLength(50);
                
            // 忽略 object 类型的导航属性（关系在 DbContext.ConfigureRelationships 中配置）
            builder.Ignore(c => c.Equipment);
            builder.Ignore(c => c.Sessions);
                
            // 索引
            builder.HasIndex(c => new { c.EquipmentId, c.ConnectorId })
                .IsUnique();
                
            // 忽略领域事件
            builder.Ignore(c => c.DomainEvents);
        }
    }
}