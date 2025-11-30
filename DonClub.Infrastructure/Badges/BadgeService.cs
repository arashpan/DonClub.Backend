using Donclub.Application.Badges;
using Donclub.Domain.Badges;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Donclub.Infrastructure.Badges;

public class BadgeService : IBadgeService
{
    private readonly DonclubDbContext _db;

    public BadgeService(DonclubDbContext db)
    {
        _db = db;
    }

    // ---------------- Badges (Admin) ----------------

    public async Task<IReadOnlyList<BadgeDto>> GetAllBadgesAsync(CancellationToken ct = default)
    {
        return await _db.Badges
            .OrderBy(b => b.Name)
            .Select(b => new BadgeDto(
                b.Id,
                b.Name,
                b.Code,
                b.Description,
                b.IconUrl,
                b.IsActive
            ))
            .ToListAsync(ct);
    }

    public async Task<BadgeDto?> GetBadgeByIdAsync(int id, CancellationToken ct = default)
    {
        var b = await _db.Badges.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (b is null) return null;

        return new BadgeDto(
            b.Id,
            b.Name,
            b.Code,
            b.Description,
            b.IconUrl,
            b.IsActive
        );
    }

    public async Task<int> CreateBadgeAsync(CreateBadgeRequest request, CancellationToken ct = default)
    {
        var entity = new Badge
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            IconUrl = request.IconUrl,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Badges.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task UpdateBadgeAsync(int id, UpdateBadgeRequest request, CancellationToken ct = default)
    {
        var b = await _db.Badges.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Badge not found.");

        b.Name = request.Name;
        b.Code = request.Code;
        b.Description = request.Description;
        b.IconUrl = request.IconUrl;
        b.IsActive = request.IsActive;
        b.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteBadgeAsync(int id, CancellationToken ct = default)
    {
        var b = await _db.Badges.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (b is null) return;

        _db.Badges.Remove(b);
        await _db.SaveChangesAsync(ct);
    }

    // ---------------- Player Badges ----------------

    public async Task<IReadOnlyList<PlayerBadgeDto>> GetBadgesForUserAsync(long userId, CancellationToken ct = default)
    {
        return await _db.PlayerBadges
            .Include(pb => pb.Badge)
            .Where(pb => pb.UserId == userId)
            .OrderByDescending(pb => pb.EarnedAtUtc)
            .Select(pb => new PlayerBadgeDto(
                pb.Id,
                pb.BadgeId,
                pb.Badge.Name,
                pb.Badge.Code,
                pb.Badge.IconUrl,
                pb.EarnedAtUtc,
                pb.IsRevoked,
                pb.Reason
            ))
            .ToListAsync(ct);
    }

    public async Task<long> GrantBadgeAsync(int badgeId, long userId, string? reason, long? grantedByUserId, CancellationToken ct = default)
    {
        // اطمینان از وجود Badge
        var badge = await _db.Badges.FirstOrDefaultAsync(b => b.Id == badgeId && b.IsActive, ct)
            ?? throw new KeyNotFoundException("Badge not found or inactive.");

        // اطمینان از وجود User
        var userExists = await _db.Users.AnyAsync(u => u.Id == userId, ct);
        if (!userExists)
            throw new KeyNotFoundException("User not found.");

        // جلوگیری از تکرار (اگر نمی‌خواهیم duplicate باشد)
        var exists = await _db.PlayerBadges.AnyAsync(pb => pb.UserId == userId && pb.BadgeId == badgeId && !pb.IsRevoked, ct);
        if (exists)
            throw new InvalidOperationException("User already has this badge.");

        var pb = new PlayerBadge
        {
            UserId = userId,
            BadgeId = badgeId,
            EarnedAtUtc = DateTime.UtcNow,
            GrantedByUserId = grantedByUserId,
            Reason = reason,
            IsRevoked = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.PlayerBadges.Add(pb);
        await _db.SaveChangesAsync(ct);
        return pb.Id;
    }

    public async Task RevokeBadgeAsync(long playerBadgeId, string? reason, long? revokedByUserId, CancellationToken ct = default)
    {
        var pb = await _db.PlayerBadges.FirstOrDefaultAsync(x => x.Id == playerBadgeId, ct)
            ?? throw new KeyNotFoundException("PlayerBadge not found.");

        if (pb.IsRevoked)
            return;

        pb.IsRevoked = true;
        pb.Reason = reason ?? pb.Reason;
        pb.UpdatedAtUtc = DateTime.UtcNow;

        // می‌تونی revokedByUserId رو در یک فیلد جدا ذخیره کنی (اگر بعداً اضافه کردیم)
        await _db.SaveChangesAsync(ct);
    }
}
