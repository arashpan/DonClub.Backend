using Donclub.Application.Games;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Donclub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperUser")]
public class GamesController : ControllerBase
{
    private readonly IGameService _games;

    public GamesController(IGameService games)
    {
        _games = games;
    }

    // ------------------ Games ------------------

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<GameSummaryDto>>> GetAll(CancellationToken ct)
        => Ok(await _games.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameDetailDto>> GetById(int id, CancellationToken ct)
    {
        var game = await _games.GetByIdAsync(id, ct);
        return game is null ? NotFound() : Ok(game);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateGameRequest req, CancellationToken ct)
    {
        var id = await _games.CreateGameAsync(req, ct);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, UpdateGameRequest req, CancellationToken ct)
    {
        await _games.UpdateGameAsync(id, req, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        await _games.DeleteGameAsync(id, ct);
        return NoContent();
    }

    // ------------------ Game Roles ------------------

    [HttpPost("{gameId:int}/roles")]
    public async Task<ActionResult> AddRole(int gameId, CreateGameRoleRequest req, CancellationToken ct)
    {
        var id = await _games.AddRoleAsync(gameId, req, ct);
        return Ok(new { roleId = id });
    }

    [HttpPut("{gameId:int}/roles/{roleId:int}")]
    public async Task<ActionResult> UpdateRole(int gameId, int roleId, UpdateGameRoleRequest req, CancellationToken ct)
    {
        await _games.UpdateRoleAsync(gameId, roleId, req, ct);
        return NoContent();
    }

    [HttpDelete("{gameId:int}/roles/{roleId:int}")]
    public async Task<ActionResult> DeleteRole(int gameId, int roleId, CancellationToken ct)
    {
        await _games.DeleteRoleAsync(gameId, roleId, ct);
        return NoContent();
    }

    // ------------------ Scenarios ------------------

    [HttpPost("{gameId:int}/scenarios")]
    public async Task<ActionResult> AddScenario(int gameId, CreateScenarioRequest req, CancellationToken ct)
    {
        var id = await _games.AddScenarioAsync(gameId, req, ct);
        return Ok(new { scenarioId = id });
    }

    [HttpPut("{gameId:int}/scenarios/{scenarioId:int}")]
    public async Task<ActionResult> UpdateScenario(int gameId, int scenarioId, UpdateScenarioRequest req, CancellationToken ct)
    {
        await _games.UpdateScenarioAsync(gameId, scenarioId, req, ct);
        return NoContent();
    }

    [HttpDelete("{gameId:int}/scenarios/{scenarioId:int}")]
    public async Task<ActionResult> DeleteScenario(int gameId, int scenarioId, CancellationToken ct)
    {
        await _games.DeleteScenarioAsync(gameId, scenarioId, ct);
        return NoContent();
    }

    // ------------------ Scenario Roles ------------------

    [HttpPost("{gameId:int}/scenarios/{scenarioId:int}/roles")]
    public async Task<ActionResult> SetScenarioRoles(int gameId, int scenarioId, SetScenarioRolesRequest req, CancellationToken ct)
    {
        await _games.SetScenarioRolesAsync(gameId, scenarioId, req, ct);
        return NoContent();
    }
}
