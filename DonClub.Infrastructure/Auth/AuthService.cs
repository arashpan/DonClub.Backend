using Donclub.Application.Auth;
using Donclub.Application.Common;
using Donclub.Domain.Auth;
using Donclub.Domain.Users;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Donclub.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly DonclubDbContext _db;
    private readonly IJwtTokenGenerator _jwt;
    private readonly ISmsSender _sms;
    private readonly IDateTimeProvider _clock;

    public AuthService(DonclubDbContext db, IJwtTokenGenerator jwt, ISmsSender sms, IDateTimeProvider clock)
    {
        _db = db;
        _jwt = jwt;
        _sms = sms;
        _clock = clock;
    }

    public async Task<RequestOtpResultDto> RequestOtpAsync(string phoneNumber, CancellationToken ct = default)
    {
        phoneNumber = NormalizePhone(phoneNumber);

        // OTP شش رقمی
        var code = Random.Shared.Next(100000, 999999).ToString();
        var expires = _clock.UtcNow.AddMinutes(3);

        // قبلی‌ها رو غیرفعال می‌کنیم
        var existing = await _db.SmsOtps
            .Where(x => x.PhoneNumber == phoneNumber && !x.IsUsed && x.ExpiresAtUtc > _clock.UtcNow)
            .ToListAsync(ct);

        foreach (var otp in existing)
            otp.IsUsed = true;

        var entity = new SmsOtp
        {
            PhoneNumber = phoneNumber,
            Code = code,
            ExpiresAtUtc = expires,
            IsUsed = false,
            CreatedAtUtc = _clock.UtcNow
        };

        _db.SmsOtps.Add(entity);
        await _db.SaveChangesAsync(ct);

        await _sms.SendAsync(phoneNumber, $"کد ورود شما به دان‌کلاب: {code}", ct);

        return new RequestOtpResultDto(phoneNumber, expires);
    }

    public async Task<AuthResultDto> VerifyOtpAsync(string phoneNumber, string code, CancellationToken ct = default)
    {
        phoneNumber = NormalizePhone(phoneNumber);

        var now = _clock.UtcNow;

        var otp = await _db.SmsOtps
            .Where(x => x.PhoneNumber == phoneNumber && x.Code == code && !x.IsUsed && x.ExpiresAtUtc >= now)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (otp == null)
            throw new InvalidOperationException("کد معتبر نیست یا منقضی شده است.");

        otp.IsUsed = true;

        // پیدا کردن یا ساختن User
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, ct);

        if (user == null)
        {
            user = new User
            {
                UserName = phoneNumber,
                PhoneNumber = phoneNumber,
                PhoneNumberConfirmed = true,
                MembershipLevel = MembershipLevel.Guest,
                IsActive = true
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);

            // نقش Player به صورت پیش‌فرض
            var playerRole = await _db.Roles.FirstAsync(r => r.Name == "Player", ct);
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = playerRole.Id });
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            user.PhoneNumberConfirmed = true;
            await _db.SaveChangesAsync(ct);
        }

        // در این مرحله، برای اینکه مطمئن باشیم Roleها همیشه درستند (حتی در اولین لاگین):
        var roles = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name)
            .ToArrayAsync(ct);

        // ساخت اکسس و رفرش توکن
        var (access, accessExp) = _jwt.GenerateAccessToken(user.Id, phoneNumber, roles);
        var (refresh, refreshExp) = _jwt.GenerateRefreshToken(user.Id);

        var refreshEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refresh,
            ExpiresAtUtc = refreshExp,
            IsRevoked = false,
            CreatedAtUtc = now
        };
        _db.RefreshTokens.Add(refreshEntity);
        await _db.SaveChangesAsync(ct);

        var tokensDto = new AuthTokensDto(access, refresh, accessExp, refreshExp);
        var userDto = new UserDto(user.Id, user.PhoneNumber, user.DisplayName, roles);
        return new AuthResultDto(userDto, tokensDto);
    }

    public async Task<AuthResultDto> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var now = _clock.UtcNow;

        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken && !x.IsRevoked && x.ExpiresAtUtc >= now, ct);

        if (token == null)
            throw new InvalidOperationException("رفرش توکن معتبر نیست.");

        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == token.UserId, ct);

        if (user == null || !user.IsActive)
            throw new InvalidOperationException("کاربر غیرفعال است.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();

        var (access, accessExp) = _jwt.GenerateAccessToken(user.Id, user.PhoneNumber, roles);
        var (newRefresh, newRefreshExp) = _jwt.GenerateRefreshToken(user.Id);

        token.IsRevoked = true;

        var newToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefresh,
            ExpiresAtUtc = newRefreshExp,
            IsRevoked = false,
            CreatedAtUtc = now
        };

        _db.RefreshTokens.Add(newToken);
        await _db.SaveChangesAsync(ct);

        var tokensDto = new AuthTokensDto(access, newRefresh, accessExp, newRefreshExp);
        var userDto = new UserDto(user.Id, user.PhoneNumber, user.DisplayName, roles);
        return new AuthResultDto(userDto, tokensDto);
    }

    private static string NormalizePhone(string phone)
    {
        phone = phone.Trim();
        if (phone.StartsWith("0")) phone = phone[1..];
        if (phone.StartsWith("+98")) phone = phone[3..];

        if (!phone.StartsWith("9") && phone.Length == 10 && phone[0] != '9')
            throw new InvalidOperationException("شماره موبایل نامعتبر است.");

        // ذخیره به صورت 98xxxxxxxxxx
        if (!phone.StartsWith("98"))
            phone = "98" + phone;

        return phone;
    }
}
