using Donclub.Domain.Common;

namespace Donclub.Domain.Games;

public class Game : BaseEntity<int>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? RulesJson { get; set; }
    public byte? MinPlayers { get; set; }
    public byte? MaxPlayers { get; set; }
    public string? ScoringSchemaJson { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<GameRole> Roles { get; set; } = new List<GameRole>();
    public ICollection<Scenario> Scenarios { get; set; } = new List<Scenario>();
}

public enum GameRoleTeam : byte
{
    Neutral = 0,
    Civilian = 1,
    Mafia = 2
}

public class GameRole : BaseEntity<int>
{
    public int GameId { get; set; }
    public string Name { get; set; } = default!;
    public GameRoleTeam Team { get; set; } = GameRoleTeam.Neutral;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Game Game { get; set; } = default!;
}

public class Scenario : BaseEntity<int>
{
    public int GameId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public byte? PlayerCount { get; set; }
    public bool IsActive { get; set; } = true;

    public Game Game { get; set; } = default!;
    public ICollection<ScenarioRole> ScenarioRoles { get; set; } = new List<ScenarioRole>();
}

public class ScenarioRole
{
    public int ScenarioId { get; set; }
    public int GameRoleId { get; set; }
    public byte Count { get; set; }

    public Scenario Scenario { get; set; } = default!;
    public GameRole GameRole { get; set; } = default!;
}
