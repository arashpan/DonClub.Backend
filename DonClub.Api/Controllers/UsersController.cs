using Donclub.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Donclub.Domain.Users;
using Donclub.Application.Games;


namespace Donclub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperUser,Admin")]  // بعداً فعال می‌کنیم
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users)
    {
        _users = users;
    }

    // GET /api/users?search=...
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserListItemDto>>> GetAll([FromQuery] string? search,[FromQuery] string? role, CancellationToken ct)
    {
		var result = await _users.GetAllAsync(search, role, ct);
		return Ok(result);
    }

    // GET /api/users/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserDetailDto>> GetById(long id, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(id, ct);
        if (user is null) return NotFound();
        return Ok(user);
    }

    // POST /api/users
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var id = await _users.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    // PUT /api/users/5
    [HttpPut("{id:long}")]
    public async Task<ActionResult> Update(long id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        await _users.UpdateAsync(id, request, ct);
        return NoContent();
    }

    // PUT /api/users/5/roles
    [HttpPut("{id:long}/roles")]
    public async Task<ActionResult> UpdateRoles(long id, [FromBody] UpdateUserRolesRequest request, CancellationToken ct)
    {
        await _users.UpdateRolesAsync(id, request, ct);
        return NoContent();
    }

    // PUT /api/users/5/active
    [HttpPut("{id:long}/active")]
    public async Task<ActionResult> SetActive(long id, [FromBody] SetUserActiveRequest request, CancellationToken ct)
    {
        await _users.SetActiveAsync(id, request.IsActive, ct);
        return NoContent();
    }

    // GET /api/Users/me
    [HttpGet("me")]
    [Authorize] // هر کاربر لاگین کرده
    public async Task<ActionResult<UserDetailDto>> GetMe(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();

        if (!long.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null) return NotFound();

        return Ok(user);
    }

	// GET /api/users/{id}/games
	[HttpGet("{id:long}/games")]
	public async Task<ActionResult<IReadOnlyList<GameSummaryDto>>> GetUserGames(long id, CancellationToken ct)
	{
		var games = await _users.GetUserGamesAsync(id, ct);
		return Ok(games);
	}


}
