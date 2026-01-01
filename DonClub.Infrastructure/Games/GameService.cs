using Donclub.Application.Games;
using Donclub.Domain.Games;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Donclub.Infrastructure.Games;

public class GameService : IGameService
{
    private readonly DonclubDbContext _db;

    public GameService(DonclubDbContext db)
    {
        _db = db;
    }

    // --------------------------------------------------------------------
    // Games
    // --------------------------------------------------------------------

    public async Task<int> CreateGameAsync(CreateGameRequest request, CancellationToken ct = default)
    {
        var game = new Game
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true
        };

        _db.Games.Add(game);
        await _db.SaveChangesAsync(ct);
        return game.Id;
    }

    public async Task UpdateGameAsync(int id, UpdateGameRequest request, CancellationToken ct = default)
    {
        var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new KeyNotFoundException("Game not found.");

        game.Name = request.Name;
        game.Description = request.Description;
        game.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteGameAsync(int id, CancellationToken ct = default)
    {
        var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == id, ct);
        if (game == null)
            return;

        _db.Games.Remove(game);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<GameDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var game = await _db.Games
            .Include(g => g.Roles)
            .Include(g => g.Scenarios)
                .ThenInclude(s => s.ScenarioRoles)
                    .ThenInclude(sr => sr.GameRole)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (game == null) return null;

        // Include global/shared roles as well (GameId == NULL)
        var globalRoles = await _db.GameRoles
            .Where(r => r.GameId == null)
            .ToListAsync(ct);

        var roles = game.Roles
            .Concat(globalRoles)
            .OrderBy(r => r.Name)
            .Select(r => new GameRoleDto(r.Id, r.Name, (byte)r.Team, r.Description))
            .ToList();

        var scenarios = game.Scenarios
            .Select(s => new ScenarioDto(
                s.Id,
                s.Name,
                s.PlayerCount,
                s.ScenarioRoles
                    .Select(sr => new ScenarioRoleDto(
                        sr.GameRoleId,
                        sr.GameRole.Name,
                        sr.Count
                    )).ToList()
            )).ToList();

        return new GameDetailDto(game.Id, game.Name, game.Description, roles, scenarios);
    }

    public async Task<IReadOnlyList<GameSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Games
            .OrderBy(g => g.Name)
            .Select(g => new GameSummaryDto(g.Id, g.Name, g.IsActive))
            .ToListAsync(ct);
    }

    // --------------------------------------------------------------------
    // Roles
    // --------------------------------------------------------------------

    public async Task<int> AddRoleAsync(int gameId, CreateGameRoleRequest request, CancellationToken ct = default)
    {
        var exists = await _db.Games.AnyAsync(g => g.Id == gameId, ct);
        if (!exists)
            throw new KeyNotFoundException("Game not found.");

        var duplicate = await _db.GameRoles
            .AnyAsync(r => r.GameId == gameId && r.Name == request.Name, ct);
        if (duplicate)
            throw new InvalidOperationException("Role name already exists for this game.");

        var role = new GameRole
        {
            GameId = gameId,
            Name = request.Name,
            Team = (GameRoleTeam)request.Team,
            Description = request.Description,
            IsActive = true
        };

        _db.GameRoles.Add(role);
        await _db.SaveChangesAsync(ct);
        return role.Id;
    }

    // --------------------------------------------------------------------
    // Global/Shared Roles (GameId == NULL)
    // --------------------------------------------------------------------

    public async Task<IReadOnlyList<GameRoleDto>> GetGlobalRolesAsync(CancellationToken ct = default)
    {
        return await _db.GameRoles
            .Where(r => r.GameId == null)
            .OrderBy(r => r.Name)
            .Select(r => new GameRoleDto(r.Id, r.Name, (byte)r.Team, r.Description))
            .ToListAsync(ct);
    }

    public async Task<int> AddGlobalRoleAsync(CreateGameRoleRequest request, CancellationToken ct = default)
    {
        var duplicate = await _db.GameRoles
            .AnyAsync(r => r.GameId == null && r.Name == request.Name, ct);
        if (duplicate)
            throw new InvalidOperationException("Global role name already exists.");

        var role = new GameRole
        {
            GameId = null,
            Name = request.Name,
            Team = (GameRoleTeam)request.Team,
            Description = request.Description,
            IsActive = true
        };

        _db.GameRoles.Add(role);
        await _db.SaveChangesAsync(ct);
        return role.Id;
    }

    public async Task UpdateGlobalRoleAsync(int roleId, UpdateGameRoleRequest request, CancellationToken ct = default)
    {
        var role = await _db.GameRoles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.GameId == null, ct)
            ?? throw new KeyNotFoundException("Global role not found.");

        var duplicate = await _db.GameRoles
            .AnyAsync(r => r.Id != roleId && r.GameId == null && r.Name == request.Name, ct);
        if (duplicate)
            throw new InvalidOperationException("Global role name already exists.");

        role.Name = request.Name;
        role.Team = (GameRoleTeam)request.Team;
        role.Description = request.Description;
        role.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteGlobalRoleAsync(int roleId, CancellationToken ct = default)
    {
        var role = await _db.GameRoles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.GameId == null, ct);

        if (role == null)
            return;

        _db.GameRoles.Remove(role);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateRoleAsync(int gameId, int roleId, UpdateGameRoleRequest request, CancellationToken ct = default)
    {
        var role = await _db.GameRoles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.GameId == gameId, ct)
            ?? throw new KeyNotFoundException("Role not found.");

        var duplicate = await _db.GameRoles
            .AnyAsync(r => r.Id != roleId && r.GameId == gameId && r.Name == request.Name, ct);
        if (duplicate)
            throw new InvalidOperationException("Role name already exists for this game.");

        role.Name = request.Name;
        role.Team = (GameRoleTeam)request.Team;
        role.Description = request.Description;
        role.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteRoleAsync(int gameId, int roleId, CancellationToken ct = default)
    {
        var role = await _db.GameRoles
            .FirstOrDefaultAsync(r => r.Id == roleId && r.GameId == gameId, ct);

        if (role == null)
            return;

        _db.GameRoles.Remove(role);
        await _db.SaveChangesAsync(ct);
    }

    // --------------------------------------------------------------------
    // Scenarios
    // --------------------------------------------------------------------

    public async Task<int> AddScenarioAsync(int gameId, CreateScenarioRequest request, CancellationToken ct = default)
    {
        var exists = await _db.Games.AnyAsync(g => g.Id == gameId, ct);
        if (!exists)
            throw new KeyNotFoundException("Game not found.");

        var scenario = new Scenario
        {
            GameId = gameId,
            Name = request.Name,
            Description = request.Description,
            PlayerCount = request.PlayerCount,
            IsActive = true
        };

        _db.Scenarios.Add(scenario);
        await _db.SaveChangesAsync(ct);
        return scenario.Id;
    }

    public async Task UpdateScenarioAsync(int gameId, int scenarioId, UpdateScenarioRequest request, CancellationToken ct = default)
    {
        var scenario = await _db.Scenarios
            .FirstOrDefaultAsync(s => s.Id == scenarioId && s.GameId == gameId, ct)
            ?? throw new KeyNotFoundException("Scenario not found.");

        scenario.Name = request.Name;
        scenario.PlayerCount = request.PlayerCount;
        scenario.Description = request.Description;
        scenario.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteScenarioAsync(int gameId, int scenarioId, CancellationToken ct = default)
    {
        var scenario = await _db.Scenarios
            .FirstOrDefaultAsync(s => s.Id == scenarioId && s.GameId == gameId, ct);
        if (scenario == null)
            return;

        _db.Scenarios.Remove(scenario);
        await _db.SaveChangesAsync(ct);
    }

    // --------------------------------------------------------------------
    // Scenario Roles
    // --------------------------------------------------------------------

    public async Task SetScenarioRolesAsync(int gameId, int scenarioId, SetScenarioRolesRequest request, CancellationToken ct = default)
    {
        var scenario = await _db.Scenarios
            .Include(s => s.ScenarioRoles)
            .FirstOrDefaultAsync(s => s.Id == scenarioId && s.GameId == gameId, ct)
            ?? throw new KeyNotFoundException("Scenario not found.");

        // Validate incoming roles: they must exist and be either global (GameId == NULL)
        // or belong to the same game.
        var roleIds = request.Roles
            .Select(r => r.GameRoleId)
            .Distinct()
            .ToList();

        if (roleIds.Count > 0)
        {
            var roles = await _db.GameRoles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => new { r.Id, r.GameId })
                .ToListAsync(ct);

            var foundIds = roles.Select(r => r.Id).ToHashSet();
            var missing = roleIds.Where(id => !foundIds.Contains(id)).ToList();
            if (missing.Count > 0)
                throw new KeyNotFoundException("One or more roles were not found.");

            var invalid = roles
                .Where(r => r.GameId != null && r.GameId != gameId)
                .Select(r => r.Id)
                .ToList();

            if (invalid.Count > 0)
                throw new InvalidOperationException("One or more roles do not belong to this game.");
        }

        // پاک‌کردن نقش‌های قبلی سناریو
        _db.ScenarioRoles.RemoveRange(scenario.ScenarioRoles);

        // اضافه‌کردن نقش‌های جدید
        foreach (var r in request.Roles)
        {
            _db.ScenarioRoles.Add(new ScenarioRole
            {
                ScenarioId = scenarioId,
                GameRoleId = r.GameRoleId,
                Count = r.Count
            });
        }

        await _db.SaveChangesAsync(ct);
    }
}
