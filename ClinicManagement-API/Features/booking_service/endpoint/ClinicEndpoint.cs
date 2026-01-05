using ClinicManagement_API.Features.booking_service.dto;
using ClinicManagement_API.Features.booking_service.handler;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.booking_service.endpoint;

public static class ClinicEndpoint
{
    public static void MapClinicEndpoint(this IEndpointRouteBuilder route)
    {
        var app = route.MapGroup("/api/admin/clinic").WithTags("Clinics Management");
        app.MapPost("/", AdminHandler.CreateClinic);
        app.MapPut("/{clinicId:guid}", AdminHandler.UpdateClinic);
        app.MapDelete("/{clinicId:guid}", AdminHandler.DeleteClinic);
        app.MapGet("/", async (ClinicDbContext context) => Results.Ok(new ApiResponse<List<ClinicDto>>(
            true, "Clinics retrieved successfully",
            await context.Clinics.AsNoTracking().Select(
                k => new ClinicDto(k.ClinicId, k.Code, k.Name, k.TimeZone, k.Phone, k.Email))
                .ToListAsync()
        )));
    }
}
