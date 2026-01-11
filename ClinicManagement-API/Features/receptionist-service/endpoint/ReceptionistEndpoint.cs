using ClinicManagement_API.Features.booking_service.handler;
using ClinicManagement_API.Features.patient_service.handler;
using ClinicManagement_API.Features.receptionist_service.handler;

namespace ClinicManagement_API.Features.receptionist_service.endpoint;

public static class ReceptionistEndpoint
{
    public static void MapReceptionistEndpoint(this IEndpointRouteBuilder route)
    {
        var receptionist = route.MapGroup("/api/receptionist")
            .WithTags("Receptionist Dashboard")
            .RequireAuthorization("ReceptionistOrAdmin");

        // Dashboard
        receptionist.MapGet("/dashboard/stats", ReceptionistHandler.GetDashboardStats);
        receptionist.MapGet("/appointments/today", ReceptionistHandler.GetTodayAppointments);

        // Appointments CRUD
        receptionist.MapGet("/appointments", ReceptionistHandler.GetAppointments);
        receptionist.MapPost("/appointments", ReceptionistHandler.CreateAppointment);
        receptionist.MapPut("/appointments/{id:guid}", ReceptionistHandler.UpdateAppointment);

        // Appointment Actions
        receptionist.MapPut("/appointments/{id:guid}/confirm", ReceptionistHandler.ConfirmAppointment);
        receptionist.MapPut("/appointments/{id:guid}/cancel", ReceptionistHandler.CancelAppointment);
        receptionist.MapPut("/appointments/{id:guid}/checkin", ReceptionistHandler.CheckinAppointment);

        // Patients (GET uses PatientHandler, POST/PUT reuse AdminHandler)
        receptionist.MapGet("/patients", PatientHandler.GetPatientsForReceptionist);
        receptionist.MapGet("/patients/{id:guid}", PatientHandler.GetPatientDetailForReceptionist);
        receptionist.MapPost("/patients", AdminHandler.CreatePatient);
        receptionist.MapPut("/patients/{patientId:guid}", AdminHandler.UpdatePatient);

        // Queue
        receptionist.MapGet("/queue", ReceptionistHandler.GetQueue);
        receptionist.MapPut("/queue/{id:guid}/checkin", ReceptionistHandler.CheckinAppointment);
        receptionist.MapPut("/queue/{id:guid}/call", ReceptionistHandler.CallPatient);
        receptionist.MapPut("/queue/{id:guid}/complete", ReceptionistHandler.CompleteAppointment);
    }
}
