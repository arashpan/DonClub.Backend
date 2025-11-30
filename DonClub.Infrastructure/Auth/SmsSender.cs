using Donclub.Application.Auth;
using Microsoft.Extensions.Logging;

namespace Donclub.Infrastructure.Auth;

public class SmsSender : ISmsSender
{
    private readonly ILogger<SmsSender> _logger;

    public SmsSender(ILogger<SmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        // فعلاً فقط لاگ می‌کنیم. بعداً به سرویس واقعی SMS وصل می‌کنیم.
        _logger.LogInformation("Sending SMS to {Phone}: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}
