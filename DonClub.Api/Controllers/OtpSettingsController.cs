using Donclub.Application.Settings;
using Donclub.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Donclub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.SuperUserOrAdmin)]
public class OtpSettingsController : ControllerBase
{
    private readonly IOtpSettingsService _otpSettings;

    public OtpSettingsController(IOtpSettingsService otpSettings)
    {
        _otpSettings = otpSettings;
    }

    // GET /api/otpsettings/rate-limit
    [HttpGet("rate-limit")]
    public async Task<ActionResult<OtpRateLimitConfigDto>> GetRateLimit(CancellationToken ct)
    {
        var config = await _otpSettings.GetOtpRateLimitAsync(ct);
        return Ok(config);
    }

    // PUT /api/otpsettings/rate-limit
    [HttpPut("rate-limit")]
    public async Task<ActionResult> UpdateRateLimit([FromBody] OtpRateLimitConfigDto request, CancellationToken ct)
    {
        await _otpSettings.UpdateOtpRateLimitAsync(request, ct);
        return NoContent();
    }
}
