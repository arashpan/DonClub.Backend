using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Badges;

public interface IBadgeService
{
    // Admin / SuperUser
    Task<IReadOnlyList<BadgeDto>> GetAllBadgesAsync(CancellationToken ct = default);
    Task<BadgeDto?> GetBadgeByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateBadgeAsync(CreateBadgeRequest request, CancellationToken ct = default);
    Task UpdateBadgeAsync(int id, UpdateBadgeRequest request, CancellationToken ct = default);
    Task DeleteBadgeAsync(int id, CancellationToken ct = default);

    // Player badges
    Task<IReadOnlyList<PlayerBadgeDto>> GetBadgesForUserAsync(long userId, CancellationToken ct = default);
    Task<long> GrantBadgeAsync(int badgeId, long userId, string? reason, long? grantedByUserId, CancellationToken ct = default);
    Task RevokeBadgeAsync(long playerBadgeId, string? reason, long? revokedByUserId, CancellationToken ct = default);
}
