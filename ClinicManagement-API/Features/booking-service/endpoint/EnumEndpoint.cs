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
            app.MapGet("/appointment-statuses", () => Enum.GetValues(typeof(AppointmentStatus))
                .Cast<AppointmentStatus>()
                .Select(e => new EnumDto((int)e, e.ToString()))
                .ToList());
            app.MapGet("/appointment-sources", () => Enum.GetValues(typeof(AppointmentSource))
                .Cast<AppointmentSource>()
                .Select(e => new EnumDto((int)e, e.ToString()))
                .ToList());
            app.MapGet("/staff-roles", () => new[]
            {
                new { Value = AppRoles.Admin, Name = "Admin" },
                new { Value = AppRoles.Receptionist, Name = "Receptionist" },
                new { Value = AppRoles.Doctor, Name = "Doctor" },
                new { Value = AppRoles.Patient, Name = "Patient" }
            });
        }
    }

    public record EnumDto(int Value, string Name);
}
