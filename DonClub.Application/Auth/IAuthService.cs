using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Auth;

public interface IAuthService
{
    Task<RequestOtpResultDto> RequestOtpAsync(string phoneNumber, CancellationToken ct = default);
    Task<AuthResultDto> VerifyOtpAsync(string phoneNumber, string code, CancellationToken ct = default);
    Task<AuthResultDto> RefreshAsync(string refreshToken, CancellationToken ct = default);
}
