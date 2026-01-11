using System.Security.Claims;
using ClinicManagement_API.Features.admin_service.dto;
using ClinicManagement_API.Features.admin_service.service;

namespace ClinicManagement_API.Features.admin_service.handler;

public static class AdminReportHandler
{
    public static async Task<IResult> GetDashboardStats(IAdminReportService service)
    {
        return await service.GetDashboardStatsAsync();
    }

    public static async Task<IResult> GetPatientReport(
        IAdminReportService service,
        int? month = null,
        int? year = null)
    {
        return await service.GetPatientReportAsync(month, year);
    }

    public static async Task<IResult> GetHistoricalStats(
        IAdminReportService service,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        return await service.GetHistoricalStatsAsync(startDate, endDate);
    }

    public static async Task<IResult> GetAppointmentReport(
        IAdminReportService service,
        int month,
        int year)
    {
        return await service.GetAppointmentReportAsync(month, year);
    }

    public static async Task<IResult> GetReviewStats(IReviewService service)
    {
        return await service.GetReviewStatsAsync();
    }
}

public static class ReviewHandler
{
    public static async Task<IResult> CreateReview(
        IReviewService service,
        ClaimsPrincipal user,
        CreateReviewRequest request)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        return await service.CreateReviewAsync(userId, request);
    }
}
