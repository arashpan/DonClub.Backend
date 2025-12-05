using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Sessions;

public interface ISessionService
{
    Task<long> CreateAsync(CreateSessionRequest request, CancellationToken ct = default);
    Task UpdateAsync(long id, UpdateSessionRequest request, CancellationToken ct = default);
    Task CancelAsync(long id, CancellationToken ct = default);
    Task ChangeStatusAsync(long id, byte status, CancellationToken ct = default);

    Task<SessionDetailDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<SessionSummaryDto>> GetByBranchAndDateAsync(int branchId, DateOnly date, CancellationToken ct = default);

    Task AddPlayerAsync(long sessionId, long playerId, CancellationToken ct = default);
    Task RemovePlayerAsync(long sessionId, long playerId, CancellationToken ct = default);

    Task SetPlayerScoreAsync(long sessionId, long playerId, int score, CancellationToken ct = default);
    // 🔹 جدید: سشن‌های کاربر به‌عنوان Player
    Task<IReadOnlyList<SessionSummaryDto>> GetByPlayerAsync(
        long playerId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken ct = default);

    // 🔹 جدید: سشن‌های کاربر به‌عنوان Manager
    Task<IReadOnlyList<SessionSummaryDto>> GetByManagerAsync(
        long managerId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken ct = default);
}
