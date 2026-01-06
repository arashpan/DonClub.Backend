namespace Donclub.Application.Settings;

public record SmsProviderSettingDto(
    int Id,
    string Provider,
    string ApiBaseUrl,
    string Username,
    string FromNumber,
    bool UseBaseServiceNumber,
    int? BodyId,
    bool IsFlash,
    bool IsEnabled,
    bool IsActive,
    bool PasswordIsSet,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record CreateSmsProviderSettingRequest(
    string Provider,
    string ApiBaseUrl,
    string Username,
    string Password,
    string FromNumber,
    bool UseBaseServiceNumber,
    int? BodyId,
    bool IsFlash,
    bool IsEnabled,
    bool IsActive,
    string? Description
);

/// <summary>
/// Password اگر null باشد تغییر نمی‌کند.
/// </summary>
public record UpdateSmsProviderSettingRequest(
    string Provider,
    string ApiBaseUrl,
    string Username,
    string? Password,
    string FromNumber,
    bool UseBaseServiceNumber,
    int? BodyId,
    bool IsFlash,
    bool IsEnabled,
    bool IsActive,
    string? Description
);
