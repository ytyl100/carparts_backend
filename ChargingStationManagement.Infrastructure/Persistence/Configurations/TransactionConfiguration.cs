// ChargingStationManagement.Infrastructure/Persistence/Configurations/TransactionConfiguration.cs
using ChargingStationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChargingStationManagement.Infrastructure.Persistence.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .ValueGeneratedNever();

            builder.Property(t => t.TransactionId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.WalletId)
                .IsRequired();

            builder.Property(t => t.SessionId)
                .IsRequired(false);

            builder.Property(t => t.Type)
                .IsRequired();

            builder.Property(t => t.Amount)
                .HasPrecision(18, 4)
                .IsRequired();

            builder.Property(t => t.BalanceBefore)
                .HasPrecision(18, 4)
                .IsRequired();

            builder.Property(t => t.BalanceAfter)
                .HasPrecision(18, 4)
                .IsRequired();

            builder.Property(t => t.PaymentMethod)
                .IsRequired()
                .HasDefaultValue(1); // Wallet

            builder.Property(t => t.PaymentProvider)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(t => t.PaymentReference)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(t => t.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Completed");

            builder.Property(t => t.Description)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(t => t.OperatorId)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(t => t.ReferenceId)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(t => t.TransactionTime)
                .IsRequired();

            builder.Property(t => t.SettlementTime)
                .IsRequired(false);

            builder.Property(t => t.ReversalTime)
                .IsRequired(false);

            builder.Property(t => t.IsSettled)
                .HasDefaultValue(false);

            builder.Property(t => t.IsReversed)
                .HasDefaultValue(false);

            builder.Property(t => t.ReversalReason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(t => t.ServiceFee)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(t => t.Tax)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(t => t.Metadata)
                .HasMaxLength(2000)
                .IsRequired(false);

            builder.Property(t => t.Notes)
                .HasMaxLength(1000)
                .IsRequired(false);

            // 导航属性配置
            builder.HasMany("Wallet")
                 .WithOne("Transactions")
                 .HasForeignKey("WalletId");

            builder.HasOne("Session")
                .WithMany()
                .HasForeignKey("StationId");

            // 忽略领域事件
            builder.Ignore(t => t.DomainEvents);
        }
    }
}