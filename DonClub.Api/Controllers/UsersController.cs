using Donclub.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<IReadOnlyList<UserListItemDto>>> GetAll([FromQuery] string? search, CancellationToken ct)
    {
        var result = await _users.GetAllAsync(search, ct);
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
}
