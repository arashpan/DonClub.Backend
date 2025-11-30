using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Missions;

public interface IMissionService
{
    // Mission definitions (Admin / SuperUser)
    Task<IReadOnlyList<MissionDefinitionDto>> GetDefinitionsAsync(bool? isActive, CancellationToken ct = default);
    Task<MissionDefinitionDto?> GetDefinitionByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateDefinitionAsync(CreateMissionDefinitionRequest request, CancellationToken ct = default);
    Task UpdateDefinitionAsync(int id, UpdateMissionDefinitionRequest request, CancellationToken ct = default);
    Task DeleteDefinitionAsync(int id, CancellationToken ct = default);

    // User missions
    Task<IReadOnlyList<UserMissionDto>> GetUserMissionsAsync(long userId, bool onlyActive, CancellationToken ct = default);
    Task<UserMissionDto> AssignMissionToUserAsync(int missionDefinitionId, long userId, DateTime? periodStartUtc, DateTime? periodEndUtc, CancellationToken ct = default);
    Task<UserMissionDto> AddProgressAsync(long userMissionId, int amount, CancellationToken ct = default);
}
