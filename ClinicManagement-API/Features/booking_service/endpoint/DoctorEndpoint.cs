using ClinicManagement_API.Features.booking_service.dto;
using ClinicManagement_API.Features.booking_service.handler;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.booking_service.endpoint;

public static class DoctorEndpoint
{
    public static void MapDoctorEndpoint(this IEndpointRouteBuilder route)
    {
        var app = route.MapGroup("/api/admin/doctor").WithTags("Doctors Management");
        app.MapPost("/", AdminHandler.CreateDoctor);
        app.MapPut("/{doctorId:guid}", AdminHandler.UpdateDoctor);
        app.MapDelete("/{doctorId:guid}", AdminHandler.DeleteDoctor);
        app.MapGet("/", AdminHandler.GetAllDoctors);
        
        app.MapPost("/time-off", AdminHandler.AddDoctorTimeOff);
        app.MapPut("/{timeOffId:guid}/time-offs", AdminHandler.UpdateDoctorTimeOff);
        app.MapGet("/time-offs/{doctorId:guid}", async (Guid doctorId, ClinicDbContext context) =>
        {
            var timeOffs = await context.DoctorTimeOffs
                .AsNoTracking()
                .Where(t => t.DoctorId == doctorId)
                .Select(t => new DoctorTimeOffDto(t.TimeOffId, t.ClinicId, t.DoctorId, t.StartAt, t.EndAt, t.Reason, t.Clinic, t.Doctor))
                .ToListAsync();
            return Results.Ok(new ApiResponse<List<DoctorTimeOffDto>>(true, "Doctor time-offs retrieved successfully", timeOffs));
        });
        app.MapDelete("/time-off/{timeOffId:guid}", AdminHandler.DeleteDoctorTimeOff);
    }
    
}
