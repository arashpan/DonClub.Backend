using Donclub.Domain.Common;
using Donclub.Domain.Sessions;
using Donclub.Domain.Users;

namespace Donclub.Domain.Incidents;

public enum IncidentStatus : byte
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public enum IncidentSeverity : byte
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public class Incident : BaseEntity<long>, IAuditableEntity
{
    public long ManagerId { get; set; }           // منیجری که خطا برای او ثبت شده
    public long? SessionId { get; set; }          // اگر مربوط به یک سشن خاص است
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Pending;

    public long CreatedByUserId { get; set; }     // کسی که Incident را ثبت کرده (Admin, SuperUser, یا سیستم)
    public long? ReviewedByUserId { get; set; }   // Admin/SU که تصمیم نهایی گرفته
    public DateTime? ReviewedAtUtc { get; set; }
    public string? ReviewNote { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public User Manager { get; set; } = default!;
    public Session? Session { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public User? ReviewedByUser { get; set; }
}
