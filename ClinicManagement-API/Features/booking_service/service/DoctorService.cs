using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Features.booking_service.dto;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.booking_service.service
{
    public interface IDoctorService
    {
        Task<IResult> CreateDoctorAsync(CreateDoctorRequest request);

        Task<IResult> GetAllDoctorAsync();

        Task<IResult> UpdateDoctorAsync(Guid doctorId, UpdateDoctorRequest request);

        Task<IResult> DeleteDoctorAsync(Guid doctorId);
        Task<IResult> AddDoctorTimeOffAsync(AddDoctorTimeOffRequest request);

        Task<IResult> UpdateDoctorTimeOffAsync(Guid timeOffId, AddDoctorTimeOffRequest request);
        Task<IResult> DeleteDoctorTimeOffAsync(Guid timeOffId);

    }
    public class DoctorService : IDoctorService
    {
        private readonly ClinicDbContext _context;

        public DoctorService(ClinicDbContext context)
        {
            _context = context;
        }
        public async Task<IResult> AddDoctorTimeOffAsync(AddDoctorTimeOffRequest request)
        {
            var doctor = await _context.Doctors.FindAsync(request.DoctorId);
            if (doctor == null)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Doctor not found.", null));
            }

            var clinic = await _context.Clinics.FindAsync(request.ClinicId);
            if (clinic == null)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Clinic not found.", null));
            }
            var timeOff = new DoctorTimeOff
            {
                ClinicId = request.ClinicId,
                DoctorId = request.DoctorId,
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                Reason = request.Reason
            };
            _context.DoctorTimeOffs.Add(timeOff);
            await _context.SaveChangesAsync();

            var result = new DoctorTimeOffDto(timeOff.TimeOffId,timeOff.ClinicId, timeOff.DoctorId, timeOff.StartAt, timeOff.EndAt, timeOff.Reason, clinic, doctor);

            return Results.Ok(new ApiResponse<DoctorTimeOffDto>(true, "Doctor time off added successfully", result));
        }

        public async Task<IResult> UpdateDoctorTimeOffAsync(Guid timeOffId, AddDoctorTimeOffRequest request)
        {
            var affectedRows = await _context.DoctorTimeOffs.Where(x => x.TimeOffId == timeOffId).ExecuteUpdateAsync(x => x
                .SetProperty(a => a.ClinicId, request.ClinicId)
                .SetProperty(a => a.DoctorId, request.DoctorId)
                .SetProperty(a => a.StartAt, request.StartAt)
                .SetProperty(a => a.EndAt, request.EndAt)
                .SetProperty(a => a.Reason, request.Reason)
            );
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, $"Doctor time off updated successfully {affectedRows}", null))
                : Results.NoContent();
        }
      
        public async Task<IResult> DeleteDoctorTimeOffAsync(Guid timeOffId)
        {
            var affectedRows = await _context.DoctorTimeOffs.Where(x => x.TimeOffId == timeOffId).ExecuteDeleteAsync();
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, $"Deleted {affectedRows} row(s)", null))
                : Results.NoContent();
        }
        public async Task<IResult> CreateDoctorAsync(CreateDoctorRequest request)
        {
            var existingDoctor = await _context.Doctors.AsNoTracking().AnyAsync(d => d.Code == request.Code && d.ClinicId == request.ClinicId);
            if (existingDoctor)
            {
                return Results.Conflict(new ApiResponse<object>(false, $"Doctor with code '{request.Code}' already exists in this clinic.", null));
            }

            var doctor = new Doctor
            {
                ClinicId = request.ClinicId,
                Code = request.Code,
                FullName = request.FullName,
                Specialty = request.Specialty,
                Phone = request.Phone,
                Email = request.Email,
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var doctorDto = new DoctorDto(doctor.DoctorId, doctor.ClinicId, doctor.Code, doctor.FullName, doctor.Specialty, doctor.Phone, doctor.Email, doctor.IsActive);
            return Results.Created($"/doctors/{doctor.DoctorId}", new ApiResponse<DoctorDto>(true, "Doctor created successfully", doctorDto));
        }

        public async Task<IResult> GetAllDoctorAsync()
        {
            var doctors = await _context.Doctors.AsNoTracking()
            .Select(doctor => new DoctorDto(doctor.DoctorId, doctor.ClinicId, doctor.Code, doctor.FullName, doctor.Specialty, doctor.Phone, doctor.Email, doctor.IsActive))
            .ToListAsync();

            return Results.Ok(new ApiResponse<List<DoctorDto>>(true, "Doctor retrieved successfully", doctors));
        }

        public async Task<IResult> UpdateDoctorAsync(Guid doctorId, UpdateDoctorRequest request)
        {
            var affectedRows = await _context.Doctors.AsNoTracking().Where(x => x.DoctorId == doctorId).ExecuteUpdateAsync(x => x
                .SetProperty(a => a.FullName, request.FullName)
                .SetProperty(a => a.Specialty, request.Specialty)
                .SetProperty(a => a.Phone, request.Phone)
                .SetProperty(a => a.Email, request.Email)
            );
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, $"Doctor updated successfully {affectedRows}", null))
                : Results.NoContent();
        }

        public async Task<IResult> DeleteDoctorAsync(Guid doctorId)
        {
            var affectedRows = await _context.Doctors.AsNoTracking().Where(x => x.DoctorId == doctorId)
                .ExecuteDeleteAsync();
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, $"Deleted {affectedRows} row(s)", null))
                : Results.NoContent();
        }

 
    }
}