using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Donclub.Application.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Donclub.Infrastructure.Auth;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _config;

    public JwtTokenGenerator(IConfiguration config)
    {
        _config = config;
    }

    public (string token, DateTime expiresAtUtc) GenerateAccessToken(long userId, string phoneNumber, IEnumerable<string> roles)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(30); // فعلاً ۳۰ دقیقه

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.MobilePhone, phoneNumber)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expires);
    }

    public (string token, DateTime expiresAtUtc) GenerateRefreshToken(long userId)
    {
        var bytes = new byte[32];
        Random.Shared.NextBytes(bytes);
        var token = Convert.ToBase64String(bytes);
        var expires = DateTime.UtcNow.AddDays(7); // فعلاً ۷ روز
        return (token, expires);
    }
}
