using ClinicManagement_API.Features.booking_service.handler;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace ClinicManagement_API.Features.booking_service.endpoint;

public static class BookingEndpoint
{
    public static void MapBookingClinicEndpoint(this IEndpointRouteBuilder route)
    {
        var app = route.MapGroup("/api/clinic").WithTags("Booking Clinics");
        app.MapGet("/", UserHandler.GetClinics);
    }

    public static void MapBookingServiceEndpoint(this IEndpointRouteBuilder route)
    {
        var app = route.MapGroup("/api/services").WithTags("Booking Services");
        app.MapGet("/", UserHandler.GetServices);
    }

    public static void MapBookingDoctorEndpoint(this IEndpointRouteBuilder route)
    {
        var app = route.MapGroup("/api/doctors").WithTags("Booking Doctors");
        app.MapGet("/", UserHandler.GetDoctors);
        app.MapGet("/{doctorId:guid}/availability", UserHandler.GetAvailability);
        app.MapPost("/availability", AdminHandler.CreateAvailability);
        app.MapPut("/availability/{availId:guid}", AdminHandler.UpdateAvailability);
    }

    public static void MapBookingSlotEndpoint(this IEndpointRouteBuilder route)
    {
        var app = route.MapGroup("/api/slots").WithTags("Booking Slots");
        app.MapGet("/", UserHandler.GetSlots);
    }

    public static void MapBookingEndpoint(this IEndpointRouteBuilder route)
    {
        var app = route.MapGroup("/api/appointments").WithTags("Appointments");
        app.MapPost("/", UserHandler.CreateBooking);
        app.MapGet("/{bookingId:guid}", UserHandler.GetBooking);
        app.MapPost("/{bookingId:guid}/confirm", UserHandler.ConfirmBooking);
        app.MapPost("/{bookingId:guid}/cancel", UserHandler.CancelBooking);
        app.MapPost("/{bookingId:guid}/reschedule", UserHandler.Rescheduling);
        
    }
}
