using System.Security.Claims;
using Donclub.Application.Incidents;
using Donclub.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Donclub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _incidents;

    public IncidentsController(IIncidentService incidents)
    {
        _incidents = incidents;
    }

    private long GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (idClaim == null)
            throw new InvalidOperationException("UserId not found in token.");

        return long.Parse(idClaim);
    }

    // ---------- Incident CRUD-like ----------

    // ایجاد Incident (مثلاً توسط Admin / SuperUser، یا حتی Manager ارشد)
    [HttpPost]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult> Create([FromBody] CreateIncidentRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var id = await _incidents.CreateAsync(request, currentUserId, ct);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    // بررسی Incident (Approve / Reject)
    [HttpPost("{id:long}/review")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult> Review(long id, [FromBody] ReviewIncidentRequest request, CancellationToken ct)
    {
        var reviewerId = GetCurrentUserId();
        await _incidents.ReviewAsync(id, reviewerId, request, ct);
        return NoContent();
    }

    // دریافت Incident بر اساس Id
    [HttpGet("{id:long}")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult<IncidentDetailDto>> GetById(long id, CancellationToken ct)
    {
        var dto = await _incidents.GetByIdAsync(id, ct);
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    // لیست Incident های Pending برای بررسی
    [HttpGet("pending")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult<IReadOnlyList<IncidentSummaryDto>>> GetPending(CancellationToken ct)
    {
        var list = await _incidents.GetPendingAsync(ct);
        return Ok(list);
    }

    // Incident های مربوط به یک Manager (Admin/SU می‌بینند)
    [HttpGet("manager/{managerId:long}")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult<IReadOnlyList<IncidentSummaryDto>>> GetForManager(long managerId, CancellationToken ct)
    {
        var list = await _incidents.GetByManagerAsync(managerId, ct);
        return Ok(list);
    }

    // Incident های Manager خودش (برای پنل منیجر)
    [HttpGet("my")]
    [Authorize(Roles = AppRoles.ManagerOrAbove)]
    public async Task<ActionResult<IReadOnlyList<IncidentSummaryDto>>> GetMyIncidents(CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        var list = await _incidents.GetByManagerAsync(currentUserId, ct);
        return Ok(list);
    }

    // ---------- Manager KPI ----------

    // KPI برای یک Manager (از دید Admin/SU)
    [HttpGet("manager/{managerId:long}/kpi")]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult<ManagerKpiDto>> GetManagerKpi(
        long managerId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        CancellationToken ct)
    {
        var dto = await _incidents.GetManagerKpiAsync(managerId, fromUtc, toUtc, ct);
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    // KPI خود Manager (پنل منیجر)
    [HttpGet("my-kpi")]
    [Authorize(Roles = AppRoles.ManagerOrAbove)]
    public async Task<ActionResult<ManagerKpiDto>> GetMyKpi(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        CancellationToken ct)
    {
        var managerId = GetCurrentUserId();
        var dto = await _incidents.GetManagerKpiAsync(managerId, fromUtc, toUtc, ct);
        if (dto is null) return NotFound();
        return Ok(dto);
    }
}
