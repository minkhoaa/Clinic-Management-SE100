using ClinicManagement_API.Features.patient_service.handler;

namespace ClinicManagement_API.Features.patient_service.endpoint;

public static class PatientEndpoint
{
    public static void MapPatientEndpoint(this IEndpointRouteBuilder route)
    {
        var patient = route.MapGroup("/api/patient").WithTags("Patient Management");

        // Admin endpoints
        patient.MapPost("/", PatientHandler.CreatePatient);
        patient.MapPut("/{patientId:guid}", PatientHandler.UpdatePatient);
        patient.MapDelete("/{patientId:guid}", PatientHandler.DeletePatient);
        patient.MapGet("/", PatientHandler.GetAllPatients);

        patient.MapGet("/profile", PatientHandler.GetProfile).RequireAuthorization();
        patient.MapPut("/profile", PatientHandler.UpdateProfile).RequireAuthorization();

        // Appointments endpoint
        var appointments = route.MapGroup("/api/patient/appointments").WithTags("Patient Appointments");
        appointments.MapGet("/", PatientHandler.GetAppointments);
        appointments.MapGet("/{id:guid}", PatientHandler.GetAppointmentDetail);
        appointments.MapPut("/{id:guid}/cancel", PatientHandler.CancelAppointment);

        // Medical Records endpoints
        var medicalRecords = route.MapGroup("/api/patient/medical-records").WithTags("Patient Medical Records");
        medicalRecords.MapGet("/", PatientHandler.GetMedicalRecords).RequireAuthorization();
        medicalRecords.MapGet("/{id:guid}", PatientHandler.GetMedicalRecordDetail).RequireAuthorization();
        medicalRecords.MapGet("/{id:guid}/attachments/{attachmentId:guid}", PatientHandler.DownloadAttachment).RequireAuthorization();
    }
}
