using ClinicManagement_API.Domains.Enums;

namespace ClinicManagement_API.Features.audit_service.dto;

// Audit log list item
public record AuditLogDto(
    Guid AuditLogId,
    AuditEntityType EntityType,
    string EntityTypeName,
    Guid EntityId,
    AuditAction Action,
    string ActionName,
    string? UserName,
    string? UserRole,
    string? ChangesSummary,
    DateTime CreatedAt
);

// Detailed audit log with old/new values
public record AuditLogDetailDto(
    Guid AuditLogId,
    AuditEntityType EntityType,
    string EntityTypeName,
    Guid EntityId,
    AuditAction Action,
    string ActionName,
    Guid? UserId,
    string? UserName,
    string? UserRole,
    string? OldValues,
    string? NewValues,
    string? ChangesSummary,
    string? IpAddress,
    DateTime CreatedAt
);

// Request to get audit logs
public record GetAuditLogsRequest(
    AuditEntityType? EntityType,
    Guid? EntityId,
    Guid? UserId,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20
);

// Paginated response
public record AuditLogPagedResponse(
    List<AuditLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
