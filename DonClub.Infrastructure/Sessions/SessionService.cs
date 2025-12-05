using Donclub.Application.Sessions;
using Donclub.Application.Achievements;
using Donclub.Domain.Sessions;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Donclub.Application.Notifications;
using Donclub.Domain.Notifications;
using System.Text.Json;


namespace Donclub.Infrastructure.Sessions;

public class SessionService : ISessionService
{
    private readonly DonclubDbContext _db;
    private readonly INotificationService _notifications;
    private readonly IAchievementService _achievements;

    public SessionService(DonclubDbContext db, IAchievementService achievements, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
        _achievements = achievements;
    }

    // -------------------- Create / Update / Status --------------------

    public async Task<long> CreateAsync(CreateSessionRequest request, CancellationToken ct = default)
    {
        // می‌تونی اینجا ولیدیشن‌هایی مثل عدم تداخل اتاق/ساعت رو اضافه کنی (بعداً)
        var entity = new Session
        {
            BranchId = request.BranchId,
            RoomId = request.RoomId,
            GameId = request.GameId,
            ScenarioId = request.ScenarioId,
            ManagerId = request.ManagerId,
            StartTimeUtc = request.StartTimeUtc,
            EndTimeUtc = request.EndTimeUtc,
            Status = SessionStatus.Planned,
            Tier = (SessionTier)request.Tier,
            MaxPlayers = request.MaxPlayers,
            Notes = request.Notes,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Sessions.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task UpdateAsync(long id, UpdateSessionRequest request, CancellationToken ct = default)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new KeyNotFoundException("Session not found.");

        // برای سشن Ended/Canceled می‌تونی اجازهٔ ویرایش ندی (بعداً)
        session.BranchId = request.BranchId;
        session.RoomId = request.RoomId;
        session.GameId = request.GameId;
        session.ScenarioId = request.ScenarioId;
        session.ManagerId = request.ManagerId;
        session.StartTimeUtc = request.StartTimeUtc;
        session.EndTimeUtc = request.EndTimeUtc;
        session.Tier = (SessionTier)request.Tier;
        session.MaxPlayers = request.MaxPlayers;
        session.Notes = request.Notes;
        session.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        // بعد از SaveChangesAsync

        session = await _db.Sessions
            .Include(s => s.Players)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (session != null)
        {
            var playerIds = session.Players
                .Select(sp => sp.PlayerId)
                .Distinct()
                .ToList();

            var title = "برنامه‌ی یک سشن تغییر کرد";
            var message = $"سشن #{session.Id} به‌روزرسانی شد.";

            var payload = JsonSerializer.Serialize(new
            {
                sessionId = session.Id,
                branchId = session.BranchId,
                roomId = session.RoomId,
                startTimeUtc = session.StartTimeUtc,
                endTimeUtc = session.EndTimeUtc
            });

            foreach (var pid in playerIds)
            {
                await _notifications.CreateAsync(
                    new CreateNotificationRequest(
                        UserId: pid,
                        Title: title,
                        Message: message,
                        Type: (byte)NotificationType.SessionUpdated,
                        DataJson: payload
                    ),
                    ct);
            }

            if (session.ManagerId is not null)
            {
                await _notifications.CreateAsync(
                    new CreateNotificationRequest(
                        UserId: session.ManagerId.Value,
                        Title: title,
                        Message: message,
                        Type: (byte)NotificationType.SessionUpdated,
                        DataJson: payload
                    ),
                    ct);
            }
        }

    }

    public async Task CancelAsync(long id, CancellationToken ct = default)
    {
        // سشن را همراه با بازیکن‌ها لود می‌کنیم
        var session = await _db.Sessions
            .Include(s => s.Players)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new KeyNotFoundException("Session not found.");

        // اگر قبلاً کنسل شده، دوباره کاری نکنیم
        if (session.Status == SessionStatus.Canceled)
            return;

        session.Status = SessionStatus.Canceled;
        session.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // حالا نوتیفیکیشن‌ها را برای بازیکن‌ها و منیجر می‌سازیم
        var playerIds = session.Players
            .Select(sp => sp.PlayerId)
            .Distinct()
            .ToList();

        var title = "یک سشن کنسل شد";
        var message = $"سشن #{session.Id} کنسل شد.";

        var payload = JsonSerializer.Serialize(new
        {
            sessionId = session.Id,
            branchId = session.BranchId,
            roomId = session.RoomId,
            startTimeUtc = session.StartTimeUtc,
            endTimeUtc = session.EndTimeUtc
        });

        // برای همه‌ی بازیکن‌ها
        foreach (var pid in playerIds)
        {
            await _notifications.CreateAsync(
                new CreateNotificationRequest(
                    UserId: pid,
                    Title: title,
                    Message: message,
                    Type: (byte)NotificationType.SessionCanceled,
                    DataJson: payload
                ),
                ct);
        }

        // برای منیجر (اگر سشن منیجر دارد)
        if (session.ManagerId is not null)
        {
            await _notifications.CreateAsync(
                new CreateNotificationRequest(
                    UserId: session.ManagerId.Value,
                    Title: title,
                    Message: message,
                    Type: (byte)NotificationType.SessionCanceled,
                    DataJson: payload
                ),
                ct);
        }
    }


    public async Task ChangeStatusAsync(long id, byte status, CancellationToken ct = default)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new KeyNotFoundException("Session not found.");

        session.Status = (SessionStatus)status;
        session.UpdatedAtUtc = DateTime.UtcNow;
        
        if (session.Status == SessionStatus.Ended)
        {
            await _achievements.ProcessSessionCompletedAsync(session.Id, ct);
        }
        await _db.SaveChangesAsync(ct);

    }

    // -------------------- Query --------------------

