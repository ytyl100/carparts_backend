// ChargingStationManagement.Infrastructure/Persistence/Configurations/SessionConfiguration.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChargingStationManagement.Infrastructure.Persistence.Configurations
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("Sessions");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedNever();

            builder.Property(s => s.SessionId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.StartChargeSeq)
                .IsRequired()
                .HasMaxLength(27);

            builder.Property(s => s.UserId)
                .IsRequired();

            builder.Property(s => s.ConnectorId)
                .IsRequired();

            builder.Property(s => s.EquipmentId)
                .IsRequired();

            builder.Property(s => s.StationId)
                .IsRequired();

            builder.Property(s => s.StartTime)
                .IsRequired();

            builder.Property(s => s.EndTime)
                .IsRequired(false);

            builder.Property(s => s.ScheduledEndTime)
                .IsRequired(false);

            builder.Property(s => s.Status)
                .IsRequired()
                .HasDefaultValue(ChargeStatus.Starting);

            builder.Property(s => s.OrderStatus)
                .IsRequired()
                .HasDefaultValue(OrderStatus.Created);

            builder.Property(s => s.ChargingMode)
                .IsRequired()
                .HasDefaultValue(ChargingMode.EnergyBased);

            builder.Property(s => s.StartMeterValue)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(s => s.EndMeterValue)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(s => s.TotalEnergy)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(s => s.PeakPower)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            builder.Property(s => s.AveragePower)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            builder.Property(s => s.ElectricityFee)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(s => s.ServiceFee)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(s => s.ParkFee)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(s => s.TotalAmount)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(s => s.IsPaid)
                .HasDefaultValue(false);

            builder.Property(s => s.PaymentTime)
                .IsRequired(false);

            builder.Property(s => s.PaymentTransactionId)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(s => s.VehicleId)
                .IsRequired(false);

            builder.Property(s => s.VehicleLicensePlate)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(s => s.VehicleBatteryCapacity)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            builder.Property(s => s.StartBatteryLevel)
                .HasPrecision(5, 2)
                .HasDefaultValue(0);

            builder.Property(s => s.EndBatteryLevel)
                .HasPrecision(5, 2)
                .HasDefaultValue(0);

            builder.Property(s => s.CurrentVoltage)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            builder.Property(s => s.CurrentCurrent)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            builder.Property(s => s.CurrentPower)
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            builder.Property(s => s.CurrentEnergy)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(s => s.LastDataUpdate)
                .IsRequired();

            builder.Property(s => s.StartedBy)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("User");

            builder.Property(s => s.StoppedBy)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(s => s.StopReason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(s => s.QRCode)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(s => s.ReservationId)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(s => s.Notes)
                .HasMaxLength(1000)
                .IsRequired(false);

           
            // 忽略 object 类型的导航属性（关系在 DbContext.ConfigureRelationships 中统一配置）
            builder.Ignore(s => s.User);
            builder.Ignore(s => s.Connector);
            builder.Ignore(s => s.Station);

            // 忽略领域事件和计算属性
            builder.Ignore(s => s.DomainEvents);
        }
    }
}