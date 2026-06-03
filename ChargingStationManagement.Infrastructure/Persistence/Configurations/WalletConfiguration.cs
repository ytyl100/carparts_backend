// ChargingStationManagement.Infrastructure/Persistence/Configurations/WalletConfiguration.cs
using ChargingStationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChargingStationManagement.Infrastructure.Persistence.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.Id)
                .ValueGeneratedNever();

            builder.Property(w => w.WalletId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(w => w.UserId)
                .IsRequired();

            builder.Property(w => w.Balance)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(w => w.AvailableBalance)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(w => w.FrozenBalance)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(w => w.CreditLimit)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(w => w.CreditUsed)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(w => w.TotalRecharge)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(w => w.TotalConsumption)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(w => w.TotalRefund)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(w => w.TotalCommission)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(w => w.TotalTransactions)
                .HasDefaultValue(0);

            builder.Property(w => w.DailySpendingLimit)
                .HasPrecision(18, 4)
                .HasDefaultValue(5000);

            builder.Property(w => w.SingleSpendingLimit)
                .HasPrecision(18, 4)
                .HasDefaultValue(2000);

            builder.Property(w => w.AutoRechargeEnabled)
                .HasDefaultValue(false);

            builder.Property(w => w.AutoRechargeThreshold)
                .HasPrecision(18, 4)
                .HasDefaultValue(50);

            builder.Property(w => w.AutoRechargeAmount)
                .HasPrecision(18, 4)
                .HasDefaultValue(200);

            builder.Property(w => w.LastRechargeTime)
                .IsRequired(false);

            builder.Property(w => w.LastConsumptionTime)
                .IsRequired(false);

            builder.Property(w => w.LastUpdateTime)
                .IsRequired();

            // 导航属性配置
            builder.HasMany("Transactions")
                 .WithOne("Wallet")
                 .HasForeignKey("WalletId");

            builder.HasMany("DailySpendingRecords")
                 .WithOne("Wallet")
                 .HasForeignKey("WalletId");

            // 忽略领域事件
            builder.Ignore(w => w.DomainEvents);
        }
    }
}