namespace Donclub.Application.Settings;

public interface ISmsProviderSettingsService
{
    Task<IReadOnlyList<SmsProviderSettingDto>> GetAllAsync(CancellationToken ct = default);
    Task<SmsProviderSettingDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SmsProviderSettingDto?> GetActiveAsync(CancellationToken ct = default);
    Task<SmsProviderSettingDto> CreateAsync(CreateSmsProviderSettingRequest request, CancellationToken ct = default);
    Task<SmsProviderSettingDto> UpdateAsync(int id, UpdateSmsProviderSettingRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task SetActiveAsync(int id, CancellationToken ct = default);
}
