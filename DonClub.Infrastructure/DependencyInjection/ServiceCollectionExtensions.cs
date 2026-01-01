using Donclub.Application.Achievements;
using Donclub.Application.Auth;
using Donclub.Application.Badges;
using Donclub.Application.Branches;
using Donclub.Application.Common;
using Donclub.Application.Games;
using Donclub.Application.Incidents;
using Donclub.Application.Missions;
using Donclub.Application.Notifications;
using Donclub.Application.Profile;
using Donclub.Application.Rewards;
using Donclub.Application.Sessions;
using Donclub.Application.Settings;
using Donclub.Application.Users;
using Donclub.Application.Wallets;
using Donclub.Infrastructure.Achievements;
using Donclub.Infrastructure.Auth;
using Donclub.Infrastructure.Badges;
using Donclub.Infrastructure.Branches;
using Donclub.Infrastructure.Common;
using Donclub.Infrastructure.Games;
using Donclub.Infrastructure.Incidents;
using Donclub.Infrastructure.Missions;
using Donclub.Infrastructure.Notifications;
using Donclub.Infrastructure.Profile;
using Donclub.Infrastructure.Rewards;
using Donclub.Infrastructure.Sessions;
using Donclub.Infrastructure.Settings;
using Donclub.Infrastructure.Users;
using Donclub.Infrastructure.Wallets;
using DonClub.Application.AdminUsers;
using DonClub.Infrastructure.AdminUsers;
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
        services.AddScoped<IOtpSettingsService, OtpSettingsService>();
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<IRewardService, RewardService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IAdminUserService, AdminUserService>();

        return services;
    }
}
