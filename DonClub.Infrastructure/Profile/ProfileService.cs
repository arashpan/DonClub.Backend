using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Donclub.Application.Profile;
using Donclub.Application.Missions;
using Donclub.Application.Badges;
using Donclub.Application.Wallets;
using Donclub.Domain.Users;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Donclub.Infrastructure.Profile;

public class ProfileService : IProfileService
{
    private readonly DonclubDbContext _db;
    private readonly IMissionService _missions;
    private readonly IBadgeService _badges;
    private readonly IWalletService _wallets;

    public ProfileService(
        DonclubDbContext db,
        IMissionService missions,
        IBadgeService badges,
        IWalletService wallets)
    {
        _db = db;
        _missions = missions;
        _badges = badges;
        _wallets = wallets;
    }

    public async Task<ProfileDto?> GetProfileAsync(long userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return null;

        var roles = user.UserRoles
            .Select(ur => ur.Role.Name)
            .ToArray();

        // Wallet: اگر هنوز ساخته نشده باشه، می‌تونی تصمیم بگیری null باشه یا auto-create
        // فعلاً فقط می‌خونیم:
        var wallet = await _wallets.GetWalletByUserIdAsync(userId, ct);

        // فقط مأموریت‌های active کاربر
        var missions = await _missions.GetUserMissionsAsync(userId, onlyActive: true, ct);

        // همه Badgeهای کاربر
        var badges = await _badges.GetBadgesForUserAsync(userId, ct);

        return new ProfileDto(
            UserId: user.Id,
            PhoneNumber: user.PhoneNumber,
            DisplayName: user.DisplayName,
            Roles: roles,
            MembershipLevel: user.MembershipLevel.ToString(),
            IsActive: user.IsActive,
            Wallet: wallet,
            Missions: missions,
            Badges: badges
        );
    }
}
