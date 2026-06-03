using ChargingStationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;

namespace ChargingStationManagement.Infrastructure.Persistence
{
    public class ChargingStationDbContext : DbContext
    {
        public ChargingStationDbContext(DbContextOptions<ChargingStationDbContext> options)
            : base(options)
        {
        }

        // 核心实体
        public DbSet<Operator> Operators { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Connector> Connectors { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        // 历史记录和辅助实体
        public DbSet<StationStatusHistory> StationStatusHistory { get; set; }
        public DbSet<EquipmentStatusHistory> EquipmentStatusHistory { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<DailySpending> DailySpendings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ⚡ 先配置关系（在 ApplyConfigurationsFromAssembly 之前）
            ConfigureRelationships(modelBuilder);

            // 然后应用所有 IEntityTypeConfiguration
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // 配置值对象的转换
            ConfigureValueObjects(modelBuilder);

            // 配置索引
            ConfigureIndexes(modelBuilder);

            // 配置全局查询过滤器
            ConfigureGlobalFilters(modelBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // SQLite 不支持精度配置，改为条件配置
            var databaseProvider = this.Database.ProviderName;
            if (databaseProvider != "Microsoft.EntityFrameworkCore.Sqlite")
            {
                configurationBuilder.Properties<decimal>()
                    .HavePrecision(18, 4); // 仅用于 SQL Server 和 MySQL
            }

            configurationBuilder.Properties<DateTime>()
                .HaveConversion(typeof(DateTimeUtcConverter));
        }

        private void ConfigureValueObjects(ModelBuilder modelBuilder)
        {
            // 地址值对象转换
            modelBuilder.Entity<Station>().OwnsOne(s => s.Address, a =>
            {
                a.Property(p => p.FullAddress).HasColumnName("Address");
                a.Property(p => p.Street).HasColumnName("Street");
                a.Property(p => p.City).HasColumnName("City");
                a.Property(p => p.Province).HasColumnName("Province");
                a.Property(p => p.Country).HasColumnName("Country");
                a.Property(p => p.PostalCode).HasColumnName("PostalCode");
            });

            // 坐标值对象转换
            modelBuilder.Entity<Station>().OwnsOne(s => s.Location, l =>
            {
                l.Property(p => p.Latitude).HasColumnName("StationLat");
                l.Property(p => p.Longitude).HasColumnName("StationLng");
                l.Ignore(p => p.Altitude);
            });

            modelBuilder.Entity<Equipment>().OwnsOne(e => e.Location, l =>
            {
                l.Property(p => p.Latitude).HasColumnName("EquipmentLat");
                l.Property(p => p.Longitude).HasColumnName("EquipmentLng");
                l.Ignore(p => p.Altitude);
            });

            // 费率值对象转换
            modelBuilder.Entity<Station>().OwnsOne(s => s.ElectricityRate, r =>
            {
                r.Property(p => p.ElectricityRate).HasColumnName("ElectricityRate");
                r.Property(p => p.ServiceRate).HasColumnName("ElectricityServiceRate");
                r.Property(p => p.ParkRate).HasColumnName("ElectricityParkRate");
                r.Property(p => p.TimeRate).HasColumnName("ElectricityTimeRate");
            });

            modelBuilder.Entity<Station>().OwnsOne(s => s.ServiceRate, r =>
            {
                r.Property(p => p.ElectricityRate).HasColumnName("ServiceRate");
                r.Property(p => p.ServiceRate).HasColumnName("ServiceServiceRate");
                r.Property(p => p.ParkRate).HasColumnName("ServiceParkRate");
                r.Property(p => p.TimeRate).HasColumnName("ServiceTimeRate");
            });

            modelBuilder.Entity<Station>().OwnsOne(s => s.ParkRate, r =>
            {
                r.Property(p => p.ElectricityRate).HasColumnName("ParkRate");
                r.Property(p => p.ServiceRate).HasColumnName("ParkServiceRate");
                r.Property(p => p.ParkRate).HasColumnName("ParkParkRate");
                r.Property(p => p.TimeRate).HasColumnName("ParkTimeRate");
            });

            modelBuilder.Entity<Session>().OwnsOne(s => s.AppliedRates, r =>
            {
                r.Property(p => p.ElectricityRate).HasColumnName("AppliedElectricityRate");
                r.Property(p => p.ServiceRate).HasColumnName("AppliedServiceRate");
                r.Property(p => p.ParkRate).HasColumnName("AppliedParkRate");
                r.Property(p => p.TimeRate).HasColumnName("AppliedTimeRate");
            });

            // 功率配置值对象转换
            modelBuilder.Entity<Equipment>().OwnsOne(e => e.PowerConfig, p =>
            {
                p.Property(p => p.MinPower).HasColumnName("MinPower");
                p.Property(p => p.RatedPower).HasColumnName("RatedPower");
                p.Property(p => p.MaxPower).HasColumnName("MaxPower");
            });

            // 通知偏好值对象转换
            modelBuilder.Entity<User>().OwnsOne(u => u.NotificationPrefs, n =>
            {
                n.Property(p => p.EmailNotifications).HasColumnName("EmailNotifications");
                n.Property(p => p.SmsNotifications).HasColumnName("SmsNotifications");
                n.Property(p => p.PushNotifications).HasColumnName("PushNotifications");
                n.Property(p => p.MarketingNotifications).HasColumnName("MarketingNotifications");
                n.Property(p => p.QuietHoursStart).HasColumnName("QuietHoursStart");
                n.Property(p => p.QuietHoursEnd).HasColumnName("QuietHoursEnd");
            });
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // Operator 索引
            modelBuilder.Entity<Operator>()
                .HasIndex(o => o.OperatorId)
                .IsUnique();

            // Station 索引
            modelBuilder.Entity<Station>()
                .HasIndex(s => s.StationId)
                .IsUnique();

            modelBuilder.Entity<Station>()
                .HasIndex(s => new { s.StationLat, s.StationLng });

            modelBuilder.Entity<Station>()
                .HasIndex(s => s.OperatorId);

            modelBuilder.Entity<Station>()
                .HasIndex(s => s.Status);

            // Equipment 索引
            modelBuilder.Entity<Equipment>()
                .HasIndex(e => e.EquipmentId)
                .IsUnique();

            modelBuilder.Entity<Equipment>()
                .HasIndex(e => e.StationId);

            modelBuilder.Entity<Equipment>()
                .HasIndex(e => e.Status);

            modelBuilder.Entity<Equipment>()
                .HasIndex(e => e.EquipmentType);

            // Connector 索引
            modelBuilder.Entity<Connector>()
                .HasIndex(c => new { c.EquipmentId, c.ConnectorId })
                .IsUnique();

            modelBuilder.Entity<Connector>()
                .HasIndex(c => c.Status);

            modelBuilder.Entity<Connector>()
                .HasIndex(c => c.Standard);

            // User 索引
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserId)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Wallet 索引
            modelBuilder.Entity<Wallet>()
                .HasIndex(w => w.WalletId)
                .IsUnique();

            modelBuilder.Entity<Wallet>()
                .HasIndex(w => w.UserId)
                .IsUnique();

            // Session 索引
            modelBuilder.Entity<Session>()
                .HasIndex(s => s.SessionId)
                .IsUnique();

            modelBuilder.Entity<Session>()
                .HasIndex(s => s.StartChargeSeq)
                .IsUnique();

            modelBuilder.Entity<Session>()
                .HasIndex(s => new { s.UserId, s.StartTime });

            modelBuilder.Entity<Session>()
                .HasIndex(s => new { s.StationId, s.StartTime });

            modelBuilder.Entity<Session>()
                .HasIndex(s => s.Status);

            modelBuilder.Entity<Session>()
                .HasIndex(s => s.OrderStatus);

            // Transaction 索引
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.TransactionId)
                .IsUnique();

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.WalletId);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.TransactionTime);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Type);

