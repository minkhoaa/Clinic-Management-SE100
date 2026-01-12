using System.Security.Claims;
using ClinicManagement_API.Features.tooth_record_service.dto;
using ClinicManagement_API.Features.tooth_record_service.service;

namespace ClinicManagement_API.Features.tooth_record_service.handler;

public static class ToothRecordHandler
{
    public static async Task<IResult> GetDentalChart(IToothRecordService service, ClaimsPrincipal user, Guid patientId)
        => await service.GetDentalChartAsync(user, patientId);

    public static async Task<IResult> UpdateToothRecord(IToothRecordService service, ClaimsPrincipal user, 
        Guid patientId, UpdateToothRecordRequest request)
        => await service.UpdateToothRecordAsync(user, patientId, request);

    public static async Task<IResult> BatchUpdateToothRecords(IToothRecordService service, ClaimsPrincipal user,
        BatchUpdateToothRecordsRequest request)
        => await service.BatchUpdateToothRecordsAsync(user, request);

    public static async Task<IResult> GetToothHistory(IToothRecordService service, ClaimsPrincipal user, 
        Guid patientId, int toothNumber)
        => await service.GetToothHistoryAsync(user, patientId, toothNumber);
}
