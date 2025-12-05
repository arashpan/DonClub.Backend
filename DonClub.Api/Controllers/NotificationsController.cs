using System.Security.Claims;
using Donclub.Application.Notifications;
using Donclub.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Donclub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications)
    {
        _notifications = notifications;
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new InvalidOperationException("UserId not found in token.");

        return long.Parse(userIdClaim);
    }

    // GET /api/notifications/my?onlyUnread=true
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetMyNotifications(
        [FromQuery] bool onlyUnread = false,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var items = await _notifications.GetUserNotificationsAsync(userId, onlyUnread, ct);
        return Ok(items);
    }

    // POST /api/notifications/my/{id}/read
    [HttpPost("my/{id:long}/read")]
    [Authorize]
    public async Task<ActionResult> MarkMyNotificationAsRead(long id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _notifications.MarkAsReadAsync(id, userId, ct);
        return NoContent();
    }

    // POST /api/notifications/my/read-all
    [HttpPost("my/read-all")]
    [Authorize]
    public async Task<ActionResult> MarkAllMyNotificationsAsRead(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _notifications.MarkAllAsReadAsync(userId, ct);
        return NoContent();
    }

    // (اختیاری) ادمین/سوپریوزر بتونه نوتیفیکیشن دستی بفرسته
    [HttpPost]
    [Authorize(Roles = AppRoles.SuperUserOrAdmin)]
    public async Task<ActionResult<long>> CreateNotification(
        [FromBody] CreateNotificationRequest request,
        CancellationToken ct)
    {
        var id = await _notifications.CreateAsync(request, ct);
        return Ok(id);
    }
}
