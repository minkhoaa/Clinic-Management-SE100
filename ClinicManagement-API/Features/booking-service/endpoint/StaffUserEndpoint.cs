using ClinicManagement_API.Contracts;
using ClinicManagement_API.Features.booking_service.dto;
using ClinicManagement_API.Features.booking_service.handler;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.booking_service.endpoint;

public static class StaffUserEndpoint
{
    public static void MapStaffUserEndpoint(this IEndpointRouteBuilder route)
    {
        var app = route.MapGroup("/api/admin/staff-user").WithTags("StaffUsers Management");
        app.MapPost("/", AdminHandler.CreateStaffUser);
        app.MapPut("/{userId:guid}", AdminHandler.UpdateStaffUser);
        app.MapDelete("/{userId:guid}", AdminHandler.DeleteStaffUser);
        app.MapGet("/", async (ClinicDbContext context) => Results.Ok(new ApiResponse<List<StaffUserDto>>(
            true, "Staff users retrieved successfully",
            await context.StaffUsers.AsNoTracking().Select(
                s => new StaffUserDto(s.UserId, s.ClinicId, s.Username, s.FullName, s.Role, s.IsActive, s.Clinic)).ToListAsync())));
    }
}