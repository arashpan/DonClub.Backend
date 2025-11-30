using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Branches;

public interface IBranchService
{
    Task<IReadOnlyList<BranchSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<BranchDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateBranchAsync(CreateBranchRequest request, CancellationToken ct = default);
    Task UpdateBranchAsync(int id, UpdateBranchRequest request, CancellationToken ct = default);
    Task DeleteBranchAsync(int id, CancellationToken ct = default);

    Task<int> AddRoomAsync(int branchId, CreateRoomRequest request, CancellationToken ct = default);
    Task UpdateRoomAsync(int branchId, int roomId, UpdateRoomRequest request, CancellationToken ct = default);
    Task DeleteRoomAsync(int branchId, int roomId, CancellationToken ct = default);
}
