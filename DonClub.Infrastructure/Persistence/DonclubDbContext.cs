using Donclub.Domain.Stadium;
using Donclub.Domain.Games;
using Donclub.Domain.Sessions;
using Donclub.Domain.Auth;
using Donclub.Domain.Common;
using Donclub.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Donclub.Infrastructure.Persistence;

public class DonclubDbContext : DbContext
{
    public DonclubDbContext(DbContextOptions<DonclubDbContext> options) : base(options) { }

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


    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("app");

        // User
        b.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserName).IsUnique();
            e.HasIndex(x => x.PhoneNumber).IsUnique();
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
            e.HasIndex(x => new { x.GameId, x.Name }).IsUnique();

            e.HasOne(x => x.Game)
                .WithMany(g => g.Roles)
                .HasForeignKey(x => x.GameId);
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

        // Seed roles پایه
        b.Entity<Role>().HasData(
            new Role { Id = 1, Name = "SuperUser" },
            new Role { Id = 2, Name = "Admin" },
            new Role { Id = 3, Name = "Manager" },
            new Role { Id = 4, Name = "Player" }
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
