using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ChargingStationManagement.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Station> Stations { get; set; }
    public DbSet<Operator> Operators { get; set; }
    public DbSet<Equipment> Equipment { get; set; }
    public DbSet<Connector> Connectors { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Station
        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StationId).IsUnique();
            entity.Property(e => e.StationId).IsRequired();
            entity.Property(e => e.OperatorId).IsRequired();
            entity.OwnsOne(e => e.Address, a =>
            {
                a.Property(p => p.FullAddress).HasColumnName("Address");
                a.Property(p => p.City).HasColumnName("City");
                a.Property(p => p.Street).HasColumnName("Street");
            });
            entity.OwnsOne(e => e.Location, l =>
            {
                l.Property(p => p.Latitude).HasColumnName("Latitude");
                l.Property(p => p.Longitude).HasColumnName("Longitude");
            });
            entity.OwnsOne(e => e.ElectricityRate, r =>
            {
                r.Property(p => p.ElectricityRate).HasColumnName("ElectricityRate");
                r.Property(p => p.ServiceRate).HasColumnName("ElectricityServiceRate"); // separate
                r.Property(p => p.ParkRate).HasColumnName("ElectricityParkRate");
                r.Property(p => p.TimeRate).HasColumnName("ElectricityTimeRate");
            });
            entity.OwnsOne(e => e.ServiceRate, r =>
            {
                r.Property(p => p.ElectricityRate).HasColumnName("ServiceElectricityRate");
                r.Property(p => p.ServiceRate).HasColumnName("ServiceServiceRate");
                r.Property(p => p.ParkRate).HasColumnName("ServiceParkRate");
                r.Property(p => p.TimeRate).HasColumnName("ServiceTimeRate");
            });
            entity.OwnsOne(e => e.ParkRate, r =>
            {
                r.Property(p => p.ElectricityRate).HasColumnName("ParkElectricityRate");
                r.Property(p => p.ServiceRate).HasColumnName("ParkServiceRate");
                r.Property(p => p.ParkRate).HasColumnName("ParkParkRate");
                r.Property(p => p.TimeRate).HasColumnName("ParkTimeRate");
            });
            entity.OwnsMany(e => e.StatusHistory, h =>
            {
                h.WithOwner().HasForeignKey("StationId");
                h.Property<int>("Id");
                h.HasKey("Id");
                h.Property(p => p.Status).HasColumnName("Status");
                h.Property(p => p.Reason).HasColumnName("Reason");
                h.Property(p => p.ChangeTime).HasColumnName("ChangeTime");
            });
            entity.HasMany(e => e.Equipment)
                  .WithOne()
                  .HasForeignKey(e => e.StationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Operator
        modelBuilder.Entity<Operator>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OperatorId).IsUnique();
        });

        // Equipment
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EquipmentId).IsUnique();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasMany(e => e.Connectors)
                  .WithOne()
                  .HasForeignKey(c => c.EquipmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Connector
        modelBuilder.Entity<Connector>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConnectorId).IsUnique();
            entity.Property(e => e.Standard).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.ParkStatus).HasConversion<int>();
            entity.Property(e => e.LockStatus).HasConversion<int>();
        });

        // Session
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SessionId).IsUnique();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.OrderStatus).HasConversion<int>();
            entity.OwnsOne(e => e.AppliedRates, r =>
            {
                r.Property(p => p.ElectricityRate).HasColumnName("AppliedElectricityRate");
                r.Property(p => p.ServiceRate).HasColumnName("AppliedServiceRate");
                r.Property(p => p.ParkRate).HasColumnName("AppliedParkRate");
                r.Property(p => p.TimeRate).HasColumnName("AppliedTimeRate");
            });
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.Status)
         .HasConversion<int>(); // store as int

            entity.Property(e => e.RegisteredAt)
                  .IsRequired();

            entity.Property(e => e.ApprovedBy)
                  .HasMaxLength(100);

            entity.Property(e => e.RejectionReason)
                  .HasMaxLength(500);
        });
        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        });
        // UserRole (many-to-many join)
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.AssignedBy).IsRequired().HasMaxLength(100);
        });

        // Wallet
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WalletId).IsUnique();
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TransactionId).IsUnique();
        });
    }
}