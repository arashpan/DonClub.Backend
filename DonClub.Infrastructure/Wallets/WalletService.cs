using Donclub.Application.Wallets;
using Donclub.Domain.Wallets;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Donclub.Application.Notifications;
using Donclub.Domain.Notifications;

namespace Donclub.Infrastructure.Wallets;

public class WalletService : IWalletService
{
    private readonly DonclubDbContext _db;
    private readonly INotificationService _notifications;

    public WalletService(DonclubDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<WalletDto> GetOrCreateWalletForUserAsync(long userId, CancellationToken ct = default)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, ct);
        if (wallet == null)
        {
            wallet = new Wallet
            {
                UserId = userId,
                Balance = 0,
                IsLocked = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Wallets.Add(wallet);
            await _db.SaveChangesAsync(ct);
        }

        return MapWallet(wallet);
    }

    public async Task<WalletDto?> GetWalletByUserIdAsync(long userId, CancellationToken ct = default)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, ct);
        return wallet == null ? null : MapWallet(wallet);
    }

    public async Task<IReadOnlyList<WalletTransactionDto>> GetTransactionsAsync(long userId, int skip, int take, CancellationToken ct = default)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, ct);
        if (wallet == null)
            return Array.Empty<WalletTransactionDto>();

        return await _db.WalletTransactions
            .Where(t => t.WalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .Select(t => new WalletTransactionDto(
                t.Id,
                t.Amount,
                t.BalanceAfter,
                (byte)t.Type,
                (byte)t.Direction,
                t.RelatedSessionId,
                t.RelatedUserId,
                t.Description,
                t.CreatedAtUtc
            ))
            .ToListAsync(ct);
    }

    public async Task<WalletDto> CreditAsync(long userId, CreditWalletRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new InvalidOperationException("Amount must be positive.");

        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId, ct);

        if (wallet == null)
        {
            wallet = new Wallet
            {
                UserId = userId,
                Balance = 0,
                IsLocked = false,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.Wallets.Add(wallet);
            await _db.SaveChangesAsync(ct);
        }

        if (wallet.IsLocked)
            throw new InvalidOperationException("Wallet is locked.");

        wallet.Balance += request.Amount;
        wallet.UpdatedAtUtc = DateTime.UtcNow;

        var tx = new WalletTransaction
        {
            WalletId = wallet.Id,
            Amount = request.Amount,
            BalanceAfter = wallet.Balance,
            Type = (WalletTransactionType)request.Type,
            Direction = WalletTransactionDirection.Credit,
            RelatedSessionId = request.RelatedSessionId,
            RelatedUserId = request.RelatedUserId,
            Description = request.Description,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.WalletTransactions.Add(tx);
        await _db.SaveChangesAsync(ct);
        // اگر نوع تراکنش Reward نباشد، نوتیفیکیشن عمومی کیف پول می‌فرستیم
        if ((WalletTransactionType)request.Type != WalletTransactionType.Reward)
        {
            var title = "شارژ کیف پول";
            var msg = $"کیف پول شما به مبلغ {request.Amount} شارژ شد. موجودی فعلی: {wallet.Balance}";

            await _notifications.CreateAsync(new CreateNotificationRequest(
                UserId: userId,
                Title: title,
                Message: msg,
                Type: (byte)NotificationType.WalletCredited,
                DataJson: null // در صورت نیاز بعداً می‌تونیم missionId/sessionId و ... را JSON کنیم
            ), ct);
        }

        return MapWallet(wallet);
    }

    public async Task<WalletDto> DebitAsync(long userId, DebitWalletRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new InvalidOperationException("Amount must be positive.");

        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId, ct)
            ?? throw new InvalidOperationException("Wallet not found for user.");

        if (wallet.IsLocked)
            throw new InvalidOperationException("Wallet is locked.");

        if (wallet.Balance < request.Amount)
            throw new InvalidOperationException("Insufficient balance.");

        wallet.Balance -= request.Amount;
        wallet.UpdatedAtUtc = DateTime.UtcNow;

        var tx = new WalletTransaction
        {
            WalletId = wallet.Id,
            Amount = request.Amount,
            BalanceAfter = wallet.Balance,
            Type = (WalletTransactionType)request.Type,
            Direction = WalletTransactionDirection.Debit,
            RelatedSessionId = request.RelatedSessionId,
            RelatedUserId = request.RelatedUserId,
            Description = request.Description,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.WalletTransactions.Add(tx);
        await _db.SaveChangesAsync(ct);
        if ((WalletTransactionType)request.Type != WalletTransactionType.Reward)
        {
            var title = "برداشت از کیف پول";
            var msg = $"به مبلغ {request.Amount} از کیف پول شما برداشت شد. موجودی فعلی: {wallet.Balance}";

            await _notifications.CreateAsync(new CreateNotificationRequest(
                UserId: userId,
                Title: title,
                Message: msg,
                Type: (byte)NotificationType.WalletCredited, // یا نوع جداگانه اگر تعریف کنی
                DataJson: null
            ), ct);
        }

        return MapWallet(wallet);
    }

    private static WalletDto MapWallet(Wallet w) =>
        new WalletDto(w.Id, w.UserId, w.Balance, w.IsLocked);
}
