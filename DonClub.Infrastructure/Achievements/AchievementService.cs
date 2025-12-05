using Donclub.Application.Achievements;
using Donclub.Application.Rewards;
using Donclub.Domain.Badges;
using Donclub.Domain.Missions;
using Donclub.Domain.Sessions;
using Donclub.Domain.Wallets;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Donclub.Application.Rewards;

namespace Donclub.Infrastructure.Achievements;

// شرط‌هایی که در ConditionJson ذخیره می‌کنیم
public class SessionEventCondition
{
    /// <summary>
    /// "Player" یا "Manager"
    /// </summary>
    public string AppliesTo { get; set; } = "Player";

    /// <summary>
    /// نوع رویداد. فعلاً فقط "SessionCompleted" استفاده می‌شود.
    /// </summary>
    public string Event { get; set; } = "SessionCompleted";

    /// <summary>
    /// حداقل تعداد کل سشن‌ها (برای این کاربر، بر اساس نوع AppliesTo)
    /// </summary>
    public int? MinTotalSessions { get; set; }

    /// <summary>
    /// حداقل تعداد سشن‌های VIP
    /// </summary>
    public int? MinVipSessions { get; set; }

    /// <summary>
    /// حداقل تعداد سشن‌های CIP
    /// </summary>
    public int? MinCipSessions { get; set; }

    /// <summary>
    /// حداقل تعداد سشن‌ها در همین Game فعلی
    /// </summary>
    public int? MinGameSessions { get; set; }

    /// <summary>
    /// حداقل تعداد سشن‌ها در همین Scenario فعلی
    /// </summary>
    public int? MinScenarioSessions { get; set; }

    /// <summary>
    /// حداقل تعداد سشن‌ها در همین Branch فعلی
    /// </summary>
    public int? MinBranchSessions { get; set; }

    /// <summary>
    /// حداقل تعداد سشن‌ها در همین Room فعلی
    /// </summary>
    public int? MinRoomSessions { get; set; }

    /// <summary>
    /// اگر true باشد، فقط وقتی این رویداد معتبر است که همین سشن VIP باشد.
    /// </summary>
    public bool? RequireCurrentSessionVip { get; set; }

    /// <summary>
    /// اگر true باشد، فقط وقتی این رویداد معتبر است که همین سشن CIP باشد.
    /// </summary>
    public bool? RequireCurrentSessionCip { get; set; }

    /// <summary>
    /// اگر true باشد، فقط وقتی معتبر است که همین سشن مربوط به همین Game باشد (برای MinGameSessions).
    /// </summary>
    public bool? RequireCurrentGame { get; set; }

    /// <summary>
    /// اگر true باشد، فقط وقتی معتبر است که همین سشن مربوط به همین Scenario باشد.
    /// </summary>
    public bool? RequireCurrentScenario { get; set; }

    /// <summary>
    /// فقط وقتی معتبر است که سشن در همین Branch فعلی باشد.
    /// </summary>
    public bool? RequireCurrentBranch { get; set; }

    /// <summary>
    /// فقط وقتی معتبر است که سشن در همین Room فعلی باشد.
    /// </summary>
    public bool? RequireCurrentRoom { get; set; }
}

public class AchievementService : IAchievementService
{
    private readonly DonclubDbContext _db;
    private readonly IRewardService _rewards;

    public AchievementService(DonclubDbContext db, IRewardService rewards)
    {
        _db = db;
        _rewards = rewards;
    }

    public async Task ProcessSessionCompletedAsync(long sessionId, CancellationToken ct = default)
    {
        var session = await _db.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

        if (session == null)
            throw new KeyNotFoundException("Session not found.");

        if (session.Status != SessionStatus.Ended)
            return;

        // Manager
        if (session.ManagerId is not null)
        {
            await ProcessManagerAchievementsAsync(session, ct);
        }

        // Players (فقط بر اساس SessionPlayers موجود)
        var players = await _db.SessionPlayers
            .Where(sp => sp.SessionId == sessionId)
            .ToListAsync(ct);

        if (players.Count > 0)
        {
            await ProcessPlayersAchievementsAsync(session, players, ct);
        }
    }

