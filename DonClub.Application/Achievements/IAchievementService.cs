using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Achievements
{
    public interface IAchievementService
    {
        /// <summary>
        /// Called after a session has been marked as Ended.
        /// This method updates achievements (missions/badges) for both
        /// the manager and participating players.
        /// </summary>
        Task ProcessSessionCompletedAsync(long sessionId, CancellationToken ct = default);
    }
}
