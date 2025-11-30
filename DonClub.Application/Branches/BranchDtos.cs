namespace Donclub.Application.Branches;

public record RoomDto(
    int Id,
    string Name,
    int? Capacity,
    bool IsActive
);

public record BranchSummaryDto(
    int Id,
    string Name,
    bool IsActive
);

public record BranchDetailDto(
    int Id,
    string Name,
    string? Address,
    bool IsActive,
    IReadOnlyList<RoomDto> Rooms
);

public record CreateBranchRequest(
    string Name,
    string? Address
);

public record UpdateBranchRequest(
    string Name,
    string? Address,
    bool IsActive
);

public record CreateRoomRequest(
    string Name,
    int? Capacity
);

public record UpdateRoomRequest(
    string Name,
    int? Capacity,
    bool IsActive
);
