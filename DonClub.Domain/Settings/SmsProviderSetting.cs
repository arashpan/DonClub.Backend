using Donclub.Domain.Common;

namespace Donclub.Domain.Settings;

/// <summary>
/// تنظیمات پیامک‌رسان (فعلاً: ملی پیامک).
/// </summary>
public class SmsProviderSetting : BaseEntity<int>, IAuditableEntity
{
    /// <summary>
    /// نام سرویس‌دهنده (مثلاً Melipayamak)
    /// </summary>
    public string Provider { get; set; } = "Melipayamak";

    /// <summary>
    /// آدرس پایه Rest API
    /// نمونه: https://rest.payamak-panel.com/api/SendSMS/
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://rest.payamak-panel.com/api/SendSMS/";

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// شماره/خط ارسال‌کننده (برای ارسال ساده)
    /// نمونه: 5000xxxx
    /// </summary>
    public string FromNumber { get; set; } = string.Empty;

    /// <summary>
    /// اگر true باشد، بجای SendSMS از BaseServiceNumber استفاده می‌کنیم (خط خدماتی اشتراکی)
    /// </summary>
    public bool UseBaseServiceNumber { get; set; }

    /// <summary>
    /// BodyId مربوط به متن از پیش تعریف‌شده در ملی پیامک (برای BaseServiceNumber)
    /// </summary>
    public int? BodyId { get; set; }

    public bool IsFlash { get; set; }

    /// <summary>
    /// روشن/خاموش بودن ارسال پیامک از سمت بک‌اند
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// فقط یک تنظیم به عنوان «فعال» انتخاب می‌شود.
    /// </summary>
    public bool IsActive { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
