using ClinicManagement_API.Features.receptionist_service.handler;

namespace ClinicManagement_API.Features.receptionist_service.endpoint;

public static class ReceptionistEndpoint
{
    public static void MapReceptionistEndpoint(this IEndpointRouteBuilder route)
    {
        var receptionist = route.MapGroup("/api/receptionist").WithTags("Receptionist Dashboard");

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
    }
}
