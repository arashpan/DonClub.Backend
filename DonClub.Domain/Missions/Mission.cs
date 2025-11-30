using Donclub.Domain.Common;
using Donclub.Domain.Users;

namespace Donclub.Domain.Missions;

public enum MissionPeriod : byte
{
    OneTime = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3
}

public class MissionDefinition : BaseEntity<int>, IAuditableEntity
{
    public string Name { get; set; } = default!;
    public string? Code { get; set; }          // مثل "PLAY_3_SESSIONS"
    public string? Description { get; set; }

    public MissionPeriod Period { get; set; }  // روزانه، هفتگی، ...
    public int TargetValue { get; set; }       // مثلاً تعداد سشن، امتیاز، ...

    // برای آینده: به‌محض کامل شدن، چه پاداشی بدهیم (مثلاً امتیاز یا مبلغ کیف پول)
    public decimal? RewardWalletAmount { get; set; }  // در صورت نیاز
    public string? RewardDescription { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON برای شرط‌های پیچیده‌تر (مثل VIP-only، فقط بازی خاص و ...)
    /// فعلاً فقط ذخیره می‌کنیم، بعداً لاجیکشو اضافه می‌کنیم.
    /// </summary>
    public string? ConditionJson { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<UserMission> UserMissions { get; set; } = new List<UserMission>();
}

public class UserMission : BaseEntity<long>, IAuditableEntity
{
    public long UserId { get; set; }
    public int MissionDefinitionId { get; set; }

    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }

    public int CurrentValue { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public DateTime? LastProgressAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public MissionDefinition MissionDefinition { get; set; } = default!;
    public User User { get; set; } = default!;
}
