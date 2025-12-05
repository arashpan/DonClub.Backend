using Donclub.Application.Rewards;
using Donclub.Application.Wallets;
using Donclub.Domain.Wallets;
using Donclub.Infrastructure.Persistence;

namespace Donclub.Infrastructure.Rewards;

public class RewardService : IRewardService
{
    private readonly DonclubDbContext _db;
    private readonly IWalletService _wallets;

    public RewardService(DonclubDbContext db, IWalletService wallets)
    {
        _db = db;
        _wallets = wallets;
    }

    public async Task CreditRewardAsync(RewardWalletRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            return;

        // Build a standard CreditWalletRequest (compatible with your real WalletService)
        var credit = new CreditWalletRequest(
            Amount: request.Amount,
            Type: (byte)WalletTransactionType.Reward, // reward = 2
            Description: request.Description,
            RelatedSessionId: null,
            RelatedUserId: null
        );

        // This uses the *real* wallet logic of your project
        await _wallets.CreditAsync(request.UserId, credit, ct);
    }
}
