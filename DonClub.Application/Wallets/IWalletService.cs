using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Wallets;

public interface IWalletService
{
    Task<WalletDto> GetOrCreateWalletForUserAsync(long userId, CancellationToken ct = default);
    Task<WalletDto?> GetWalletByUserIdAsync(long userId, CancellationToken ct = default);
    Task<IReadOnlyList<WalletTransactionDto>> GetTransactionsAsync(long userId, int skip, int take, CancellationToken ct = default);

    Task<WalletDto> CreditAsync(long userId, CreditWalletRequest request, CancellationToken ct = default);
    Task<WalletDto> DebitAsync(long userId, DebitWalletRequest request, CancellationToken ct = default);
}
