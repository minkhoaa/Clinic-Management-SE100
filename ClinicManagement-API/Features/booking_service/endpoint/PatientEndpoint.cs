using ClinicManagement_API.Features.booking_service.dto;
using ClinicManagement_API.Features.booking_service.handler;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.booking_service.endpoint
{
    public static class PatientEndpoint
    {
        public static void MapPatientEndpoint(this IEndpointRouteBuilder route)
        {
            var app = route.MapGroup("/api/patient").WithTags("Patient Management");
            
            app.MapPost("/", AdminHandler.CreatePatient);
            app.MapPut("/{patientId:guid}", AdminHandler.UpdatePatient);
            app.MapDelete("/{patientId:guid}", AdminHandler.DeletePatient);
             app.MapGet("/", async (ClinicDbContext context) => Results.Ok(new ApiResponse<List<PatientDto>>(
            true, "Patients retrieved successfully",
            await context.Patients.AsNoTracking().Select(
                s => new PatientDto(s.PatientId, s.ClinicId, s.PatientCode, s.FullName, s.Gender, s.Dob,s.PrimaryPhone,s.Email, s.Note, s.AddressLine1, s.Clinic)).ToListAsync())));
        }
    }
}