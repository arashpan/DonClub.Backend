using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Users;

public interface IUserService
{
    Task<IReadOnlyList<UserListItemDto>> GetAllAsync(string? search, string? role, CancellationToken ct = default);
    Task<UserDetailDto?> GetByIdAsync(long id, CancellationToken ct = default);

    Task<long> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task UpdateAsync(long id, UpdateUserRequest request, CancellationToken ct = default);

    Task UpdateRolesAsync(long id, UpdateUserRolesRequest request, CancellationToken ct = default);
    Task SetActiveAsync(long id, bool isActive, CancellationToken ct = default);
}