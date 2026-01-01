using Donclub.Domain.Auth;
using Donclub.Domain.Badges;
using Donclub.Domain.Common;
using Donclub.Domain.Games;
using Donclub.Domain.Incidents;
using Donclub.Domain.Missions;
using Donclub.Domain.Notifications;
using Donclub.Domain.Sessions;
using Donclub.Domain.Settings;
using Donclub.Domain.Stadium;
using Donclub.Domain.Users;
using Donclub.Domain.Wallets;
using Microsoft.EntityFrameworkCore;

namespace Donclub.Infrastructure.Persistence;

public class DonclubDbContext : DbContext
{
    public DonclubDbContext(DbContextOptions<DonclubDbContext> options) : base(options) { }
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SmsOtp> SmsOtps => Set<SmsOtp>();
    // Stadium
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Room> Rooms => Set<Room>();

    // Games
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameRole> GameRoles => Set<GameRole>();
    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<ScenarioRole> ScenarioRoles => Set<ScenarioRole>();

    // Sessions
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionPlayer> SessionPlayers => Set<SessionPlayer>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<ScoreAudit> ScoreAudits => Set<ScoreAudit>();

    //Wallet
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

    // Badges
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<PlayerBadge> PlayerBadges => Set<PlayerBadge>();

    // Incidents
    public DbSet<Incident> Incidents => Set<Incident>();

    // Missions
    public DbSet<MissionDefinition> MissionDefinitions => Set<MissionDefinition>();
    public DbSet<UserMission> UserMissions => Set<UserMission>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("app");

        b.Entity<Notification>(e =>
        {
            e.ToTable("Notifications", "app");

            e.HasKey(x => x.Id);

            e.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            e.Property(x => x.Message)
                .HasMaxLength(1000)
                .IsRequired();

            e.Property(x => x.Type)
                .IsRequired();

            e.Property(x => x.DataJson)
                .HasColumnType("nvarchar(max)");

            e.Property(x => x.IsRead)
                .IsRequired();

            e.Property(x => x.CreatedAtUtc)
                .IsRequired();

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Missions
        b.Entity<MissionDefinition>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Code).HasMaxLength(100);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.RewardDescription).HasMaxLength(500);
            e.Property(x => x.RewardWalletAmount).HasColumnType("decimal(18,2)");

