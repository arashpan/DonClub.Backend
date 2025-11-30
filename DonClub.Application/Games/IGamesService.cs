using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Games;

public interface IGameService
{
    // Games
    Task<int> CreateGameAsync(CreateGameRequest request, CancellationToken ct = default);
    Task UpdateGameAsync(int id, UpdateGameRequest request, CancellationToken ct = default);
    Task DeleteGameAsync(int id, CancellationToken ct = default);
    Task<GameDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<GameSummaryDto>> GetAllAsync(CancellationToken ct = default);

    // Roles
    Task<int> AddRoleAsync(int gameId, CreateGameRoleRequest request, CancellationToken ct = default);
    Task UpdateRoleAsync(int gameId, int roleId, UpdateGameRoleRequest request, CancellationToken ct = default);
    Task DeleteRoleAsync(int gameId, int roleId, CancellationToken ct = default);

    // Scenarios
    Task<int> AddScenarioAsync(int gameId, CreateScenarioRequest request, CancellationToken ct = default);
    Task UpdateScenarioAsync(int gameId, int scenarioId, UpdateScenarioRequest request, CancellationToken ct = default);
    Task DeleteScenarioAsync(int gameId, int scenarioId, CancellationToken ct = default);

    // Scenario Roles
    Task SetScenarioRolesAsync(int gameId, int scenarioId, SetScenarioRolesRequest request, CancellationToken ct = default);
}
