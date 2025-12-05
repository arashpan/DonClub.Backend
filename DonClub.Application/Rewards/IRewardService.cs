using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Rewards;

public interface IRewardService
{
    /// <summary>
    /// Credits a wallet reward for a user — used by Achievements (missions/badges).
    /// </summary>
    Task CreditRewardAsync(RewardWalletRequest request, CancellationToken ct = default);
}

public record RewardWalletRequest(
    long UserId,
    decimal Amount,
    string Description,
    byte Type,              // WalletTransactionType.Reward
    long? RelatedMissionId,
    long? RelatedBadgeId
);
