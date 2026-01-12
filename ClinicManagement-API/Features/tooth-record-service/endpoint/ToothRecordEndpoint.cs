using ClinicManagement_API.Features.tooth_record_service.handler;

namespace ClinicManagement_API.Features.tooth_record_service.endpoint;

public static class ToothRecordEndpoint
{
    public static void MapToothRecordEndpoints(this IEndpointRouteBuilder app)
    {
        var teeth = app.MapGroup("/api/patients/{patientId:guid}/dental-chart")
            .WithTags("Dental Chart")
            .RequireAuthorization("StaffOnly");

        // Get full dental chart for a patient
        teeth.MapGet("", ToothRecordHandler.GetDentalChart)
            .WithName("GetDentalChart")
            .WithDescription("Lấy sơ đồ răng đầy đủ của bệnh nhân");

        // Update single tooth record
        teeth.MapPut("teeth", ToothRecordHandler.UpdateToothRecord)
            .WithName("UpdateToothRecord")
            .WithDescription("Cập nhật trạng thái một răng");

        // Get specific tooth history
        teeth.MapGet("teeth/{toothNumber:int}", ToothRecordHandler.GetToothHistory)
            .WithName("GetToothHistory")
            .WithDescription("Xem chi tiết lịch sử điều trị một răng");

        // Batch update - separate endpoint for doctor workflow
        var batch = app.MapGroup("/api/dental-chart")
            .WithTags("Dental Chart")
            .RequireAuthorization("StaffOnly");

        batch.MapPost("batch-update", ToothRecordHandler.BatchUpdateToothRecords)
            .WithName("BatchUpdateToothRecords")
            .WithDescription("Cập nhật nhiều răng cùng lúc (dùng sau khi khám)");
    }
}
