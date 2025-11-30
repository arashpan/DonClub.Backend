namespace Donclub.Application.Wallets;

public record WalletDto(
    long Id,
    long UserId,
    decimal Balance,
    bool IsLocked
);

public record WalletTransactionDto(
    long Id,
    decimal Amount,
    decimal BalanceAfter,
    byte Type,
    byte Direction,
    long? RelatedSessionId,
    long? RelatedUserId,
    string? Description,
    DateTime CreatedAtUtc
);

// Requests
public record CreditWalletRequest(
    decimal Amount,
    byte Type,
    string? Description,
    long? RelatedSessionId,
    long? RelatedUserId
);

public record DebitWalletRequest(
    decimal Amount,
    byte Type,
    string? Description,
    long? RelatedSessionId,
    long? RelatedUserId
);
