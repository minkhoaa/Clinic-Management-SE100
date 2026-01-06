using System.Xml;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.booking_service.dto;

namespace ClinicManagement_API.Features.booking_service.endpoint
{
    public static class EnumEndpoint
    {
        public static void MapEnumEndpoint(this IEndpointRouteBuilder route)
        {
            var app = route.MapGroup("/api/enums").WithTags("Enums");

            app.MapGet("/genders", () =>
            {
                return Enum.GetValues(typeof(Gender))
                .Cast<Gender>()
                .Select(e => new EnumDto((int)e, e.ToString()))
                .ToList();

            });
            app.MapGet("/booking-statuses", () => Enum.GetValues(typeof(BookingStatus))
                .Cast<BookingStatus>()
                .Select(e => new EnumDto((int)e, e.ToString()))
                .ToList());
            app.MapGet("/appointment-statuses", () => Enum.GetValues(typeof(AppointmentStatus))
                .Cast<AppointmentStatus>()
                .Select(e => new EnumDto((int)e, e.ToString()))
                .ToList());
            app.MapGet("/appointment-sources", () => Enum.GetValues(typeof(AppointmentSource))
                .Cast<AppointmentSource>()
                .Select(e => new EnumDto((int)e, e.ToString()))
                .ToList());
            app.MapGet("/staff-roles", () => Enum.GetValues(typeof(StaffRole))
                .Cast<StaffRole>()
                .Select(e => new EnumDto((int)e, e.ToString()))
                .ToList());
        }


    }
    public record EnumDto(int Value, string Name);
}
