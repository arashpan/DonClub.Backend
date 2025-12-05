using Donclub.Application.Settings;

public interface IOtpSettingsService
{
    Task<OtpRateLimitConfigDto> GetOtpRateLimitAsync(CancellationToken ct = default);
    Task UpdateOtpRateLimitAsync(OtpRateLimitConfigDto config, CancellationToken ct = default);
}
