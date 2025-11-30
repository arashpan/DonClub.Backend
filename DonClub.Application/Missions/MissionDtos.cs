namespace Donclub.Application.Missions;

public record MissionDefinitionDto(
    int Id,
    string Name,
    string? Code,
    string? Description,
    byte Period,
    int TargetValue,
    decimal? RewardWalletAmount,
    string? RewardDescription,
    bool IsActive
);

public record UserMissionDto(
    long Id,
    int MissionDefinitionId,
    string MissionName,
    string? MissionCode,
    byte Period,
    int TargetValue,
    int CurrentValue,
    bool IsCompleted,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc,
    DateTime? CompletedAtUtc
);

// Requests
public record CreateMissionDefinitionRequest(
    string Name,
    string? Code,
    string? Description,
    byte Period,
    int TargetValue,
    decimal? RewardWalletAmount,
    string? RewardDescription
);

public record UpdateMissionDefinitionRequest(
    string Name,
    string? Code,
    string? Description,
    byte Period,
    int TargetValue,
    decimal? RewardWalletAmount,
    string? RewardDescription,
    bool IsActive
);

public record AssignMissionToUserRequest(
    long UserId,
    DateTime? PeriodStartUtc,
    DateTime? PeriodEndUtc
);

public record AddMissionProgressRequest(
    int Amount
);
