using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Features.booking_service.dto;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.booking_service.service
{
    
    public interface IAdminService
    {
        Task<IResult> CreateStaffAsync(CreateStaffUserDto request);
            
        Task<IResult> UpdateStaffAsync(Guid userId, CreateStaffUserDto request);
            
        Task<IResult> DeleteStaffAsync(Guid userId);

        Task <IResult> CreatePatientAsync(CreatePatientDto request);

        Task<IResult> UpdatePatientAsync(Guid patientId, CreatePatientDto request);

        Task<IResult> DeletePatientAsync(Guid patientId);
    }
    public class AdminService : IAdminService
    {
        private readonly ClinicDbContext _context;
        public AdminService(ClinicDbContext context)
        {
            _context = context;
        }
        
        public async Task<IResult> CreateStaffAsync(CreateStaffUserDto request)
        {
            var existingClinic = await _context.Clinics.AsNoTracking().AnyAsync(c => c.ClinicId == request.ClinicId);
            if (existingClinic)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Clinic not found", existingClinic));
            }
            
            var staff = new StaffUser
            {

                ClinicId = request.ClinicId,
                Username = request.Username,
                FullName = request.FullName,
                IsActive = request.IsActive,
                Role = request.Role
            };

            _context.StaffUsers.Add(staff);
            await _context.SaveChangesAsync();
            
            var staffUserDto = new StaffUserDto(staff.UserId, staff.ClinicId, staff.Username, staff.FullName, staff.Role,staff.IsActive, staff.Clinic);
            return Results.Created($"/staff/{staff.UserId}", new ApiResponse<StaffUserDto>(true, "Staff created successfully", staffUserDto));
        }

        public async Task<IResult> UpdateStaffAsync(Guid userId, CreateStaffUserDto request)
        {
            var existingClinic = await _context.Clinics.AsNoTracking().AnyAsync(c => c.ClinicId == request.ClinicId);
            if (!existingClinic)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Clinic not found", existingClinic));
            }
            
            var existingStaff = await _context.StaffUsers.AsNoTracking().AnyAsync(c => c.UserId == userId);
            if (!existingStaff)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Staff not found", existingStaff));
            }
            
            var affectedRows = await _context.StaffUsers.AsNoTracking().Where(s => s.UserId == userId).ExecuteUpdateAsync<StaffUser>(s => s
                .SetProperty(s => s.ClinicId, request.ClinicId)
                .SetProperty(s => s.Username, request.Username)
                .SetProperty(s => s.FullName, request.FullName)
                .SetProperty(s => s.Role, request.Role)
                .SetProperty(s => s.IsActive, request.IsActive));
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, "Staff updated successfully",  affectedRows))
                : Results.NoContent();
        }

        public async Task<IResult> DeleteStaffAsync(Guid userId)
        {
            var existingStaff = await _context.StaffUsers.AsNoTracking().AnyAsync(c => c.UserId == userId);
            if (!existingStaff)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Staff not found", existingStaff));
            }
            
            var affectedRows = await _context.StaffUsers.AsNoTracking().Where(x => x.UserId == userId)
                .ExecuteDeleteAsync();
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, $"Deleted {affectedRows} row(s)", null))
                : Results.NoContent();
        }

        public async Task<IResult> CreatePatientAsync(CreatePatientDto request)
        {
             var existingClinic = await _context.Clinics.AsNoTracking().AnyAsync(c => c.ClinicId == request.ClinicId);
            if (!existingClinic)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Clinic not found", existingClinic));
            }

            var patient = new Patients
            {
                ClinicId = request.ClinicId,
                PatientCode = request.PatientCode,
                FullName = request.FullName,
                PrimaryPhone = request.PrimaryPhone,
                Email = request.Email,
                Gender = request.Gender,
                Dob = request.Dob,
                AddressLine1 = request.AddressLine1,
                Note = request.Note
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            var patientDto = new PatientDto(patient.PatientId, patient.ClinicId, patient.PatientCode, patient.FullName, patient.Gender, patient.Dob, patient.PrimaryPhone, patient.Email, patient.AddressLine1, patient.Note, patient.Clinic);
            return Results.Created($"/patients/{patient.PatientId}", new ApiResponse<PatientDto>(true, "Patient created successfully", patientDto));
        }

        public async Task<IResult> UpdatePatientAsync(Guid patientId, CreatePatientDto request)
        {
            var existingClinic = await _context.Clinics.AsNoTracking().AnyAsync(c => c.ClinicId == request.ClinicId);
            if (!existingClinic)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Clinic not found", existingClinic));
            }

            var existingPatient = await _context.Patients.AsNoTracking().AnyAsync(c => c.PatientId == patientId);
            if (!existingPatient)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Patient not found", existingPatient));
            }

            var affectedRows = await _context.Patients.AsNoTracking().Where(p => p.PatientId == patientId).ExecuteUpdateAsync<Patients>(p => p
                .SetProperty(p => p.ClinicId, request.ClinicId)
                .SetProperty(p => p.PatientCode, request.PatientCode)
                .SetProperty(p => p.FullName, request.FullName)
                .SetProperty(p => p.PrimaryPhone, request.PrimaryPhone)
                .SetProperty(p => p.Email, request.Email)
                .SetProperty(p => p.Gender, request.Gender)
                .SetProperty(p => p.Dob, request.Dob)
                .SetProperty(p => p.AddressLine1, request.AddressLine1)
                .SetProperty(p => p.Note, request.Note));
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, "Patient updated successfully",  affectedRows))
                : Results.NoContent();
        }

        public async Task<IResult> DeletePatientAsync(Guid patientId)
        {
            var existingPatient = await _context.Patients.AsNoTracking().AnyAsync(c => c.PatientId == patientId);
            if (!existingPatient)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Patient not found", existingPatient));
            }

            var affectedRows = await _context.Patients.AsNoTracking().Where(x => x.PatientId == patientId)
                .ExecuteDeleteAsync();
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, $"Deleted {affectedRows} row(s)", null))
                : Results.NoContent();
        }
    }
}