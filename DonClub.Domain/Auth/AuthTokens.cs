namespace Donclub.Domain.Auth;

public class RefreshToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Token { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class SmsOtp
{
    public long Id { get; set; }
    public string PhoneNumber { get; set; } = default!;
    public string Code { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
