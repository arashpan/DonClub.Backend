namespace Donclub.Application.Settings;

public record OtpRateLimitConfigDto(
    bool IsEnabled,
    int MaxRequestsPerWindow,
    int WindowMinutes
);
