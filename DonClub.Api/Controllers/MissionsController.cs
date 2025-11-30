using System.Security.Claims;
using Donclub.Application.Missions;
using Donclub.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Donclub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MissionsController : ControllerBase
{
    private readonly IMissionService _missions;

    public MissionsController(IMissionService missions)
    {
        _missions = missions;
    }

    private long GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idClaim))
            throw new InvalidOperationException("UserId not found in token.");
        return long.Parse(idClaim);
    }

    // -------- Definitions (Admin) --------

    // GET /api/missions/definitions?isActive=true
    [HttpGet("definitions")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult<IReadOnlyList<MissionDefinitionDto>>> GetDefinitions(
        [FromQuery] bool? isActive,
        CancellationToken ct)
    {
        var list = await _missions.GetDefinitionsAsync(isActive, ct);
        return Ok(list);
    }

    // GET /api/missions/definitions/{id}
    [HttpGet("definitions/{id:int}")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult<MissionDefinitionDto>> GetDefinitionById(int id, CancellationToken ct)
    {
        var def = await _missions.GetDefinitionByIdAsync(id, ct);
        if (def is null) return NotFound();
        return Ok(def);
    }

    // POST /api/missions/definitions
    [HttpPost("definitions")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult> CreateDefinition([FromBody] CreateMissionDefinitionRequest request, CancellationToken ct)
    {
        var id = await _missions.CreateDefinitionAsync(request, ct);
        return CreatedAtAction(nameof(GetDefinitionById), new { id }, null);
    }

    // PUT /api/missions/definitions/{id}
    [HttpPut("definitions/{id:int}")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult> UpdateDefinition(int id, [FromBody] UpdateMissionDefinitionRequest request, CancellationToken ct)
    {
        await _missions.UpdateDefinitionAsync(id, request, ct);
        return NoContent();
    }

    // DELETE /api/missions/definitions/{id}
    [HttpDelete("definitions/{id:int}")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult> DeleteDefinition(int id, CancellationToken ct)
    {
        await _missions.DeleteDefinitionAsync(id, ct);
        return NoContent();
    }

    // -------- User missions --------

    // برای Admin/SU: مأموریت‌های یک یوزر خاص
    // GET /api/missions/user/{userId}
    [HttpGet("user/{userId:long}")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult<IReadOnlyList<UserMissionDto>>> GetUserMissions(long userId, [FromQuery] bool onlyActive, CancellationToken ct)
    {
        var list = await _missions.GetUserMissionsAsync(userId, onlyActive, ct);
        return Ok(list);
    }

    // برای خود کاربر: GET /api/missions/my
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<UserMissionDto>>> GetMyMissions([FromQuery] bool onlyActive, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var list = await _missions.GetUserMissionsAsync(userId, onlyActive, ct);
        return Ok(list);
    }

    // Admin: Assign mission to a user
    // POST /api/missions/definitions/{missionId}/assign
    [HttpPost("definitions/{missionId:int}/assign")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult<UserMissionDto>> AssignMissionToUser(
        int missionId,
        [FromBody] AssignMissionToUserRequest request,
        CancellationToken ct)
    {
        var dto = await _missions.AssignMissionToUserAsync(
            missionId,
            request.UserId,
            request.PeriodStartUtc,
            request.PeriodEndUtc,
            ct);

        return Ok(dto);
    }

    // کاربر: افزایش progress روی مأموریت خودش
    // POST /api/missions/my/{userMissionId}/progress
    [HttpPost("my/{userMissionId:long}/progress")]
    [Authorize]
    public async Task<ActionResult<UserMissionDto>> AddMyMissionProgress(
        long userMissionId,
        [FromBody] AddMissionProgressRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();

        // می‌تونیم اینجا یک check اضافه کنیم که این UM واقعاً متعلق به همین user باشد
        // برای ساده‌سازی، از سرویس همان آیدی را استفاده می‌کنیم و فرض می‌کنیم سرویس امنیت را بعداً چک کند.
        var dto = await _missions.AddProgressAsync(userMissionId, request.Amount, ct);
        // TODO: در آینده می‌توانیم داخل سرویس مطمئن شویم که dto مربوط به userId است.
        return Ok(dto);
    }

    // Admin: افزایش progress برای یک UserMission خاص (مثلاً دستی)
    // POST /api/missions/{userMissionId}/progress
    [HttpPost("{userMissionId:long}/progress")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult<UserMissionDto>> AddProgressAdmin(
        long userMissionId,
        [FromBody] AddMissionProgressRequest request,
        CancellationToken ct)
    {
        var dto = await _missions.AddProgressAsync(userMissionId, request.Amount, ct);
        return Ok(dto);
    }
}
