using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Incidents;

public interface IIncidentService
{
    Task<long> CreateAsync(CreateIncidentRequest request, long createdByUserId, CancellationToken ct = default);
    Task ReviewAsync(long id, long reviewerUserId, ReviewIncidentRequest request, CancellationToken ct = default);

    Task<IncidentDetailDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<IncidentSummaryDto>> GetByManagerAsync(long managerId, CancellationToken ct = default);
    Task<IReadOnlyList<IncidentSummaryDto>> GetPendingAsync(CancellationToken ct = default);

    Task<ManagerKpiDto?> GetManagerKpiAsync(long managerId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
}
