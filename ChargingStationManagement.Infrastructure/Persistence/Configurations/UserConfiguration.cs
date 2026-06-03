// ChargingStationManagement.Infrastructure/Persistence/Configurations/UserConfiguration.cs
using ChargingStationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChargingStationManagement.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .ValueGeneratedNever();

            builder.Property(u => u.UserId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(u => u.Email)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.IdentityNumber)
                .HasMaxLength(18)
                .IsRequired(false);

            builder.Property(u => u.UserType)
                .IsRequired()
                .HasDefaultValue(1); // Normal

            builder.Property(u => u.AvatarUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(u => u.DateOfBirth)
                .IsRequired(false);

            builder.Property(u => u.Gender)
                .HasDefaultValue(0); // Unknown

            builder.Property(u => u.EmergencyContact)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(u => u.EmergencyPhone)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);

            builder.Property(u => u.IsVerified)
                .HasDefaultValue(false);

            builder.Property(u => u.VerificationDate)
                .IsRequired(false);

            builder.Property(u => u.VerificationMethod)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(u => u.PasswordHash)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(u => u.Salt)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(u => u.LastLoginTime)
                .IsRequired(false);

            builder.Property(u => u.LastLoginIp)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(u => u.FailedLoginAttempts)
                .HasDefaultValue(0);

            builder.Property(u => u.LockoutEndTime)
                .IsRequired(false);

            builder.Property(u => u.Language)
                .HasMaxLength(10)
                .HasDefaultValue("zh-CN");

            builder.Property(u => u.Timezone)
                .HasMaxLength(50)
                .HasDefaultValue("China Standard Time");

            builder.Property(u => u.TotalSessions)
                .HasDefaultValue(0);

            builder.Property(u => u.TotalEnergyConsumed)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(u => u.TotalAmountSpent)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);

            builder.Property(u => u.LastChargingTime)
                .IsRequired(false);

            builder.Property(u => u.RegistrationDate)
                .IsRequired();

            builder.Property(u => u.RegistrationSource)
                .HasMaxLength(50)
                .HasDefaultValue("App");

            builder.Property(u => u.ReferralCode)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(u => u.ReferredBy)
                .HasMaxLength(50)
                .IsRequired(false);

            // 导航属性配置
            builder.HasMany("Wallet")
                 .WithOne("User")
                 .HasForeignKey("UserId");

            builder.HasMany("Vehicles")
                 .WithOne("User")
                 .HasForeignKey("UserId");

            builder.HasMany("Sessions")
                 .WithOne("User")
                 .HasForeignKey("UserId");

            builder.HasMany("Favorites")
                 .WithOne("User")
                 .HasForeignKey("UserId");

            // 忽略领域事件
            builder.Ignore(u => u.DomainEvents);
        }
    }
}