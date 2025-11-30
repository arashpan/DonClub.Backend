using Donclub.Domain.Common;
using Donclub.Domain.Users;

namespace Donclub.Domain.Badges;

public class Badge : BaseEntity<int>, IAuditableEntity
{
    public string Name { get; set; } = default!;
    public string? Code { get; set; }        // unique key like "FIRST_WIN"
    public string? Description { get; set; }
    public string? IconUrl { get; set; }     // optional link to icon
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional JSON condition metadata (e.g. first_win, 100_sessions, VIP_only).
    /// The logic of when to grant can be implemented later.
    /// </summary>
    public string? ConditionJson { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<PlayerBadge> PlayerBadges { get; set; } = new List<PlayerBadge>();
}

public class PlayerBadge : BaseEntity<long>, IAuditableEntity
{
    public long UserId { get; set; }       // Player (User)
    public int BadgeId { get; set; }

    public DateTime EarnedAtUtc { get; set; } = DateTime.UtcNow;
    public long? GrantedByUserId { get; set; } // Admin/Manager who manually granted
    public string? Reason { get; set; }
    public bool IsRevoked { get; set; } = false;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public Badge Badge { get; set; } = default!;
    public User User { get; set; } = default!;
}