    // ---------------- Helpers: Metrics ----------------

    /// <summary>
    /// متریک‌های قابل استفاده برای Rule Engine برای یک کاربر در context یک سشن مشخص.
    /// </summary>
    private sealed class SessionAchievementMetrics
    {
        public int TotalSessions { get; init; }
        public int VipSessions { get; init; }
        public int CipSessions { get; init; }
        public int GameSessions { get; init; }
        public int ScenarioSessions { get; init; }
        public int BranchSessions { get; init; }
        public int RoomSessions { get; init; }
    }

    private async Task<SessionAchievementMetrics> BuildMetricsForUserAsync(
        long userId,
        string appliesTo, // "Manager" یا "Player"
        Session currentSession,
        CancellationToken ct)
    {
        // برای Manager مستقیم روی Sessions می‌رویم
        if (string.Equals(appliesTo, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            var sessionsQuery = _db.Sessions
                .Where(s => s.ManagerId == userId && s.Status == SessionStatus.Ended);

            var total = await sessionsQuery.CountAsync(ct);

            var vip = await sessionsQuery
                .Where(s => s.Tier == SessionTier.Vip)
                .CountAsync(ct);

            var cip = await sessionsQuery
                .Where(s => s.Tier == SessionTier.Cip)
                .CountAsync(ct);

            var game = currentSession.GameId != 0
                ? await sessionsQuery.Where(s => s.GameId == currentSession.GameId).CountAsync(ct)
                : 0;

            var scenario = currentSession.ScenarioId is not null
                ? await sessionsQuery.Where(s => s.ScenarioId == currentSession.ScenarioId).CountAsync(ct)
                : 0;

            var branch = await sessionsQuery
                .Where(s => s.BranchId == currentSession.BranchId)
                .CountAsync(ct);

            var room = await sessionsQuery
                .Where(s => s.RoomId == currentSession.RoomId)
                .CountAsync(ct);

            return new SessionAchievementMetrics
            {
                TotalSessions = total,
                VipSessions = vip,
                CipSessions = cip,
                GameSessions = game,
                ScenarioSessions = scenario,
                BranchSessions = branch,
                RoomSessions = room
            };
        }

        // برای Player بر اساس SessionPlayers و join به Sessions
        var spQuery = _db.SessionPlayers
            .Where(sp => sp.PlayerId == userId)
            .Join(
                _db.Sessions,
                sp => sp.SessionId,
                s => s.Id,
                (sp, s) => s);

        var pTotal = await spQuery.CountAsync(ct);

        var pVip = await spQuery
            .Where(s => s.Tier == SessionTier.Vip)
            .CountAsync(ct);

        var pCip = await spQuery
            .Where(s => s.Tier == SessionTier.Cip)
            .CountAsync(ct);

        var pGame = currentSession.GameId != 0
            ? await spQuery.Where(s => s.GameId == currentSession.GameId).CountAsync(ct)
            : 0;

        var pScenario = currentSession.ScenarioId is not null
            ? await spQuery.Where(s => s.ScenarioId == currentSession.ScenarioId).CountAsync(ct)
            : 0;

        var pBranch = await spQuery
            .Where(s => s.BranchId == currentSession.BranchId)
            .CountAsync(ct);

        var pRoom = await spQuery
            .Where(s => s.RoomId == currentSession.RoomId)
            .CountAsync(ct);

        return new SessionAchievementMetrics
        {
            TotalSessions = pTotal,
            VipSessions = pVip,
            CipSessions = pCip,
            GameSessions = pGame,
            ScenarioSessions = pScenario,
            BranchSessions = pBranch,
            RoomSessions = pRoom
        };
    }

    private static bool MatchesCondition(
        SessionEventCondition cond,
        SessionAchievementMetrics metrics,
        Session currentSession,
        string appliesTo)
    {
        // AppliesTo و Event
        if (!string.Equals(cond.AppliesTo ?? "Player", appliesTo, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.Equals(cond.Event ?? "SessionCompleted", "SessionCompleted", StringComparison.OrdinalIgnoreCase))
            return false;

        // متریک‌ها
        if (cond.MinTotalSessions.HasValue && metrics.TotalSessions < cond.MinTotalSessions.Value)
            return false;

        if (cond.MinVipSessions.HasValue && metrics.VipSessions < cond.MinVipSessions.Value)
            return false;

        if (cond.MinCipSessions.HasValue && metrics.CipSessions < cond.MinCipSessions.Value)
            return false;

        if (cond.MinGameSessions.HasValue && metrics.GameSessions < cond.MinGameSessions.Value)
            return false;

        if (cond.MinScenarioSessions.HasValue && metrics.ScenarioSessions < cond.MinScenarioSessions.Value)
            return false;

        if (cond.MinBranchSessions.HasValue && metrics.BranchSessions < cond.MinBranchSessions.Value)
            return false;

        if (cond.MinRoomSessions.HasValue && metrics.RoomSessions < cond.MinRoomSessions.Value)
            return false;

        // محدودیت‌های مربوط به سشن فعلی
        if (cond.RequireCurrentSessionVip == true && currentSession.Tier != SessionTier.Vip)
            return false;

        if (cond.RequireCurrentSessionCip == true && currentSession.Tier != SessionTier.Cip)
            return false;

        if (cond.RequireCurrentGame == true &&
            cond.MinGameSessions.HasValue &&
            currentSession.GameId == 0)
            return false;

        if (cond.RequireCurrentScenario == true &&
            cond.MinScenarioSessions.HasValue &&
            currentSession.ScenarioId is null)
            return false;

        if (cond.RequireCurrentBranch == true &&
            cond.MinBranchSessions.HasValue &&
            currentSession.BranchId == 0)
            return false;

        if (cond.RequireCurrentRoom == true &&
            cond.MinRoomSessions.HasValue &&
            currentSession.RoomId == 0)
            return false;

        return true;
    }

    // ---------------- Manager ----------------

    private async Task ProcessManagerAchievementsAsync(Session session, CancellationToken ct)
    {
        var managerId = session.ManagerId!.Value;
        var now = DateTime.UtcNow;

        // متریک‌ها برای این Manager در context همین سشن
        var metrics = await BuildMetricsForUserAsync(
            userId: managerId,
            appliesTo: "Manager",
            currentSession: session,
            ct: ct);

        await UpdateUserMissionsForEventAsync(
            userId: managerId,
            appliesTo: "Manager",
            now: now,
            metrics: metrics,
            session: session,
            ct: ct);

        await CheckAndGrantBadgesForEventAsync(
            userId: managerId,
            appliesTo: "Manager",
            now: now,
            metrics: metrics,
            session: session,
            ct: ct);
    }

    // ---------------- Players ----------------

    private async Task ProcessPlayersAchievementsAsync(
        Session session,
        List<SessionPlayer> players,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        foreach (var sp in players)
        {
            var playerId = sp.PlayerId;

            var metrics = await BuildMetricsForUserAsync(
                userId: playerId,
                appliesTo: "Player",
                currentSession: session,
                ct: ct);

            await UpdateUserMissionsForEventAsync(
                userId: playerId,
                appliesTo: "Player",
                now: now,
                metrics: metrics,
                session: session,
                ct: ct);

            await CheckAndGrantBadgesForEventAsync(
                userId: playerId,
                appliesTo: "Player",
                now: now,
                metrics: metrics,
                session: session,
                ct: ct);
        }
    }

    // ---------------- Missions (Manager + Player) ----------------

    private async Task UpdateUserMissionsForEventAsync(
        long userId,
        string appliesTo,
        DateTime now,
        SessionAchievementMetrics metrics,
        Session session,
        CancellationToken ct)
    {
        var userMissions = await _db.UserMissions
            .Include(um => um.MissionDefinition)
            .Where(um =>
                um.UserId == userId &&
                !um.IsCompleted &&
                um.PeriodStartUtc <= now &&
                um.PeriodEndUtc >= now &&
                um.MissionDefinition.IsActive)
            .ToListAsync(ct);

        if (!userMissions.Any())
            return;

        foreach (var um in userMissions)
        {
            var cond = ParseCondition(um.MissionDefinition.ConditionJson);

            if (cond != null)
            {
                if (!MatchesCondition(cond, metrics, session, appliesTo))
                    continue;
            }

            // اگر ConditionJson نداشت یا شرط Match شد:
            um.CurrentValue += 1;
            um.LastProgressAtUtc = now;
            um.UpdatedAtUtc = now;

            if (um.CurrentValue >= um.MissionDefinition.TargetValue)
            {
                um.IsCompleted = true;
                um.CompletedAtUtc = now;

                if (um.MissionDefinition.RewardWalletAmount.HasValue &&
                    um.MissionDefinition.RewardWalletAmount.Value > 0)
                {
                    var reward = new RewardWalletRequest(
                        UserId: userId,
                        Amount: um.MissionDefinition.RewardWalletAmount.Value,
                        Description: $"Mission '{um.MissionDefinition.Name}' completed.",
                        Type: (byte)WalletTransactionType.Reward,
                        RelatedMissionId: um.MissionDefinition.Id,
                        RelatedBadgeId: null
                    );

                    await _rewards.CreditRewardAsync(reward, ct);
                }
            }

        }

        await _db.SaveChangesAsync(ct);
    }

    // ---------------- Badges (Manager + Player) ----------------

    private async Task CheckAndGrantBadgesForEventAsync(
        long userId,
        string appliesTo,
        DateTime now,
        SessionAchievementMetrics metrics,
        Session session,
        CancellationToken ct)
    {
        var activeBadges = await _db.Badges
            .Where(b => b.IsActive)
            .ToListAsync(ct);

        var candidates = new List<Badge>();

        foreach (var b in activeBadges)
        {
            var cond = ParseCondition(b.ConditionJson);
            if (cond == null)
                continue;

            if (!MatchesCondition(cond, metrics, session, appliesTo))
                continue;

            candidates.Add(b);
        }

        if (!candidates.Any())
            return;

        var existing = await _db.PlayerBadges
            .Where(pb => pb.UserId == userId && !pb.IsRevoked)
            .ToListAsync(ct);

        var existingIds = existing.Select(pb => pb.BadgeId).ToHashSet();

        var toAdd = new List<PlayerBadge>();

        foreach (var badge in candidates)
        {
            if (existingIds.Contains(badge.Id))
                continue;

            toAdd.Add(new PlayerBadge
            {
                UserId = userId,
                BadgeId = badge.Id,
                EarnedAtUtc = now,
                CreatedAtUtc = now,
                IsRevoked = false,
                Reason = "Auto granted by SessionCompleted event."
            });
        }

        if (toAdd.Count > 0)
        {
            _db.PlayerBadges.AddRange(toAdd);
            await _db.SaveChangesAsync(ct);

            // 🎁 پاداش کیف پول برای هر Badge جدید (در صورت وجود RewardWalletAmount)
            foreach (var pb in toAdd)
            {
                if (pb.Badge.RewardWalletAmount.HasValue &&
                    pb.Badge.RewardWalletAmount.Value > 0)
                {
                    var reward = new RewardWalletRequest(
                        UserId: userId,
                        Amount: pb.Badge.RewardWalletAmount.Value,
                        Description: $"Badge '{pb.Badge.Name}' granted.",
                        Type: (byte)WalletTransactionType.Reward,
                        RelatedMissionId: null,
                        RelatedBadgeId: pb.BadgeId
                    );

                    await _rewards.CreditRewardAsync(reward, ct);
                }
            }

        }
    }

    // ---------------- Helpers ----------------

    private static SessionEventCondition? ParseCondition(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<SessionEventCondition>(json);
        }
        catch
        {
            // اگر JSON خراب باشد، شرط را نادیده می‌گیریم
            return null;
        }
    }
}
