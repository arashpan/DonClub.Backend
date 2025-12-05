namespace Donclub.Application.Notifications;

public record NotificationDto(
    long Id,
    string Title,
    string Message,
    byte Type,
    bool IsRead,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc,
    string? DataJson
);

public record CreateNotificationRequest(
    long UserId,
    string Title,
    string Message,
    byte Type,
    string? DataJson
);
