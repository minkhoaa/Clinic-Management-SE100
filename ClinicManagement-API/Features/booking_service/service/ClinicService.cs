
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Features.booking_service.dto;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.booking_service.service
{

    public interface IClinicService
    {
        Task<IResult> GetAllClinicAsync();
        Task<IResult> CreateClinicAsync(CreateClinicRequest request);
        Task<IResult> UpdateClinicAsync(Guid clinicId, UpdateClinicRequest request);
        Task<IResult> DeleteClinicAsync(Guid clinicId);
    }

    public class ClinicService : IClinicService
    {
        private readonly ClinicDbContext _context;

        public ClinicService(ClinicDbContext context)
        {
            _context = context;
        }

        public async Task<IResult> CreateClinicAsync(CreateClinicRequest request)
        {
            var existingClinic = await _context.Clinics.AsNoTracking().AnyAsync(x => x.Code == request.Code);
            if (existingClinic)
            {
                return Results.Conflict(new ApiResponse<object>
                    (false, $"Clinic with code '{request.Code}' already exists.", null));
            }
            var clinic = new Clinic
            {
                Code = request.Code,
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Clinics.Add(clinic);
            await _context.SaveChangesAsync();

            var clinicDto = new ClinicDto(clinic.ClinicId, clinic.Code, clinic.Name, clinic.TimeZone, clinic.Phone, clinic.Email);
            return Results.Created($"/clinics", new ApiResponse<ClinicDto>(true, "Clinic created successfully", clinicDto));
        }

        public async Task<IResult> UpdateClinicAsync(Guid clinicId, UpdateClinicRequest request)
        {
            var affectedRows = await _context.Clinics.AsNoTracking().Where(x => x.ClinicId == clinicId).ExecuteUpdateAsync(x => x.SetProperty(a =>
                    a.Name, request.Name)
                .SetProperty(a => a.Phone, request.Phone)
                .SetProperty(a => a.Email, request.Email)
                .SetProperty(a => a.UpdatedAt, DateTime.UtcNow)
            );
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, $"Clinic updated successfully {affectedRows}", null))
                : Results.NoContent();
        }

        public async Task<IResult> GetAllClinicAsync()
        {
            var clinics = await _context.Clinics.AsNoTracking()
                .Select(clinic => new ClinicDto(clinic.ClinicId, clinic.Code, clinic.Name, clinic.TimeZone, clinic.Phone, clinic.Email))
                .ToListAsync();
            return Results.Ok(new ApiResponse<List<ClinicDto>>(true, "Clinics retrieved successfully", clinics));
        }
        public async Task<IResult> DeleteClinicAsync(Guid clinicId)
        {
            var affectedRows = await _context.Clinics.AsNoTracking().Where(x => x.ClinicId == clinicId)
                .ExecuteDeleteAsync();
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, $"Deleted {affectedRows} row(s)", null))
                : Results.NoContent();
        }
    }
}
