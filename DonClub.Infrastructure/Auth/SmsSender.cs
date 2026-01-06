using Donclub.Application.Auth;
using Donclub.Domain.Settings;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Donclub.Infrastructure.Auth;

/// <summary>
/// ارسال پیامک از طریق ملی پیامک (Melipayamak) بر اساس تنظیمات دیتابیس.
/// </summary>
public class SmsSender : ISmsSender
{
    private readonly DonclubDbContext _db;
    private readonly ILogger<SmsSender> _logger;

    // HttpClient را به صورت static نگه می‌داریم تا از Socket Exhaustion جلوگیری شود.
    private static readonly HttpClient Http = new();

    public SmsSender(DonclubDbContext db, ILogger<SmsSender> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SendAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        var settings = await _db.SmsProviderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive, ct);

        if (settings == null)
        {
            _logger.LogWarning("SmsProviderSettings فعال یافت نشد. پیامک ارسال نشد. To={To}", phoneNumber);
            return;
        }

        if (!settings.IsEnabled)
        {
            _logger.LogInformation("ارسال پیامک غیرفعال است (SmsProviderSettings.IsEnabled=false). To={To}", phoneNumber);
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.Username) || string.IsNullOrWhiteSpace(settings.Password))
            throw new InvalidOperationException("نام کاربری/رمز عبور پیامک‌رسان تنظیم نشده است.");

        var to = NormalizeToIranMobile(settings, phoneNumber);

        var op = settings.UseBaseServiceNumber ? "BaseServiceNumber" : "SendSMS";
        var url = Combine(settings.ApiBaseUrl, op);

        // Melipayamak نمونه‌ها: ارسال FormUrlEncoded.
        var form = new List<KeyValuePair<string, string>>
        {
            new("username", settings.Username),
            new("password", settings.Password)
        };

        if (settings.UseBaseServiceNumber)
        {
            if (settings.BodyId is null)
                throw new InvalidOperationException("برای UseBaseServiceNumber باید BodyId تنظیم شود.");

            form.Add(new("text", message));
            form.Add(new("to", to));
            form.Add(new("bodyId", settings.BodyId.Value.ToString()));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(settings.FromNumber))
                throw new InvalidOperationException("برای ارسال ساده باید FromNumber تنظیم شود.");

            form.Add(new("to", to));
            form.Add(new("from", settings.FromNumber));
            form.Add(new("text", message));
            form.Add(new("isFlash", settings.IsFlash.ToString()));
        }

        using var content = new FormUrlEncodedContent(form);

        HttpResponseMessage resp;
        try
        {
            resp = await Http.PostAsync(url, content, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در ارتباط با پیامک‌رسان. Url={Url}", url);
            throw;
        }

        var body = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("ارسال پیامک ناموفق. Status={Status} Body={Body}", (int)resp.StatusCode, body);
            throw new InvalidOperationException("ارسال پیامک ناموفق بود.");
        }

        // پاسخ در نمونه‌های رسمی: { Value, RetStatus, StrRetStatus }
        MelipayamakRestResponse? parsed = null;
        try
        {
            parsed = JsonSerializer.Deserialize<MelipayamakRestResponse>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "پاسخ پیامک‌رسان قابل Deserialize نیست. Body={Body}", body);
        }

        if (parsed != null && parsed.RetStatus != 1)
        {
            _logger.LogError("ارسال پیامک با خطا برگشت. RetStatus={RetStatus} StrRetStatus={StrRetStatus} Value={Value}",
                parsed.RetStatus, parsed.StrRetStatus, parsed.Value);
            throw new InvalidOperationException($"ارسال پیامک با خطا برگشت: {parsed.StrRetStatus} (RetStatus={parsed.RetStatus})");
        }
    }

    private static string NormalizeToIranMobile(SmsProviderSetting _settings, string input)
    {
        // پروژه فعلاً شماره را به شکل 98xxxxxxxxxx ذخیره می‌کند.
        // ملی پیامک در نمونه‌ها شماره 09xxxxxxxxx را استفاده می‌کند.
        var phone = input.Trim();
        if (phone.StartsWith("+")) phone = phone[1..];

        if (phone.StartsWith("98") && phone.Length == 12)
            return "0" + phone[2..];

        if (phone.StartsWith("9") && phone.Length == 10)
            return "0" + phone;

        return phone;
    }

    private static string Combine(string baseUrl, string path)
    {
        baseUrl = (baseUrl ?? string.Empty).Trim();
        if (!baseUrl.EndsWith("/")) baseUrl += "/";
        return baseUrl + path;
    }

    private sealed class MelipayamakRestResponse
    {
        public string? Value { get; set; }
        public int RetStatus { get; set; }
        public string? StrRetStatus { get; set; }
    }
}
