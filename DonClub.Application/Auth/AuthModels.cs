namespace Donclub.Application.Auth;

public record AuthTokensDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc
);

public record UserDto(
    long Id,
    string PhoneNumber,
    string? DisplayName,
    string[] Roles
);

public record AuthResultDto(
    UserDto User,
    AuthTokensDto Tokens
);

public record RequestOtpResultDto(
    string PhoneNumber,
    DateTime ExpiresAtUtc
);
