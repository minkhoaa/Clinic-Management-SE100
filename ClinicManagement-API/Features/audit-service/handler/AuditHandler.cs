using System.Security.Claims;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.audit_service.dto;
using ClinicManagement_API.Features.audit_service.service;

namespace ClinicManagement_API.Features.audit_service.handler;

public static class AuditHandler
{
    public static async Task<IResult> GetAuditLogs(
        IAuditService service, 
        ClaimsPrincipal user,
        AuditEntityType? entityType,
        Guid? entityId,
        Guid? userId,
        DateTime? fromDate,
        DateTime? toDate,
        int page = 1,
        int pageSize = 20)
    {
        var request = new GetAuditLogsRequest(entityType, entityId, userId, fromDate, toDate, page, pageSize);
        return await service.GetAuditLogsAsync(user, request);
    }

    public static async Task<IResult> GetAuditLog(IAuditService service, ClaimsPrincipal user, Guid auditLogId)
        => await service.GetAuditLogAsync(user, auditLogId);

    public static async Task<IResult> GetMedicalRecordHistory(IAuditService service, ClaimsPrincipal user, Guid medicalRecordId)
        => await service.GetEntityHistoryAsync(user, AuditEntityType.MedicalRecord, medicalRecordId);
}
