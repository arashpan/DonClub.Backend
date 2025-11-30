using Donclub.Application.Badges;
using Donclub.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Donclub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "SuperUser,Admin")]  // بعداً برای مدیریت Badge فعال می‌کنیم
public class BadgesController : ControllerBase
{
    private readonly IBadgeService _badges;

    public BadgesController(IBadgeService badges)
    {
        _badges = badges;
    }

    // ---------- Badges (Admin) ----------

    [HttpGet]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult<IReadOnlyList<BadgeDto>>> GetAll(CancellationToken ct)
    {
        var result = await _badges.GetAllBadgesAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult<BadgeDto>> GetById(int id, CancellationToken ct)
    {
        var badge = await _badges.GetBadgeByIdAsync(id, ct);
        if (badge is null) return NotFound();
        return Ok(badge);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult> Create([FromBody] CreateBadgeRequest request, CancellationToken ct)
    {
        var id = await _badges.CreateBadgeAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateBadgeRequest request, CancellationToken ct)
    {
        await _badges.UpdateBadgeAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        await _badges.DeleteBadgeAsync(id, ct);
        return NoContent();
    }

    // ---------- Player Badges ----------

    // GET /api/badges/user/5
    [HttpGet("user/{userId:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<PlayerBadgeDto>>> GetForUser(long userId, CancellationToken ct)
    {
        var list = await _badges.GetBadgesForUserAsync(userId, ct);
        return Ok(list);
    }

    // POST /api/badges/{badgeId}/grant
    [HttpPost("{badgeId:int}/grant")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult> Grant(int badgeId, [FromBody] GrantBadgeRequest request, CancellationToken ct)
    {
        // فعلاً grantedByUserId رو null می‌ذاریم؛ بعداً از JWT می‌خونیم
        var id = await _badges.GrantBadgeAsync(badgeId, request.UserId, request.Reason, null, ct);
        return Ok(new { playerBadgeId = id });
    }

    // POST /api/badges/revoke/{playerBadgeId}
    [HttpPost("revoke/{playerBadgeId:long}")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult> Revoke(long playerBadgeId, [FromBody] RevokeBadgeRequest request, CancellationToken ct)
    {
        await _badges.RevokeBadgeAsync(playerBadgeId, request.Reason, null, ct);
        return NoContent();
    }
}
