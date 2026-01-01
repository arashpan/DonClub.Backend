using DonClub.Application.AdminUsers;
using DonClub.Application.AdminUsers.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DonClub.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "SuperUser")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _svc;
    public AdminUsersController(IAdminUserService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminUserListItemDto>>> GetUsers(
        [FromQuery] string? q,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        return Ok(await _svc.GetUsersAsync(q, role, page, pageSize, ct));
    }

    [HttpGet("{userId:long}/roles")]
    public async Task<ActionResult<UserRolesDto>> GetRoles(long userId, CancellationToken ct)
        => Ok(await _svc.GetUserRolesAsync(userId, ct));

    [HttpPut("{userId:long}/roles")]
    public async Task<ActionResult<UserRolesDto>> SetRoles(long userId, [FromBody] SetUserRolesDto dto, CancellationToken ct)
    {
        var actorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _svc.SetUserRolesAsync(userId, dto, actorId, ct));
    }
}
