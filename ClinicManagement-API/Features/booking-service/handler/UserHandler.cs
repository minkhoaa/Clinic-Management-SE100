using ClinicManagement_API.Features.booking_service.dto;
using ClinicManagement_API.Features.booking_service.service;
using Microsoft.AspNetCore.Http;

namespace ClinicManagement_API.Features.booking_service.handler;

public static class UserHandler
{
    public static Task<IResult> GetClinics(IUserService svc, string? nameOrCode)
        => svc.GetClinicsAsync(nameOrCode);

    public static Task<IResult> GetServices(IUserService svc, Guid? clinicId, string? nameOrCode, bool? isActive)
        => svc.GetServicesAsync(clinicId, nameOrCode, isActive);

    public static Task<IResult> GetDoctors(IUserService svc, Guid? clinicId, string? nameOrCode, string? specialty, Guid? serviceId, bool? isActive)
        => svc.GetDoctorsAsync(clinicId, nameOrCode, specialty, serviceId, isActive);

    public static Task<IResult> GetAvailability(IUserService svc, Guid doctorId, DateOnly from, DateOnly to)
        => svc.GetAvailabilityAsync(doctorId, from, to);


    public static Task<IResult> GetSlots(IUserService svc, Guid clinicId, Guid doctorId, Guid? serviceId, DateOnly date)
        => svc.GetSlotsAsync(clinicId, doctorId, serviceId, date);

    public static Task<IResult> CreateBooking(IUserService svc, CreateBookingRequest req)
        => svc.CreateBookingAsync(req);

    public static Task<IResult> GetBooking(IUserService svc, Guid bookingId)
        => svc.GetBookingAsync(bookingId);

    public static Task<IResult> ConfirmBooking(IUserService svc, Guid bookingId)
        => svc.ConfirmBookingAsync(bookingId);
    public static Task<IResult> CancelBooking(IUserService svc, string token)
        => svc.CancelAppointmentAsync(token);
    public static Task<IResult> Rescheduling(IUserService svc, string token, DateTime start, DateTime end)
        => svc.ReschedulingAppointmentAsync(token, start, end);
    

}
