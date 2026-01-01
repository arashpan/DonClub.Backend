namespace DonClub.Application.AdminUsers.Dtos;

public sealed record SetUserRolesDto(IReadOnlyList<string> Roles);

public sealed record UserRolesDto(long UserId, IReadOnlyList<string> Roles);

public sealed record AdminUserListItemDto(
    long Id,
    string PhoneNumber,
    string? DisplayName,
    IReadOnlyList<string> Roles
);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);
