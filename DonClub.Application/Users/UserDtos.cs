using Donclub.Domain.Users;

namespace Donclub.Application.Users;

public record UserListItemDto(
    long Id,
    string PhoneNumber,
    string? DisplayName,
    bool IsActive,
    string[] Roles,
    MembershipLevel MembershipLevel
);

public record UserDetailDto(
    long Id,
    string UserName,
    string PhoneNumber,
    string? DisplayName,
    string? Email,
    bool IsActive,
    bool PhoneNumberConfirmed,
    MembershipLevel MembershipLevel,
    string[] Roles,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record CreateUserRequest(
    string PhoneNumber,
    string? DisplayName,
    string? Email,
    MembershipLevel MembershipLevel,
    string[] Roles           // مثل: ["Manager", "Admin"]
);

public record UpdateUserRequest(
    string? DisplayName,
    string? Email,
    bool IsActive,
    MembershipLevel MembershipLevel
);

public record UpdateUserRolesRequest(
    string[] Roles
);

public record SetUserActiveRequest(
    bool IsActive
);
