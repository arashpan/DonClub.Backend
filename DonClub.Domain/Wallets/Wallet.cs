using Donclub.Domain.Common;
using Donclub.Domain.Users;

namespace Donclub.Domain.Wallets;

public class Wallet : BaseEntity<long>, IAuditableEntity
{
    public long UserId { get; set; }
    public decimal Balance { get; set; }   // e.g. decimal(18,2)
    public bool IsLocked { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public User User { get; set; } = default!;
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}

public enum WalletTransactionType : byte
{
    Unknown = 0,
    GameFee = 1,          // هزینه شرکت در بازی
    Reward = 2,           // پاداش
    ManagerCommission = 3,
    Penalty = 4,
    Refund = 5,
    ManualAdjustment = 6  // تنظیم دستی
}

public enum WalletTransactionDirection : byte
{
    Credit = 0,   // ورود پول به کیف
    Debit = 1     // خروج پول از کیف
}

public class WalletTransaction : BaseEntity<long>, IAuditableEntity
{
    public long WalletId { get; set; }

    public decimal Amount { get; set; }          // همیشه مثبت
    public decimal BalanceAfter { get; set; }    // موجودی بعد از این تراکنش

    public WalletTransactionType Type { get; set; }
    public WalletTransactionDirection Direction { get; set; }

    public long? RelatedSessionId { get; set; }  // optional link to Session
    public long? RelatedUserId { get; set; }     // e.g. admin who made manual adjustment

    public string? Description { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public Wallet Wallet { get; set; } = default!;
}
