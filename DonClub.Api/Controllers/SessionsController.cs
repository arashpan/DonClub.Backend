using Donclub.Application.Sessions;
using Microsoft.AspNetCore.Mvc;

namespace Donclub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "SuperUser,Admin,Manager")]  // فعلاً برای تست باز می‌ذاریم
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessions;

    public SessionsController(ISessionService sessions)
    {
        _sessions = sessions;
    }

    // -------------------- Query --------------------

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SessionDetailDto>> GetById(long id, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(id, ct);
        return session is null ? NotFound() : Ok(session);
    }

    // /api/sessions/by-branch?branchId=1&date=2025-11-30
    [HttpGet("by-branch")]
    public async Task<ActionResult<IReadOnlyList<SessionSummaryDto>>> GetByBranch(
        [FromQuery] int branchId,
        [FromQuery] DateOnly date,
        CancellationToken ct)
    {
        var list = await _sessions.GetByBranchAndDateAsync(branchId, date, ct);
        return Ok(list);
    }

    // -------------------- Create / Update / Status --------------------

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateSessionRequest request, CancellationToken ct)
    {
        var id = await _sessions.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult> Update(long id, [FromBody] UpdateSessionRequest request, CancellationToken ct)
    {
        await _sessions.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<ActionResult> Cancel(long id, CancellationToken ct)
    {
        await _sessions.CancelAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:long}/status")]
    public async Task<ActionResult> ChangeStatus(long id, [FromBody] ChangeSessionStatusRequest request, CancellationToken ct)
    {
        await _sessions.ChangeStatusAsync(id, request.Status, ct);
        return NoContent();
    }

    // -------------------- Players --------------------

    [HttpPost("{id:long}/players")]
    public async Task<ActionResult> AddPlayer(long id, [FromBody] AddPlayerToSessionRequest request, CancellationToken ct)
    {
        await _sessions.AddPlayerAsync(id, request.PlayerId, ct);
        return NoContent();
    }

    [HttpDelete("{id:long}/players/{playerId:long}")]
    public async Task<ActionResult> RemovePlayer(long id, long playerId, CancellationToken ct)
    {
        await _sessions.RemovePlayerAsync(id, playerId, ct);
        return NoContent();
    }

    // -------------------- Scores --------------------

    [HttpPost("{id:long}/players/score")]
    public async Task<ActionResult> SetScore(long id, [FromBody] SetPlayerScoreRequest request, CancellationToken ct)
    {
        await _sessions.SetPlayerScoreAsync(id, request.PlayerId, request.Score, ct);
        return NoContent();
    }
}