            e.HasIndex(x => x.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
        });

        b.Entity<UserMission>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasOne(x => x.MissionDefinition)
                .WithMany(m => m.UserMissions)
                .HasForeignKey(x => x.MissionDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // یک مأموریت، در یک بازه‌ی زمانی، برای یک کاربر، یک بار
            e.HasIndex(x => new { x.UserId, x.MissionDefinitionId, x.PeriodStartUtc, x.PeriodEndUtc })
                .IsUnique();
        });


        // Incidents
        b.Entity<Incident>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            e.Property(x => x.Description)
                .IsRequired();

            e.Property(x => x.ReviewNote)
                .HasMaxLength(1000);

            e.HasOne(x => x.Manager)
                .WithMany()
                .HasForeignKey(x => x.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Session)
                .WithMany()
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ReviewedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // Badges
        b.Entity<Badge>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Code).HasMaxLength(100);
            e.Property(x => x.RewardWalletAmount).HasColumnType("decimal(18,2)");

            e.HasIndex(x => x.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
        });

        b.Entity<PlayerBadge>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Reason).HasMaxLength(500);

            e.HasOne(x => x.Badge)
                .WithMany(bd => bd.PlayerBadges)
                .HasForeignKey(x => x.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // جلوگیری از گرفتن یک Badge چند بار (اگر نخواهیم تکراری باشد)
            e.HasIndex(x => new { x.UserId, x.BadgeId }).IsUnique();
        });

        // Wallet
        b.Entity<Wallet>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Balance)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            e.HasIndex(x => x.UserId).IsUnique();

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<WalletTransaction>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            e.Property(x => x.BalanceAfter)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            e.Property(x => x.Description)
                .HasMaxLength(500);

            e.HasOne(x => x.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
        });



        // User
        b.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserName).IsUnique();
            e.HasIndex(x => x.PhoneNumber).IsUnique();
			e.Property(x => x.UserCode).IsRequired().HasMaxLength(6);
			e.HasIndex(x => x.UserCode).IsUnique();
			e.Property(x => x.MembershipLevel).HasConversion<byte>();
            e.HasQueryFilter(x => !x.IsDeleted);

		});

        // Role
        b.Entity<Role>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name).IsUnique();
        });

        // Branch & Room
        b.Entity<Branch>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(150);
        });

        b.Entity<Room>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(x => new { x.BranchId, x.Name }).IsUnique();

            e.HasOne(x => x.Branch)
                .WithMany(b => b.Rooms)
                .HasForeignKey(x => x.BranchId);
        });

        // Games
        b.Entity<Game>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<GameRole>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);

            // GameId == NULL  -> global/shared role
            // GameId != NULL  -> game-specific role
            // Ensure uniqueness within each scope.
            e.HasIndex(x => new { x.GameId, x.Name })
                .IsUnique()
                .HasFilter("[GameId] IS NOT NULL");

            e.HasIndex(x => x.Name)
                .IsUnique()
                .HasFilter("[GameId] IS NULL");

            e.HasOne(x => x.Game)
                .WithMany(g => g.Roles)
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Scenario>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(x => new { x.GameId, x.Name }).IsUnique();

            e.HasOne(x => x.Game)
                .WithMany(g => g.Scenarios)
                .HasForeignKey(x => x.GameId);
        });

        b.Entity<ScenarioRole>(e =>
        {
            e.HasKey(x => new { x.ScenarioId, x.GameRoleId });

            e.HasOne(x => x.Scenario)
                .WithMany(s => s.ScenarioRoles)
                .HasForeignKey(x => x.ScenarioId)
                .OnDelete(DeleteBehavior.NoAction);   // 👈 مهم

            e.HasOne(x => x.GameRole)
                .WithMany()
                .HasForeignKey(x => x.GameRoleId)
                .OnDelete(DeleteBehavior.NoAction);   // 👈 مهم
        });


        // Sessions
        b.Entity<Session>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.RowVersion).IsRowVersion();

            e.HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);   // 👈 مهم

            e.HasOne(x => x.Room)
                .WithMany()
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);   // 👈 مهم

            e.HasOne(x => x.Game)
                .WithMany()
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Restrict);   // 👈 مهم

            e.HasOne(x => x.Scenario)
                .WithMany()
                .HasForeignKey(x => x.ScenarioId)
                .OnDelete(DeleteBehavior.NoAction);   // قبلاً NoAction گذاشته بودیم یا بذار

            e.HasOne(x => x.Manager)
                .WithMany()
                .HasForeignKey(x => x.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);   // برای اطمینان

            e.HasIndex(x => new { x.BranchId, x.StartTimeUtc });
            e.HasIndex(x => new { x.RoomId, x.StartTimeUtc, x.EndTimeUtc });
        });


        b.Entity<SessionPlayer>(e =>
        {
            e.HasKey(x => new { x.SessionId, x.PlayerId });

            e.HasOne(x => x.Session)
                .WithMany(s => s.Players)
                .HasForeignKey(x => x.SessionId);

            e.HasOne(x => x.Player)
                .WithMany()
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        b.Entity<Score>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.SessionId, x.PlayerId }).IsUnique();

            e.HasOne(x => x.Session)
                .WithMany()
                .HasForeignKey(x => x.SessionId);

            e.HasOne(x => x.Player)
                .WithMany()
                .HasForeignKey(x => x.PlayerId)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(x => x.EnteredBy)
                .WithMany()
                .HasForeignKey(x => x.EnteredByManagerId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        b.Entity<ScoreAudit>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Score)
                .WithMany()
                .HasForeignKey(x => x.ScoreId);
        });


        // UserRole
        b.Entity<UserRole>(e =>
        {
            e.HasKey(x => new { x.UserId, x.RoleId });
            e.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
        });

        // RefreshToken
        b.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.Token }).IsUnique();
        });

        // SmsOtp
        b.Entity<SmsOtp>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.PhoneNumber, x.Code });
        });

        // SystemSettings
        b.Entity<SystemSetting>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Key)
                .IsRequired()
                .HasMaxLength(200);

            e.Property(x => x.Value)
                .HasMaxLength(1000);

            e.Property(x => x.Description)
                .HasMaxLength(500);

            e.HasIndex(x => x.Key).IsUnique();
        });


        // Seed roles پایه
        b.Entity<Role>().HasData(
            new Role { Id = 1, Name = "SuperUser" },
            new Role { Id = 2, Name = "Admin" },
            new Role { Id = 3, Name = "Manager" },
            new Role { Id = 4, Name = "Player" },
            new Role { Id = 5, Name = "Operator" }
        );

        base.OnModelCreating(b);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAtUtc = now;
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAtUtc = now;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
