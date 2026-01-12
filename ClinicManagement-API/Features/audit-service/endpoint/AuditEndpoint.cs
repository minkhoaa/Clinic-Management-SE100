using ClinicManagement_API.Features.audit_service.handler;

namespace ClinicManagement_API.Features.audit_service.endpoint;

public static class AuditEndpoint
{
    public static void MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var audit = app.MapGroup("/api/audit")
            .WithTags("Audit Trail")
            .RequireAuthorization("AdminOnly"); // Only admin can view audit logs

        // Get paginated audit logs with filters
        audit.MapGet("", AuditHandler.GetAuditLogs)
            .WithName("GetAuditLogs")
            .WithDescription("Lấy danh sách audit logs (phân trang)");

        // Get specific audit log detail
        audit.MapGet("{auditLogId:guid}", AuditHandler.GetAuditLog)
            .WithName("GetAuditLog")
            .WithDescription("Xem chi tiết một audit log");

        // Get history of a specific medical record
        audit.MapGet("medical-records/{medicalRecordId:guid}/history", AuditHandler.GetMedicalRecordHistory)
            .WithName("GetMedicalRecordHistory")
            .WithDescription("Xem lịch sử thay đổi của hồ sơ bệnh án");
    }
}
