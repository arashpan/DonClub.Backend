using System.Security.Claims;
using Donclub.Application.Profile;
using Donclub.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Donclub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profiles;

    public ProfileController(IProfileService profiles)
    {
        _profiles = profiles;
    }

    // GET /api/profile/me  → پروفایل خود کاربر لاگین‌شده
    [HttpGet("me")]
    [Authorize] // هر کاربر لاگین شده (Player, Manager, Admin, SuperUser)
    public async Task<ActionResult<ProfileDto>> GetMyProfile(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();

        if (!long.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var profile = await _profiles.GetProfileAsync(userId, ct);
        if (profile is null)
            return NotFound();

        return Ok(profile);
    }

    // GET /api/profile/user/{userId}  → برای Admin / SuperUser
    [HttpGet("user/{userId:long}")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult<ProfileDto>> GetUserProfile(long userId, CancellationToken ct)
    {
        var profile = await _profiles.GetProfileAsync(userId, ct);
        if (profile is null)
            return NotFound();

        return Ok(profile);
    }
}
