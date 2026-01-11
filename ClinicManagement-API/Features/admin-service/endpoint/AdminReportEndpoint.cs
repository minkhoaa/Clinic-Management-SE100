using ClinicManagement_API.Features.admin_service.handler;

namespace ClinicManagement_API.Features.admin_service.endpoint;

public static class AdminReportEndpoint
{
    public static void MapAdminReportEndpoint(this IEndpointRouteBuilder route)
    {
        var admin = route.MapGroup("/api/admin")
            .WithTags("Admin Dashboard & Reports")
            .RequireAuthorization("AdminOnly");

        // Dashboard
        admin.MapGet("/dashboard/stats", AdminReportHandler.GetDashboardStats);

        // Reports
        admin.MapGet("/report/patients", AdminReportHandler.GetPatientReport);
        admin.MapGet("/report/historical-stats", AdminReportHandler.GetHistoricalStats);
        admin.MapGet("/report/appointment", AdminReportHandler.GetAppointmentReport);

        // Review Stats
        admin.MapGet("/reviews/stats", AdminReportHandler.GetReviewStats);
    }
}
