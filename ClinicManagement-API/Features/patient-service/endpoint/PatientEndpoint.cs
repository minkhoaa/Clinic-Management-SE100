using ClinicManagement_API.Features.admin_service.handler;
using ClinicManagement_API.Features.patient_service.handler;

namespace ClinicManagement_API.Features.patient_service.endpoint;

public static class PatientEndpoint
{
    public static void MapPatientEndpoint(this IEndpointRouteBuilder route)
    {
        // Admin endpoints - CRUD patients (requires Admin role)
        var patientAdmin = route.MapGroup("/api/patient")
            .WithTags("Patient Management")
            .RequireAuthorization("AdminOnly");
        patientAdmin.MapPost("/", PatientHandler.CreatePatient);
        patientAdmin.MapPut("/{patientId:guid}", PatientHandler.UpdatePatient);
        patientAdmin.MapDelete("/{patientId:guid}", PatientHandler.DeletePatient);
        patientAdmin.MapGet("/", PatientHandler.GetAllPatients);

        // Patient profile endpoints (requires Patient role)
        var patientProfile = route.MapGroup("/api/patient")
            .WithTags("Patient Profile")
            .RequireAuthorization("PatientOnly");
        patientProfile.MapGet("/profile", PatientHandler.GetProfile);
        patientProfile.MapPut("/profile", PatientHandler.UpdateProfile);

        // Appointments endpoint (requires Patient role)
        var appointments = route.MapGroup("/api/patient/appointments")
            .WithTags("Patient Appointments")
            .RequireAuthorization("PatientOnly");
        appointments.MapGet("/", PatientHandler.GetAppointments);
        appointments.MapGet("/{id:guid}", PatientHandler.GetAppointmentDetail);
        appointments.MapPut("/{id:guid}/cancel", PatientHandler.CancelAppointment);

        // Medical Records endpoints (requires Patient role)
        var medicalRecords = route.MapGroup("/api/patient/medical-records")
            .WithTags("Patient Medical Records")
            .RequireAuthorization("PatientOnly");
        medicalRecords.MapGet("/", PatientHandler.GetMedicalRecords);
        medicalRecords.MapGet("/{id:guid}", PatientHandler.GetMedicalRecordDetail);
        medicalRecords.MapGet("/{id:guid}/attachments/{attachmentId:guid}", PatientHandler.DownloadAttachment);

        // Reviews endpoint (requires Patient role)
        var reviews = route.MapGroup("/api/patient/reviews")
            .WithTags("Patient Reviews")
            .RequireAuthorization("PatientOnly");
        reviews.MapPost("/", ReviewHandler.CreateReview);
    }
}
