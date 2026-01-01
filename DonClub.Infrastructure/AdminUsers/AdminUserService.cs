using Donclub.Domain.Users;
using Donclub.Infrastructure.Persistence;
using DonClub.Application.AdminUsers;
using DonClub.Application.AdminUsers.Dtos;
using Microsoft.EntityFrameworkCore;

namespace DonClub.Infrastructure.AdminUsers;

public sealed class AdminUserService : IAdminUserService
{
    private static readonly HashSet<string> AllowedRoles =
        new(StringComparer.OrdinalIgnoreCase) { "SuperUser", "Admin", "Manager", "Operator", "Player" };

    private readonly DonclubDbContext _db;
    public AdminUserService(DonclubDbContext db) => _db = db;

    public async Task<PagedResult<AdminUserListItemDto>> GetUsersAsync(string? q, string? role, int page, int pageSize, CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(u => u.PhoneNumber.Contains(q) || u.UserName.Contains(q));

        if (!string.IsNullOrWhiteSpace(role))
        {
            query =
                from u in query
                join ur in _db.UserRoles.AsNoTracking() on u.Id equals ur.UserId
                join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
                where r.Name == role
                select u;
        }

        var total = await query.CountAsync(ct);

        var users = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new { u.Id, u.PhoneNumber, u.DisplayName })
            .ToListAsync(ct);

        var userIds = users.Select(x => x.Id).ToList();

        var rolesMap = await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where userIds.Contains(ur.UserId)
            select new { ur.UserId, r.Name }
        ).ToListAsync(ct);

        var items = users.Select(u =>
        {
            var roles = rolesMap.Where(x => x.UserId == u.Id).Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            return new AdminUserListItemDto(u.Id, u.PhoneNumber, u.DisplayName, roles);
        }).ToList();

        return new PagedResult<AdminUserListItemDto>(items, page, pageSize, total);
    }

    public async Task<UserRolesDto> GetUserRolesAsync(long userId, CancellationToken ct)
    {
        var exists = await _db.Users.AsNoTracking().AnyAsync(x => x.Id == userId, ct);
        if (!exists) throw new KeyNotFoundException("User not found.");

        var roles = await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == userId
            select r.Name
        ).Distinct().ToListAsync(ct);

        return new UserRolesDto(userId, roles);
    }

    public async Task<UserRolesDto> SetUserRolesAsync(long userId, SetUserRolesDto dto, int actorUserId, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user is null) throw new KeyNotFoundException("User not found.");

        var roles = (dto.Roles ?? Array.Empty<string>())
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (roles.Count == 0) throw new InvalidOperationException("At least one role is required.");

        if (roles.Any(r => !AllowedRoles.Contains(r)))
            throw new InvalidOperationException("One or more roles are invalid.");

        // جلوگیری از حذف آخرین سوپریوزر
        var wantsRemoveSuperUser = !roles.Any(r => r.Equals("SuperUser", StringComparison.OrdinalIgnoreCase));
        if (wantsRemoveSuperUser)
        {
            var currentUserIsSuper = await (
                from ur in _db.UserRoles
                join r in _db.Roles on ur.RoleId equals r.Id
                where ur.UserId == userId && r.Name == "SuperUser"
                select ur
            ).AnyAsync(ct);

            if (currentUserIsSuper)
            {
                var superUsersCount = await (
                    from ur in _db.UserRoles
                    join r in _db.Roles on ur.RoleId equals r.Id
                    where r.Name == "SuperUser"
                    select ur.UserId
                ).Distinct().CountAsync(ct);

                if (superUsersCount <= 1)
                    throw new InvalidOperationException("Cannot remove the last SuperUser.");
            }
        }

        // نقش‌ها رو از جدول Roles می‌گیریم
        var roleEntities = await _db.Roles.Where(r => roles.Contains(r.Name)).ToListAsync(ct);

        // پاک کردن نقش‌های فعلی
        var current = await _db.UserRoles.Where(x => x.UserId == userId).ToListAsync(ct);
        _db.UserRoles.RemoveRange(current);

        // اضافه کردن نقش‌های جدید
        foreach (var r in roleEntities)
            _db.UserRoles.Add(new UserRole { UserId = userId, RoleId = r.Id });

        await _db.SaveChangesAsync(ct);

        // (اختیاری) ثبت Audit / Notification برای تغییر نقش‌ها
        return await GetUserRolesAsync(userId, ct);
    }
}
