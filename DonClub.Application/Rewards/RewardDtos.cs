namespace Donclub.Application.Rewards;

public enum RewardReasonType : byte
{
    MissionCompleted = 1,
    BadgeGranted = 2,
    ManualAdjustment = 3
}

public record RewardRequest(
    long UserId,
    decimal Amount,
    RewardReasonType ReasonType,
    string? Description,
    string? ReferenceEntity,
    long? ReferenceId
);
