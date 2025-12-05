using Donclub.Application.Notifications;
using Donclub.Domain.Notifications;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Donclub.Infrastructure.Notifications;

public class NotificationService : INotificationService
{
    private readonly DonclubDbContext _db;

    public NotificationService(DonclubDbContext db)
    {
        _db = db;
    }

    public async Task<long> CreateAsync(CreateNotificationRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var entity = new Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = (NotificationType)request.Type,
            DataJson = request.DataJson,
            IsRead = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = null
        };

        _db.Notifications.Add(entity);
        await _db.SaveChangesAsync(ct);

        return entity.Id;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(
        long userId,
        bool onlyUnread,
        CancellationToken ct = default)
    {
        var query = _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .AsQueryable();

        if (onlyUnread)
            query = query.Where(n => !n.IsRead);

        return await query
            .Select(n => new NotificationDto(
                n.Id,
                n.Title,
                n.Message,
                (byte)n.Type,
                n.IsRead,
                n.CreatedAtUtc,
                n.ReadAtUtc,
                n.DataJson
            ))
            .ToListAsync(ct);
    }

    public async Task MarkAsReadAsync(long notificationId, long userId, CancellationToken ct = default)
    {
        var n = await _db.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, ct);

        if (n == null)
            return;

        if (!n.IsRead)
        {
            n.IsRead = true;
            n.ReadAtUtc = DateTime.UtcNow;
            n.UpdatedAtUtc = n.ReadAtUtc;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task MarkAllAsReadAsync(long userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        if (!unread.Any())
            return;

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAtUtc = now;
            n.UpdatedAtUtc = now;
        }

        await _db.SaveChangesAsync(ct);
    }
}
