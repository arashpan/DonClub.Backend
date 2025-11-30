namespace Donclub.Application.Games;

public record GameSummaryDto(
    int Id,
    string Name,
    bool IsActive
);

public record GameDetailDto(
    int Id,
    string Name,
    string? Description,
    List<GameRoleDto> Roles,
    List<ScenarioDto> Scenarios
);

public record GameRoleDto(
    int Id,
    string Name,
    byte Team,
    string? Description
);

public record ScenarioDto(
    int Id,
    string Name,
    byte? PlayerCount,
    List<ScenarioRoleDto> Roles
);

public record ScenarioRoleDto(
    int GameRoleId,
    string RoleName,
    byte Count
);

// Requests
public record CreateGameRequest(string Name, string? Description);
public record UpdateGameRequest(string Name, string? Description, bool IsActive);

public record CreateGameRoleRequest(string Name, byte Team, string? Description);
public record UpdateGameRoleRequest(string Name, byte Team, string? Description, bool IsActive);

public record CreateScenarioRequest(string Name, byte? PlayerCount, string? Description);
public record UpdateScenarioRequest(string Name, byte? PlayerCount, string? Description, bool IsActive);

public record SetScenarioRolesRequest(List<ScenarioRoleInput> Roles);
public record ScenarioRoleInput(int GameRoleId, byte Count);
