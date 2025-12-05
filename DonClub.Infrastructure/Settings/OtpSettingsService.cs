using Donclub.Application.Settings;
using Donclub.Domain.Settings;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Donclub.Infrastructure.Settings;

public class OtpSettingsService : IOtpSettingsService
{
    private readonly DonclubDbContext _db;

    private const string MaxRequestsKey = "Auth:Otp:MaxRequestsPerWindow";
    private const string WindowMinutesKey = "Auth:Otp:WindowMinutes";
    private const string IsEnabledKey = "Auth:Otp:IsEnabled";
    private const bool DefaultIsEnabled = true;

    // مقادیر پیش‌فرض اگر ادمین چیزی تنظیم نکرده باشد
    private const int DefaultMaxRequests = 5;
    private const int DefaultWindowMinutes = 5;

    public OtpSettingsService(DonclubDbContext db)
    {
        _db = db;
    }

    public async Task<OtpRateLimitConfigDto> GetOtpRateLimitAsync(CancellationToken ct = default)
    {
        var keys = new[] { IsEnabledKey, MaxRequestsKey, WindowMinutesKey };

        var settings = await _db.SystemSettings
            .Where(s => keys.Contains(s.Key))
            .ToListAsync(ct);

        bool isEnabled = DefaultIsEnabled;
        int max = DefaultMaxRequests;
        int win = DefaultWindowMinutes;

        var isEnabledSetting = settings.FirstOrDefault(s => s.Key == IsEnabledKey);
        if (isEnabledSetting?.Value is not null &&
            bool.TryParse(isEnabledSetting.Value, out var parsedEnabled))
        {
            isEnabled = parsedEnabled;
        }

        var maxSetting = settings.FirstOrDefault(s => s.Key == MaxRequestsKey);
        if (maxSetting?.Value is not null &&
            int.TryParse(maxSetting.Value, out var parsedMax) && parsedMax > 0)
        {
            max = parsedMax;
        }

        var winSetting = settings.FirstOrDefault(s => s.Key == WindowMinutesKey);
        if (winSetting?.Value is not null &&
            int.TryParse(winSetting.Value, out var parsedWin) && parsedWin > 0)
        {
            win = parsedWin;
        }

        return new OtpRateLimitConfigDto(isEnabled, max, win);
    }


    public async Task UpdateOtpRateLimitAsync(OtpRateLimitConfigDto config, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // اگر Rate Limiting روشن است، باید مقادیر معتبر باشند
        if (config.IsEnabled)
        {
            if (config.MaxRequestsPerWindow <= 0 || config.WindowMinutes <= 0)
                throw new InvalidOperationException("مقادیر MaxRequestsPerWindow و WindowMinutes باید بزرگتر از صفر باشند.");
        }

        // IsEnabled
        var enabledSetting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == IsEnabledKey, ct);
        if (enabledSetting == null)
        {
            enabledSetting = new SystemSetting
            {
                Key = IsEnabledKey,
                Value = config.IsEnabled.ToString(),
                Description = "آیا Rate Limiting برای OTP فعال است؟",
                CreatedAtUtc = now
            };
            _db.SystemSettings.Add(enabledSetting);
        }
        else
        {
            enabledSetting.Value = config.IsEnabled.ToString();
            enabledSetting.UpdatedAtUtc = now;
        }

        // اگر خاموش شده، مقادیر عددی را هم می‌توانیم ذخیره کنیم (برای بعد)، ولی استفاده نمی‌شوند
        var maxSetting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == MaxRequestsKey, ct);
        if (maxSetting == null)
        {
            maxSetting = new SystemSetting
            {
                Key = MaxRequestsKey,
                Value = config.MaxRequestsPerWindow.ToString(),
                Description = "حداکثر تعداد درخواست OTP در بازه زمانی مشخص",
                CreatedAtUtc = now
            };
            _db.SystemSettings.Add(maxSetting);
        }
        else
        {
            maxSetting.Value = config.MaxRequestsPerWindow.ToString();
            maxSetting.UpdatedAtUtc = now;
        }

        var winSetting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == WindowMinutesKey, ct);
        if (winSetting == null)
        {
            winSetting = new SystemSetting
            {
                Key = WindowMinutesKey,
                Value = config.WindowMinutes.ToString(),
                Description = "طول بازه زمانی (دقیقه) برای محدودسازی OTP",
                CreatedAtUtc = now
            };
            _db.SystemSettings.Add(winSetting);
        }
        else
        {
            winSetting.Value = config.WindowMinutes.ToString();
            winSetting.UpdatedAtUtc = now;
        }

        await _db.SaveChangesAsync(ct);
    }

}
