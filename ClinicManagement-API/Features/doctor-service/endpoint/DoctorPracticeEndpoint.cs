using ClinicManagement_API.Features.doctor_service.handler;

namespace ClinicManagement_API.Features.doctor_service.endpoint;

public static class DoctorPracticeEndpoint
{
    public static void MapDoctorPracticeEndpoint(this IEndpointRouteBuilder route)
    {
        var doctor = route.MapGroup("/api/doctor")
            .WithTags("Doctor Practice")
            .RequireAuthorization("DoctorOnly");

        // Dashboard
        doctor.MapGet("/dashboard/stats", DoctorPracticeHandler.GetDashboardStats);

        // Queue
        doctor.MapGet("/queue", DoctorPracticeHandler.GetQueue);
        doctor.MapPut("/queue/{appointmentId:guid}/start", DoctorPracticeHandler.StartExam);
        doctor.MapPut("/queue/{appointmentId:guid}/complete", DoctorPracticeHandler.CompleteExam);

        // Patients
        doctor.MapGet("/patients", DoctorPracticeHandler.GetPatients);
        doctor.MapGet("/patients/{patientId:guid}", DoctorPracticeHandler.GetPatientDetail);

        // Medical Records
        doctor.MapGet("/medical-records/{recordId:guid}", DoctorPracticeHandler.GetMedicalRecordDetail);
        doctor.MapPost("/medical-records", DoctorPracticeHandler.CreateMedicalRecord);
        doctor.MapPut("/medical-records/{recordId:guid}", DoctorPracticeHandler.UpdateMedicalRecord);

        // Prescription Templates
        doctor.MapGet("/prescription-templates", DoctorPracticeHandler.GetPrescriptionTemplates);
        doctor.MapPost("/prescription-templates", DoctorPracticeHandler.CreatePrescriptionTemplate);

        // Examinations
        doctor.MapPost("/examinations", DoctorPracticeHandler.CreateExamination);

        // Attachments (X-Ray, images, documents)
        doctor.MapGet("/medical-records/{recordId:guid}/attachments", DoctorPracticeHandler.GetAttachments)
            .WithDescription("Lấy danh sách file đính kèm của hồ sơ bệnh án");

        doctor.MapPost("/medical-records/{recordId:guid}/attachments", DoctorPracticeHandler.UploadAttachment)
            .WithDescription("Upload ảnh X-Ray hoặc file đính kèm (max 20MB, types: jpg, png, pdf, dicom)")
            .DisableAntiforgery();

        doctor.MapDelete("/medical-records/{recordId:guid}/attachments/{attachmentId:guid}",
                DoctorPracticeHandler.DeleteAttachment)
            .WithDescription("Xóa file đính kèm");
    }
}
