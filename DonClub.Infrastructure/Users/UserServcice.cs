using Donclub.Application.Users;
using Donclub.Domain.Users;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Donclub.Infrastructure.Users;

public class UserService : IUserService
{
    private readonly DonclubDbContext _db;

    public UserService(DonclubDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<UserListItemDto>> GetAllAsync(string? search, string? role, CancellationToken ct = default)
    {
        var query = _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(u =>
                u.PhoneNumber.Contains(search) ||
                (u.DisplayName != null && u.DisplayName.Contains(search)) ||
                (u.UserName != null && u.UserName.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var normalizedRole = role.Trim().ToLower();
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name.ToLower() == normalizedRole));
        }

        return await query
            .OrderByDescending(u => u.CreatedAtUtc)
            .Select(u => new UserListItemDto(
                u.Id,
                u.PhoneNumber,
                u.DisplayName,
                u.IsActive,
                u.UserRoles.Select(ur => ur.Role.Name).ToArray(),
                u.MembershipLevel
            ))
            .ToListAsync(ct);
    }

    public async Task<UserDetailDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user == null) return null;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();

        return new UserDetailDto(
            user.Id,
            user.UserName,
            user.PhoneNumber,
            user.DisplayName,
            user.Email,
            user.IsActive,
            user.PhoneNumberConfirmed,
            user.MembershipLevel,
            roles,
            user.CreatedAtUtc,
            user.UpdatedAtUtc
        );
    }

    public async Task<long> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var phone = NormalizePhone(request.PhoneNumber);

        var existing = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone, ct);
        if (existing != null)
        {
            // اگر کاربر با این شماره وجود داشته باشد، نقش‌ها و اطلاعاتش را به‌روزرسانی می‌کنیم
            existing.DisplayName = request.DisplayName ?? existing.DisplayName;
            existing.Email = request.Email ?? existing.Email;
            existing.MembershipLevel = request.MembershipLevel;
            existing.IsActive = true;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            await SetRolesInternal(existing, request.Roles, ct);
            await _db.SaveChangesAsync(ct);
            return existing.Id;
        }

        var user = new User
        {
            UserName = phone,
            PhoneNumber = phone,
            DisplayName = request.DisplayName,
            Email = request.Email,
            MembershipLevel = request.MembershipLevel,
            IsActive = true,
            PhoneNumberConfirmed = false, // تا وقتی با OTP وارد نشه
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        await SetRolesInternal(user, request.Roles, ct);
        await _db.SaveChangesAsync(ct);

        return user.Id;
    }

    public async Task UpdateAsync(long id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        user.DisplayName = request.DisplayName ?? user.DisplayName;
        user.Email = request.Email ?? user.Email;
        user.IsActive = request.IsActive;
        user.MembershipLevel = request.MembershipLevel;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateRolesAsync(long id, UpdateUserRolesRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        await SetRolesInternal(user, request.Roles, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SetActiveAsync(long id, bool isActive, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new KeyNotFoundException("User not found.");

        user.IsActive = isActive;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    // -------------------- Helpers --------------------

    private async Task SetRolesInternal(User user, string[] roles, CancellationToken ct)
    {
        var normalized = roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var existingRoles = await _db.Roles
            .Where(r => normalized.Contains(r.Name))
            .ToListAsync(ct);

        var missing = normalized
            .Except(existingRoles.Select(r => r.Name), StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (missing.Length > 0)
        {
            throw new InvalidOperationException($"Roles not found: {string.Join(", ", missing)}");
        }

        // پاک کردن نقش‌های قبلی
        var oldUserRoles = _db.UserRoles.Where(ur => ur.UserId == user.Id);
        _db.UserRoles.RemoveRange(oldUserRoles);

        // اضافه کردن نقش‌های جدید
        foreach (var role in existingRoles)
        {
            _db.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });
        }
    }

    private static string NormalizePhone(string phone)
    {
        phone = phone.Trim();
        if (phone.StartsWith("0")) phone = phone[1..];
        if (phone.StartsWith("+98")) phone = phone[3..];
        if (!phone.StartsWith("98"))
            phone = "98" + phone;
        return phone;
    }
}
