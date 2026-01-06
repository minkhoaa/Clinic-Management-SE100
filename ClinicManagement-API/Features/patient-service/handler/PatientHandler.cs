using System.Security.Claims;
using ClinicManagement_API.Features.patient_service.dto;
using ClinicManagement_API.Features.patient_service.service;


namespace ClinicManagement_API.Features.patient_service.handler;

public static class PatientHandler
{
    // Profile endpoints
    public static Task<IResult> GetProfile(ClaimsPrincipal user, IPatientService svc)
        => svc.GetProfileAsync(user);

    public static Task<IResult> UpdateProfile(ClaimsPrincipal user, UpdatePatientProfileRequest request, IPatientService svc)
        => svc.UpdateProfileAsync(user, request);

    // Appointments
    public static Task<IResult> GetAppointments(ClaimsPrincipal user, Guid? patientId, string? phone, IPatientService svc)
        => svc.GetAppointmentsAsync(user, patientId, phone);

    // Admin endpoints
    public static Task<IResult> GetAllPatients(IPatientService svc)
        => svc.GetAllPatientsAsync();

    public static Task<IResult> CreatePatient(CreatePatientDto request, IPatientService svc)
        => svc.CreatePatientAsync(request);

    public static Task<IResult> UpdatePatient(Guid patientId, CreatePatientDto request, IPatientService svc)
        => svc.UpdatePatientAsync(patientId, request);

    public static Task<IResult> DeletePatient(Guid patientId, IPatientService svc)
        => svc.DeletePatientAsync(patientId);

    // Appointment detail
    public static Task<IResult> GetAppointmentDetail(Guid id, IPatientService svc)
        => svc.GetAppointmentDetailAsync(id);

    // Cancel appointment
    public static Task<IResult> CancelAppointment(Guid id, CancelAppointmentRequest request, IPatientService svc)
        => svc.CancelPatientAppointmentAsync(id, request);
}