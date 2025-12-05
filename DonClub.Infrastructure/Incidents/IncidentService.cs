using Donclub.Application.Incidents;
using Donclub.Domain.Incidents;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Donclub.Application.Notifications;
using Donclub.Domain.Notifications;
using System.Text.Json;


namespace Donclub.Infrastructure.Incidents;

public class IncidentService : IIncidentService
{
    private readonly DonclubDbContext _db;
    private readonly INotificationService _notifications;

    public IncidentService(DonclubDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<long> CreateAsync(CreateIncidentRequest request, long createdByUserId, CancellationToken ct = default)
    {
        // اطمینان از وجود Manager
        var managerExists = await _db.Users.AnyAsync(u => u.Id == request.ManagerId, ct);
        if (!managerExists)
            throw new KeyNotFoundException("Manager user not found.");

        // اگر Session داده شده، بررسی کنیم
        if (request.SessionId.HasValue)
        {
            var sessionExists = await _db.Sessions.AnyAsync(s => s.Id == request.SessionId.Value, ct);
            if (!sessionExists)
                throw new KeyNotFoundException("Session not found.");
        }

        var entity = new Incident
        {
            ManagerId = request.ManagerId,
            SessionId = request.SessionId,
            Title = request.Title,
            Description = request.Description,
            Severity = (IncidentSeverity)request.Severity,
            Status = IncidentStatus.Pending,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Incidents.Add(entity);
        await _db.SaveChangesAsync(ct);
        // Notify manager that an incident has been created for them
        await _notifications.CreateAsync(
            new CreateNotificationRequest(
                UserId: entity.ManagerId,
                Title: "اینسیدنت جدید برای شما ثبت شد",
                Message: $"یک اینسیدنت با عنوان «{entity.Title}» برای شما ثبت شد.",
                Type: (byte)NotificationType.IncidentCreated,
                DataJson: JsonSerializer.Serialize(new
                {
                    incidentId = entity.Id,
                    sessionId = entity.SessionId
                })
            ),
            ct);
        return entity.Id;
    }

    public async Task ReviewAsync(long id, long reviewerUserId, ReviewIncidentRequest request, CancellationToken ct = default)
    {
        var incident = await _db.Incidents.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new KeyNotFoundException("Incident not found.");

        var status = (IncidentStatus)request.Status;
        if (status == IncidentStatus.Pending)
            throw new InvalidOperationException("Cannot set status back to Pending.");

        incident.Status = status;
        incident.ReviewNote = request.ReviewNote;
        incident.ReviewedByUserId = reviewerUserId;
        incident.ReviewedAtUtc = DateTime.UtcNow;
        incident.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        var decisionText = incident.Status switch
        {
            IncidentStatus.Approved => "تایید شد",
            IncidentStatus.Rejected => "رد شد",
            _ => "به‌روزرسانی شد"
        };

        var title = "اینسیدنت شما بررسی شد";
        var message = $"اینسیدنت «{incident.Title}» {decisionText}.";

        var payload = JsonSerializer.Serialize(new
        {
            incidentId = incident.Id,
            status = incident.Status,
            reviewedByUserId = reviewerUserId
        });

        // برای خود Manager
        await _notifications.CreateAsync(
            new CreateNotificationRequest(
                UserId: incident.ManagerId,
                Title: title,
                Message: message,
                Type: (byte)NotificationType.IncidentResolved,
                DataJson: payload
            ),
            ct);

        // اگر کسی که Incident را ثبت کرده با Manager فرق دارد، به او هم اطلاع بده
        if (incident.CreatedByUserId != incident.ManagerId)
        {
            await _notifications.CreateAsync(
                new CreateNotificationRequest(
                    UserId: incident.CreatedByUserId,
                    Title: title,
                    Message: message,
                    Type: (byte)NotificationType.IncidentResolved,
                    DataJson: payload
                ),
                ct);
        }

    }

    public async Task<IncidentDetailDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var i = await _db.Incidents
            .Include(x => x.Manager)
            .Include(x => x.Session).ThenInclude(s => s.Game)
            .Include(x => x.CreatedByUser)
            .Include(x => x.ReviewedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (i is null) return null;

        return new IncidentDetailDto(
            i.Id,
            i.ManagerId,
            i.Manager.DisplayName ?? i.Manager.PhoneNumber,
            i.SessionId,
            i.Session?.Game?.Name,
            i.Title,
            i.Description,
            (byte)i.Severity,
            (byte)i.Status,
            i.CreatedByUserId,
            i.CreatedByUser.DisplayName ?? i.CreatedByUser.PhoneNumber,
            i.ReviewedByUserId,
            i.ReviewedByUser?.DisplayName ?? i.ReviewedByUser?.PhoneNumber,
            i.CreatedAtUtc,
            i.ReviewedAtUtc,
            i.ReviewNote
        );
    }

    public async Task<IReadOnlyList<IncidentSummaryDto>> GetByManagerAsync(long managerId, CancellationToken ct = default)
    {
        return await _db.Incidents
            .Where(i => i.ManagerId == managerId)
            .OrderByDescending(i => i.CreatedAtUtc)
            .Select(i => new IncidentSummaryDto(
                i.Id,
                i.ManagerId,
                i.SessionId,
                i.Title,
                (byte)i.Severity,
                (byte)i.Status,
                i.CreatedAtUtc
            ))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<IncidentSummaryDto>> GetPendingAsync(CancellationToken ct = default)
    {
        return await _db.Incidents
            .Where(i => i.Status == IncidentStatus.Pending)
            .OrderByDescending(i => i.CreatedAtUtc)
            .Select(i => new IncidentSummaryDto(
                i.Id,
                i.ManagerId,
                i.SessionId,
                i.Title,
                (byte)i.Severity,
                (byte)i.Status,
                i.CreatedAtUtc
            ))
            .ToListAsync(ct);
    }

    public async Task<ManagerKpiDto?> GetManagerKpiAsync(long managerId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        // Sessions that this manager managed in range
        var sessionsQuery = _db.Sessions
            .Where(s => s.ManagerId == managerId &&
                        s.StartTimeUtc >= fromUtc &&
                        s.StartTimeUtc <= toUtc);

        var totalSessions = await sessionsQuery.CountAsync(ct);
        var completedSessions = await sessionsQuery
            .Where(s => s.Status == Domain.Sessions.SessionStatus.Ended)
            .CountAsync(ct);

        // Incidents in that range
        var incidentsQuery = _db.Incidents
            .Where(i => i.ManagerId == managerId &&
                        i.CreatedAtUtc >= fromUtc &&
                        i.CreatedAtUtc <= toUtc);

        var totalIncidents = await incidentsQuery.CountAsync(ct);
        var approvedIncidents = await incidentsQuery
            .Where(i => i.Status == IncidentStatus.Approved)
            .CountAsync(ct);
        var rejectedIncidents = await incidentsQuery
            .Where(i => i.Status == IncidentStatus.Rejected)
            .CountAsync(ct);

        var manager = await _db.Users.FirstOrDefaultAsync(u => u.Id == managerId, ct);
        if (manager is null)
            return null;

        double incidentRate = 0;
        if (totalSessions > 0)
            incidentRate = (double)approvedIncidents / totalSessions;

        return new ManagerKpiDto(
            managerId,
            manager.DisplayName ?? manager.PhoneNumber,
            totalSessions,
            completedSessions,
            totalIncidents,
            approvedIncidents,
            rejectedIncidents,
            incidentRate
        );
    }
}
