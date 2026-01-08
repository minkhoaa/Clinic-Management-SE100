using System.Security.Claims;
using ClinicManagement_API.Features.doctor_service.dto;
using ClinicManagement_API.Features.doctor_service.service;

namespace ClinicManagement_API.Features.doctor_service.handler;

public static class DoctorPracticeHandler
{
    public static async Task<IResult> GetDashboardStats(IDoctorPracticeService service, ClaimsPrincipal user)
        => await service.GetDashboardStatsAsync(user);

    public static async Task<IResult> GetQueue(IDoctorPracticeService service, ClaimsPrincipal user, string? date)
        => await service.GetQueueAsync(user, date);

    public static async Task<IResult> StartExam(IDoctorPracticeService service, ClaimsPrincipal user,
        Guid appointmentId)
        => await service.StartExamAsync(user, appointmentId);

    public static async Task<IResult> CompleteExam(IDoctorPracticeService service, ClaimsPrincipal user,
        Guid appointmentId)
        => await service.CompleteExamAsync(user, appointmentId);

    public static async Task<IResult> GetPatients(IDoctorPracticeService service, ClaimsPrincipal user)
        => await service.GetPatientsAsync(user);

    public static async Task<IResult> GetPatientDetail(IDoctorPracticeService service, ClaimsPrincipal user,
        Guid patientId)
        => await service.GetPatientDetailAsync(user, patientId);

    public static async Task<IResult> GetMedicalRecordDetail(IDoctorPracticeService service, Guid recordId)
        => await service.GetMedicalRecordDetailAsync(recordId);

    public static async Task<IResult> CreateMedicalRecord(IDoctorPracticeService service, ClaimsPrincipal user,
        CreateMedicalRecordRequest request)
        => await service.CreateMedicalRecordAsync(user, request);

    public static async Task<IResult> UpdateMedicalRecord(IDoctorPracticeService service, ClaimsPrincipal user,
        Guid recordId, UpdateMedicalRecordRequest request)
        => await service.UpdateMedicalRecordAsync(user, recordId, request);

    public static async Task<IResult> GetPrescriptionTemplates(IDoctorPracticeService service, ClaimsPrincipal user)
        => await service.GetPrescriptionTemplatesAsync(user);

    public static async Task<IResult> CreatePrescriptionTemplate(IDoctorPracticeService service, ClaimsPrincipal user,
        CreatePrescriptionTemplateRequest request)
        => await service.CreatePrescriptionTemplateAsync(user, request);

    public static async Task<IResult> CreateExamination(IDoctorPracticeService service, ClaimsPrincipal user,
        CreateExaminationRequest request)
        => await service.CreateExaminationAsync(user, request);
}
