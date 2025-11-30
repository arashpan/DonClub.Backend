using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Auth;

public interface ISmsSender
{
    Task SendAsync(string phoneNumber, string message, CancellationToken ct = default);
}
