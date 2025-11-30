using System.Collections.Generic;
using System.Security.Claims;

namespace Donclub.Application.Auth;

public interface IJwtTokenGenerator
{
    (string token, DateTime expiresAtUtc) GenerateAccessToken(long userId, string phoneNumber, IEnumerable<string> roles);
    (string token, DateTime expiresAtUtc) GenerateRefreshToken(long userId);
}
