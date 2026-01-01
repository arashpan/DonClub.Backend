using DonClub.Application.AdminUsers.Dtos;

namespace DonClub.Application.AdminUsers;

public interface IAdminUserService
{
    Task<PagedResult<AdminUserListItemDto>> GetUsersAsync(string? q, string? role, int page, int pageSize, CancellationToken ct);
    Task<UserRolesDto> GetUserRolesAsync(long userId, CancellationToken ct);
    Task<UserRolesDto> SetUserRolesAsync(long userId, SetUserRolesDto dto, int actorUserId, CancellationToken ct);
}
