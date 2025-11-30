namespace Donclub.Application.Badges;

public record BadgeDto(
    int Id,
    string Name,
    string? Code,
    string? Description,
    string? IconUrl,
    bool IsActive
);

public record PlayerBadgeDto(
    long Id,
    int BadgeId,
    string BadgeName,
    string? BadgeCode,
    string? IconUrl,
    DateTime EarnedAtUtc,
    bool IsRevoked,
    string? Reason
);

// Requests
public record CreateBadgeRequest(
    string Name,
    string? Code,
    string? Description,
    string? IconUrl
);

public record UpdateBadgeRequest(
    string Name,
    string? Code,
    string? Description,
    string? IconUrl,
    bool IsActive
);

public record GrantBadgeRequest(
    long UserId,
    string? Reason
);

public record RevokeBadgeRequest(
    string? Reason
);