    public async Task<SessionDetailDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var session = await _db.Sessions
            .Include(s => s.Players).ThenInclude(sp => sp.Player)
            .Include(s => s.Game)
            .Include(s => s.Scenario)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (session == null) return null;

        var players = await _db.SessionPlayers
            .Where(sp => sp.SessionId == id)
            .Select(sp => new SessionPlayerDto(
                sp.PlayerId,
                sp.Player.DisplayName,
                sp.Player.PhoneNumber,
                (byte)sp.Status,
                _db.Scores
                    .Where(sc => sc.SessionId == sp.SessionId && sc.PlayerId == sp.PlayerId)
                    .Select(sc => (int?)sc.Value)
                    .FirstOrDefault()
            ))
            .ToListAsync(ct);

        return new SessionDetailDto(
            session.Id,
            session.BranchId,
            session.RoomId,
            session.GameId,
            session.ScenarioId,
            session.ManagerId,
            session.StartTimeUtc,
            session.EndTimeUtc,
            (byte)session.Status,
            (byte)session.Tier,
            session.MaxPlayers,
            session.Notes,
            players
        );
    }

    public async Task<IReadOnlyList<SessionSummaryDto>> GetByBranchAndDateAsync(int branchId, DateOnly date, CancellationToken ct = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        return await _db.Sessions
            .Where(s => s.BranchId == branchId && s.StartTimeUtc >= start && s.StartTimeUtc < end)
            .OrderBy(s => s.StartTimeUtc)
            .Select(s => new SessionSummaryDto(
                s.Id,
                s.BranchId,
                s.RoomId,
                s.GameId,
                s.ScenarioId,
                s.ManagerId,
                s.StartTimeUtc,
                s.EndTimeUtc,
                (byte)s.Status,
                (byte)s.Tier
            ))
            .ToListAsync(ct);
    }

    // -------------------- Players --------------------

    public async Task AddPlayerAsync(long sessionId, long playerId, CancellationToken ct = default)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new KeyNotFoundException("Session not found.");

        var playerExists = await _db.Users.AnyAsync(u => u.Id == playerId, ct);
        if (!playerExists)
            throw new KeyNotFoundException("Player not found.");

        var exists = await _db.SessionPlayers.AnyAsync(sp => sp.SessionId == sessionId && sp.PlayerId == playerId, ct);
        if (exists)
            return;

        var spEntity = new SessionPlayer
        {
            SessionId = sessionId,
            PlayerId = playerId,
            ReservedAtUtc = DateTime.UtcNow,
            Status = SessionPlayerStatus.Registered
        };

        _db.SessionPlayers.Add(spEntity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemovePlayerAsync(long sessionId, long playerId, CancellationToken ct = default)
    {
        var sp = await _db.SessionPlayers
            .FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.PlayerId == playerId, ct);

        if (sp == null) return;

        _db.SessionPlayers.Remove(sp);

        // اگر نخواستی ردیف Score باقی بمونه:
        var scores = await _db.Scores
            .Where(sc => sc.SessionId == sessionId && sc.PlayerId == playerId)
            .ToListAsync(ct);
        _db.Scores.RemoveRange(scores);

        await _db.SaveChangesAsync(ct);
    }

    // -------------------- Scores --------------------

    public async Task SetPlayerScoreAsync(long sessionId, long playerId, int score, CancellationToken ct = default)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new KeyNotFoundException("Session not found.");

        var player = await _db.SessionPlayers
            .FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.PlayerId == playerId, ct)
            ?? throw new InvalidOperationException("Player is not in this session.");

        var existing = await _db.Scores
            .FirstOrDefaultAsync(sc => sc.SessionId == sessionId && sc.PlayerId == playerId, ct);

        if (existing == null)
        {
            existing = new Score
            {
                SessionId = sessionId,
                PlayerId = playerId,
                Value = score,
                EnteredByManagerId = session.ManagerId ?? 0,
                EnteredAtUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.Scores.Add(existing);
        }
        else
        {
            existing.Value = score;
            existing.LastEditedAtUtc = DateTime.UtcNow;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SessionSummaryDto>> GetByPlayerAsync(
    long playerId,
    DateTime? fromUtc,
    DateTime? toUtc,
    CancellationToken ct = default)
    {
        var query = _db.Sessions.AsQueryable();

        // فقط سشن‌هایی که این کاربر داخل‌شون Player است
        query = query.Where(s =>
            _db.SessionPlayers.Any(sp => sp.SessionId == s.Id && sp.PlayerId == playerId));

        if (fromUtc.HasValue)
            query = query.Where(s => s.StartTimeUtc >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(s => s.StartTimeUtc < toUtc.Value);

        return await query
            .OrderBy(s => s.StartTimeUtc)
            .Select(s => new SessionSummaryDto(
                s.Id,
                s.BranchId,
                s.RoomId,
                s.GameId,
                s.ScenarioId,
                s.ManagerId,
                s.StartTimeUtc,
                s.EndTimeUtc,
                (byte)s.Status,
                (byte)s.Tier
            ))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SessionSummaryDto>> GetByManagerAsync(
        long managerId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken ct = default)
    {
        var query = _db.Sessions
            .Where(s => s.ManagerId == managerId);

        if (fromUtc.HasValue)
            query = query.Where(s => s.StartTimeUtc >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(s => s.StartTimeUtc < toUtc.Value);

        return await query
            .OrderBy(s => s.StartTimeUtc)
            .Select(s => new SessionSummaryDto(
                s.Id,
                s.BranchId,
                s.RoomId,
                s.GameId,
                s.ScenarioId,
                s.ManagerId,
                s.StartTimeUtc,
                s.EndTimeUtc,
                (byte)s.Status,
                (byte)s.Tier
            ))
            .ToListAsync(ct);
    }

}
