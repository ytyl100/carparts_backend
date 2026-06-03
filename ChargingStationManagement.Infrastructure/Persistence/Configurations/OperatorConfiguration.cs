// ChargingStationManagement.Infrastructure/Persistence/Configurations/OperatorConfiguration.cs
using ChargingStationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChargingStationManagement.Infrastructure.Persistence.Configurations
{
    public class OperatorConfiguration : IEntityTypeConfiguration<Operator>
    {
        public void Configure(EntityTypeBuilder<Operator> builder)
        {
            builder.ToTable("Operators");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .ValueGeneratedNever(); // 使用自定义Id

            builder.Property(o => o.OperatorId)
                .IsRequired()
                .HasMaxLength(9);

            builder.Property(o => o.OperatorName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(o => o.OperatorTel)
                .HasMaxLength(50);

            builder.Property(o => o.OperatorRegAddress)
                .HasMaxLength(500);

            builder.Property(o => o.OperatorNote)
                .HasMaxLength(1000);

            builder.Property(o => o.ElectricityProfitRate)
                .HasPrecision(5, 4)
                .HasDefaultValue(0.8m);

            builder.Property(o => o.ServiceProfitRate)
                .HasPrecision(5, 4)
                .HasDefaultValue(0.1m);

            builder.Property(o => o.ParkProfitRate)
                .HasPrecision(5, 4)
                .HasDefaultValue(0.1m);

            builder.Property(o => o.ApiBaseUrl)
                .HasMaxLength(500);

            builder.Property(o => o.ApiToken)
                .HasMaxLength(500);

            builder.Property(o => o.ApiSecret)
                .HasMaxLength(500);

            builder.Property(o => o.ApiEncryptionKey)
                .HasMaxLength(500);

            builder.Property(o => o.IsActive)
                .HasDefaultValue(true);

            builder.Property(o => o.ActivationDate)
                .IsRequired(false);

            // 忽略领域事件集合（不持久化）
            builder.Ignore(o => o.DomainEvents);
        }
    }
}