            // 其他实体索引
            modelBuilder.Entity<UserFavorite>()
                .HasIndex(f => new { f.UserId, f.StationId })
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            modelBuilder.Entity<DailySpending>()
                .HasIndex(d => new { d.WalletId, d.Date })
                .IsUnique();
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // Operator -> Station (一对多)
            modelBuilder.Entity<Station>()
                .HasOne(s => s.Operator)
                .WithMany("Stations")
                .HasForeignKey(s => s.OperatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Station -> Equipment (一对多)
            // 不创建导航属性，仅定义外键关系，避免 Station 影子属性扩散到 Equipment
            modelBuilder.Entity<Equipment>()
                .HasIndex(e => e.StationId);
            modelBuilder.Entity<Equipment>()
                .HasOne<Station>()
                .WithMany()
                .HasForeignKey(e => e.StationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Equipment -> Connector (一对多)
            // Connector.Equipment 是 object 类型，已在 ConnectorConfiguration 中 Ignore
            modelBuilder.Entity<Connector>()
                .HasOne<Equipment>()
                .WithMany(e => e.Connectors)
                .HasForeignKey(c => c.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Wallet (一对一)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Wallet)
                .WithOne("User")
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Session (一对多)
            // Session.User 是 object 类型，已在 SessionConfiguration 中 Ignore
            modelBuilder.Entity<Session>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Session -> Connector (多对一)
            // Session.Connector 是 object 类型，已在 SessionConfiguration 中 Ignore
            modelBuilder.Entity<Session>()
                .HasOne<Connector>()
                .WithMany()
                .HasForeignKey(s => s.ConnectorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Session -> Equipment (多对一)
            modelBuilder.Entity<Session>()
                .HasOne<Equipment>()
                .WithMany()
                .HasForeignKey("EquipmentId")
                .OnDelete(DeleteBehavior.Restrict);

            // Session -> Station (多对一)
            // Session.Station 是 object 类型，已在 SessionConfiguration 中 Ignore
            modelBuilder.Entity<Session>()
                .HasOne<Station>()
                .WithMany()
                .HasForeignKey(s => s.StationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Wallet -> Transaction (一对多)
            modelBuilder.Entity<Transaction>()
                .HasOne("Wallet")
                .WithMany("Transactions")
                .HasForeignKey("WalletId")
                .OnDelete(DeleteBehavior.Cascade);

            // Session -> Transaction (可选一对一)
            modelBuilder.Entity<Transaction>()
                .HasOne("Session")
                .WithMany()
                .HasForeignKey("SessionId")
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Vehicle (一对多)
            modelBuilder.Entity<Vehicle>()
                .HasOne("User")
                .WithMany("Vehicles")
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade);

            // User -> UserFavorite (一对多)
            modelBuilder.Entity<UserFavorite>()
                .HasOne("User")
                .WithMany("Favorites")
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade);

            // Wallet -> DailySpending (一对多)
            modelBuilder.Entity<DailySpending>()
                .HasOne("Wallet")
                .WithMany("DailySpendingRecords")
                .HasForeignKey("WalletId")
                .OnDelete(DeleteBehavior.Cascade);

            // Station -> StationStatusHistory (一对多)
            modelBuilder.Entity<StationStatusHistory>()
                .HasOne<Station>()
                .WithMany()
                .HasForeignKey("StationId")
                .OnDelete(DeleteBehavior.Cascade);

            // Equipment -> EquipmentStatusHistory (一对多)
            // 已在 EquipmentConfiguration 中通过 HasMany(e => e.StatusHistory) 配置
            // 此处不再重复配置，避免冲突
        }

        private void ConfigureGlobalFilters(ModelBuilder modelBuilder)
        {
            // 软删除过滤器
            modelBuilder.Entity<Operator>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<Station>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Equipment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Connector>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Wallet>().HasQueryFilter(w => !w.IsDeleted);
            modelBuilder.Entity<Session>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Transaction>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<UserFavorite>().HasQueryFilter(f => !f.IsDeleted);
            modelBuilder.Entity<Vehicle>().HasQueryFilter(v => !v.IsDeleted);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 自动设置更新时间
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var baseEntity = (BaseEntity)entityEntry.Entity;

                // 使用 Update 方法设置更新时间
                baseEntity.Update();

                if (entityEntry.State == EntityState.Added)
                {
                    // CreatedAt 只能在构造函数或方法内部设置，不能直接赋值
                    // 可通过反射设置 protected 属性
                    typeof(BaseEntity)
                        .GetProperty(nameof(BaseEntity.CreatedAt), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        ?.SetValue(baseEntity, DateTime.UtcNow);
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }

    // DateTime UTC 转换器
    public class DateTimeUtcConverter : ValueConverter<DateTime, DateTime>
    {
        public DateTimeUtcConverter() : base(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }
}