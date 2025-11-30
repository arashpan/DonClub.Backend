using Donclub.Application.Auth;
using Donclub.Application.Common;
using Donclub.Infrastructure.Auth;
using Donclub.Infrastructure.Common;
using Donclub.Application.Branches;
using Donclub.Infrastructure.Branches;

using Microsoft.Extensions.DependencyInjection;

namespace Donclub.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ISmsSender, SmsSender>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IBranchService, BranchService>();

        return services;
    }
}
