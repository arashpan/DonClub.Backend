using Donclub.Domain.Common;

namespace Donclub.Domain.Settings;

public class SystemSetting : BaseEntity<int>, IAuditableEntity
{
    public string Key { get; set; } = default!;      // مثل "Auth:Otp:MaxRequestsPerWindow"
    public string? Value { get; set; }              // مقدار به صورت string (خودمون parse می‌کنیم)
    public string? Description { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
