using Donclub.Domain.Common;
using Donclub.Domain.Games;
using Donclub.Domain.Stadium;
using Donclub.Domain.Users;

namespace Donclub.Domain.Sessions;

public enum SessionStatus : byte
{
    Planned = 0,
    Live = 1,
    Paused = 2,
    Ended = 3,
    Canceled = 4
}

public enum SessionTier : byte
{
    Normal = 0,
    Vip = 1,
    Cip = 2
}

public class Session : BaseEntity<long>, IAuditableEntity
{
    public int BranchId { get; set; }
    public int RoomId { get; set; }
    public int GameId { get; set; }
    public int? ScenarioId { get; set; }
    public long? ManagerId { get; set; }   // User با نقش Manager

    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Planned;
    public SessionTier Tier { get; set; } = SessionTier.Normal;

    public byte? MaxPlayers { get; set; }
    public string? Notes { get; set; }
    public byte ChangeRequestStatus { get; set; } = 0; // 0=none,1=pending,...

    public byte[]? RowVersion { get; set; }  // concurrency

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public Branch Branch { get; set; } = default!;
    public Room Room { get; set; } = default!;
    public Game Game { get; set; } = default!;
    public Scenario? Scenario { get; set; }
    public User? Manager { get; set; }

    public ICollection<SessionPlayer> Players { get; set; } = new List<SessionPlayer>();
}

public enum SessionPlayerStatus : byte
{
    Registered = 0,
    CheckedIn = 1,
    NoShow = 2,
    Canceled = 3
}

public class SessionPlayer
{
    public long SessionId { get; set; }
    public long PlayerId { get; set; }
    public DateTime ReservedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CheckInAtUtc { get; set; }
    public SessionPlayerStatus Status { get; set; } = SessionPlayerStatus.Registered;

    public Session Session { get; set; } = default!;
    public User Player { get; set; } = default!;
}

public class Score : BaseEntity<long>, IAuditableEntity
{
    public long SessionId { get; set; }
    public long PlayerId { get; set; }
    public int Value { get; set; }

    public long EnteredByManagerId { get; set; }

    public DateTime EnteredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastEditedAtUtc { get; set; }
    public bool IsLocked { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public Session Session { get; set; } = default!;
    public User Player { get; set; } = default!;
    public User EnteredBy { get; set; } = default!;
}

public class ScoreAudit : BaseEntity<long>
{
    public long ScoreId { get; set; }
    public int? OldValue { get; set; }
    public int NewValue { get; set; }
    public long ChangedById { get; set; }
    public string? Reason { get; set; }
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;

    public Score Score { get; set; } = default!;
}
