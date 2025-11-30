namespace Donclub.Application.Sessions;

public record SessionSummaryDto(
    long Id,
    int BranchId,
    int RoomId,
    int GameId,
    int? ScenarioId,
    long? ManagerId,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    byte Status,
    byte Tier
);

public record SessionPlayerDto(
    long PlayerId,
    string? DisplayName,
    string PhoneNumber,
    byte Status,
    int? Score
);

public record SessionDetailDto(
    long Id,
    int BranchId,
    int RoomId,
    int GameId,
    int? ScenarioId,
    long? ManagerId,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    byte Status,
    byte Tier,
    byte? MaxPlayers,
    string? Notes,
    IReadOnlyList<SessionPlayerDto> Players
);

// Requests
public record CreateSessionRequest(
    int BranchId,
    int RoomId,
    int GameId,
    int? ScenarioId,
    long? ManagerId,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    byte Tier,
    byte? MaxPlayers,
    string? Notes
);

public record UpdateSessionRequest(
    int BranchId,
    int RoomId,
    int GameId,
    int? ScenarioId,
    long? ManagerId,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    byte Tier,
    byte? MaxPlayers,
    string? Notes
);

public record AddPlayerToSessionRequest(long PlayerId);
public record SetPlayerScoreRequest(long PlayerId, int Score);
public record ChangeSessionStatusRequest(byte Status);
