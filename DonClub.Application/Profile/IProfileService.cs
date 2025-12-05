using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Profile;

public interface IProfileService
{
    Task<ProfileDto?> GetProfileAsync(long userId, CancellationToken ct = default);
}
