using Donclub.Domain.Common;
using Donclub.Domain.Users;

namespace Donclub.Domain.Notifications;

public enum NotificationType : byte
{
    General = 0,
    MissionCompleted = 1,
    BadgeGranted = 2,
    WalletCredited = 3,
    IncidentCreated = 4,
    IncidentResolved = 5,
    SessionUpdated = 6,
    SessionCanceled = 7
}


public class Notification : IAuditableEntity
{
    public long Id { get; set; }

    public long UserId { get; set; }
    public User User { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public NotificationType Type { get; set; }

    /// <summary>
    /// JSON payload for extra data (e.g. missionId, badgeId, amount, etc.)
    /// </summary>
    public string? DataJson { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
