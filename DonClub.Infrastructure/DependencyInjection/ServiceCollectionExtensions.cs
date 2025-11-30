using Donclub.Application.Auth;
using Donclub.Application.Common;
using Donclub.Infrastructure.Auth;
using Donclub.Infrastructure.Common;
using Donclub.Application.Branches;
using Donclub.Infrastructure.Branches;
using Donclub.Application.Games;
using Donclub.Infrastructure.Games;
using Donclub.Application.Sessions;
using Donclub.Infrastructure.Sessions;
using Donclub.Application.Users;
using Donclub.Infrastructure.Users;
using Donclub.Application.Wallets;
using Donclub.Infrastructure.Wallets;
using Donclub.Application.Badges;
using Donclub.Infrastructure.Badges;
using Donclub.Application.Incidents;
using Donclub.Infrastructure.Incidents;
using Donclub.Application.Missions;
using Donclub.Infrastructure.Missions;


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
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IBadgeService, BadgeService>();
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IMissionService, MissionService>();

        services.AddScoped<IBranchService, BranchService>();

        return services;
    }
}
