using Donclub.Application.Settings;
using Donclub.Domain.Settings;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Donclub.Infrastructure.Settings;

public class SmsProviderSettingsService : ISmsProviderSettingsService
{
    private readonly DonclubDbContext _db;

    public SmsProviderSettingsService(DonclubDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SmsProviderSettingDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _db.SmsProviderSettings
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        return list.Select(Map).ToList();
    }

    public async Task<SmsProviderSettingDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.SmsProviderSettings.FirstOrDefaultAsync(x => x.Id == id, ct);
        return entity == null ? null : Map(entity);
    }

    public async Task<SmsProviderSettingDto?> GetActiveAsync(CancellationToken ct = default)
    {
        var entity = await _db.SmsProviderSettings.FirstOrDefaultAsync(x => x.IsActive, ct);
        return entity == null ? null : Map(entity);
    }

    public async Task<SmsProviderSettingDto> CreateAsync(CreateSmsProviderSettingRequest request, CancellationToken ct = default)
    {
        var entity = new SmsProviderSetting
        {
            Provider = request.Provider?.Trim() ?? "Melipayamak",
            ApiBaseUrl = request.ApiBaseUrl?.Trim() ?? "https://rest.payamak-panel.com/api/SendSMS/",
            Username = request.Username?.Trim() ?? string.Empty,
            Password = request.Password ?? string.Empty,
            FromNumber = request.FromNumber?.Trim() ?? string.Empty,
            UseBaseServiceNumber = request.UseBaseServiceNumber,
            BodyId = request.BodyId,
            IsFlash = request.IsFlash,
            IsEnabled = request.IsEnabled,
            IsActive = request.IsActive,
            Description = request.Description
        };

        if (entity.IsActive)
            await DeactivateAllAsync(ct);

        _db.SmsProviderSettings.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<SmsProviderSettingDto> UpdateAsync(int id, UpdateSmsProviderSettingRequest request, CancellationToken ct = default)
    {
        var entity = await _db.SmsProviderSettings.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("تنظیمات پیامک‌رسان یافت نشد.");

        entity.Provider = request.Provider?.Trim() ?? entity.Provider;
        entity.ApiBaseUrl = request.ApiBaseUrl?.Trim() ?? entity.ApiBaseUrl;
        entity.Username = request.Username?.Trim() ?? entity.Username;
        if (request.Password is not null)
            entity.Password = request.Password;

        entity.FromNumber = request.FromNumber?.Trim() ?? entity.FromNumber;
        entity.UseBaseServiceNumber = request.UseBaseServiceNumber;
        entity.BodyId = request.BodyId;
        entity.IsFlash = request.IsFlash;
        entity.IsEnabled = request.IsEnabled;

        if (request.IsActive && !entity.IsActive)
        {
            await DeactivateAllAsync(ct);
            entity.IsActive = true;
        }
        else if (!request.IsActive)
        {
            entity.IsActive = false;
        }

        entity.Description = request.Description;

        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.SmsProviderSettings.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("تنظیمات پیامک‌رسان یافت نشد.");

        _db.SmsProviderSettings.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SetActiveAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.SmsProviderSettings.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("تنظیمات پیامک‌رسان یافت نشد.");

        await DeactivateAllAsync(ct);
        entity.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    private async Task DeactivateAllAsync(CancellationToken ct)
    {
        var actives = await _db.SmsProviderSettings.Where(x => x.IsActive).ToListAsync(ct);
        foreach (var s in actives)
            s.IsActive = false;
    }

    private static SmsProviderSettingDto Map(SmsProviderSetting x)
        => new(
            x.Id,
            x.Provider,
            x.ApiBaseUrl,
            x.Username,
            x.FromNumber,
            x.UseBaseServiceNumber,
            x.BodyId,
            x.IsFlash,
            x.IsEnabled,
            x.IsActive,
            PasswordIsSet: !string.IsNullOrWhiteSpace(x.Password),
            x.Description,
            x.CreatedAtUtc,
            x.UpdatedAtUtc
        );
}
