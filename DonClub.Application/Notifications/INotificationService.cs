using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Donclub.Application.Notifications;

public interface INotificationService
{
    Task<long> CreateAsync(CreateNotificationRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(
        long userId,
        bool onlyUnread,
        CancellationToken ct = default);

    Task MarkAsReadAsync(long notificationId, long userId, CancellationToken ct = default);

    Task MarkAllAsReadAsync(long userId, CancellationToken ct = default);
}
