using ClinicManagement_API.Features.receptionist_service.dto;
using ClinicManagement_API.Features.receptionist_service.service;

namespace ClinicManagement_API.Features.receptionist_service.handler;

public static class ReceptionistHandler
{
    // Dashboard
    public static Task<IResult> GetDashboardStats(Guid? clinicId, IReceptionistService svc)
        => svc.GetDashboardStatsAsync(clinicId);

    public static Task<IResult> GetTodayAppointments(Guid? clinicId, int limit, IReceptionistService svc)
        => svc.GetTodayAppointmentsAsync(clinicId, limit);

    // Appointments
    public static Task<IResult> GetAppointments(string date, string? doctor, string? status, string? search, Guid? clinicId, IReceptionistService svc)
        => svc.GetAppointmentsAsync(date, doctor, status, search, clinicId);

    public static Task<IResult> CreateAppointment(CreateAppointmentRequest request, IReceptionistService svc)
        => svc.CreateAppointmentAsync(request);

    public static Task<IResult> UpdateAppointment(Guid id, UpdateAppointmentRequest request, IReceptionistService svc)
        => svc.UpdateAppointmentAsync(id, request);

    public static Task<IResult> ConfirmAppointment(Guid id, IReceptionistService svc)
        => svc.ConfirmAppointmentAsync(id);

    public static Task<IResult> CancelAppointment(Guid id, CancelReasonRequest? request, IReceptionistService svc)
        => svc.CancelAppointmentAsync(id, request?.Reason);

    public static Task<IResult> CheckinAppointment(Guid id, IReceptionistService svc)
        => svc.CheckinAppointmentAsync(id);

    // Queue
    public static Task<IResult> GetQueue(string? date, string? search, Guid? clinicId, IReceptionistService svc)
        => svc.GetQueueAsync(date, search, clinicId);

    public static Task<IResult> CallPatient(Guid id, IReceptionistService svc)
        => svc.CallPatientAsync(id);

    public static Task<IResult> CompleteAppointment(Guid id, IReceptionistService svc)
        => svc.CompleteAppointmentAsync(id);
}
