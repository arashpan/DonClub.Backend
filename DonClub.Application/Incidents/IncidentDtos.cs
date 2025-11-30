namespace Donclub.Application.Incidents;

public record IncidentSummaryDto(
    long Id,
    long ManagerId,
    long? SessionId,
    string Title,
    byte Severity,
    byte Status,
    DateTime CreatedAtUtc
);

public record IncidentDetailDto(
    long Id,
    long ManagerId,
    string ManagerName,
    long? SessionId,
    string? GameName,
    string Title,
    string Description,
    byte Severity,
    byte Status,
    long CreatedByUserId,
    string? CreatedByName,
    long? ReviewedByUserId,
    string? ReviewedByName,
    DateTime CreatedAtUtc,
    DateTime? ReviewedAtUtc,
    string? ReviewNote
);

public record CreateIncidentRequest(
    long ManagerId,
    long? SessionId,
    string Title,
    string Description,
    byte Severity
);

public record ReviewIncidentRequest(
    byte Status,
    string? ReviewNote
);

// برای KPI:
public record ManagerKpiDto(
    long ManagerId,
    string? ManagerName,
    int TotalSessions,
    int CompletedSessions,
    int TotalIncidents,
    int ApprovedIncidents,
    int RejectedIncidents,
    double IncidentRate // نسبت Incident به Session
);
