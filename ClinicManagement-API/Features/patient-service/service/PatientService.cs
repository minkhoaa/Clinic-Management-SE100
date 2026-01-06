using System.Security.Claims;
using ClinicManagement_API.Contracts;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.patient_service.dto;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.patient_service.service;

public interface IPatientService
{
    Task<IResult> GetProfileAsync(ClaimsPrincipal user);
    Task<IResult> UpdateProfileAsync(ClaimsPrincipal user, UpdatePatientProfileRequest request);
    Task<IResult> GetAllPatientsAsync();
    Task<IResult> CreatePatientAsync(CreatePatientDto request);
    Task<IResult> UpdatePatientAsync(Guid patientId, CreatePatientDto request);
    Task<IResult> DeletePatientAsync(Guid patientId);
    Task<IResult> GetAppointmentsAsync(ClaimsPrincipal user, Guid? patientId, string? phone);
    Task<IResult> GetAppointmentDetailAsync(Guid id);
    Task<IResult> CancelPatientAppointmentAsync(Guid id, CancelAppointmentRequest request);
}

public class PatientService : IPatientService
{
    private readonly ClinicDbContext _context;

    public PatientService(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> GetProfileAsync(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (patient == null)
        {
            return Results.NotFound(new ApiResponse<object>(false, "Patient profile not found", null));
        }

        var response = new PatientProfileResponse(
            patient.PatientId,
            patient.FullName,
            patient.Gender.ToString(),
            patient.Dob?.ToString("yyyy-MM-dd"),
            patient.PrimaryPhone,
            patient.Email,
            patient.AddressLine1,
            patient.Allergy,
            patient.ChronicDisease,
            patient.EmergencyName,
            patient.EmergencyPhone,
            patient.BloodGroup,
            patient.InsuranceType,
            patient.InsuranceNumber);

        return Results.Ok(new ApiResponse<PatientProfileResponse>(true, "Profile retrieved successfully", response));
    }

    public async Task<IResult> UpdateProfileAsync(ClaimsPrincipal user, UpdatePatientProfileRequest request)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patient == null)
        {
            return Results.NotFound(new ApiResponse<object>(false, "Patient profile not found", null));
        }

        patient.FullName = request.FullName;
        if (Enum.TryParse<Gender>(request.Gender, true, out var gender))
        {
            patient.Gender = gender;
        }
        if (!string.IsNullOrEmpty(request.Dob) && DateTime.TryParse(request.Dob, out var dob))
        {
            patient.Dob = DateTime.SpecifyKind(dob, DateTimeKind.Utc);
        }
        patient.PrimaryPhone = request.Phone;
        patient.Email = request.Email;
        patient.AddressLine1 = request.Address;
        patient.EmergencyName = request.EmergencyName;
        patient.EmergencyPhone = request.EmergencyPhone;
        patient.BloodGroup = request.BloodGroup;
        patient.Allergy = request.Allergy;
        patient.ChronicDisease = request.ChronicDisease;
        patient.InsuranceType = request.InsuranceType;
        patient.InsuranceNumber = request.InsuranceNumber;
        patient.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Results.Ok(new ApiResponse<object>(true, "Profile updated successfully", new { success = true }));
    }

    public async Task<IResult> GetAllPatientsAsync()
    {
        var patients = await _context.Patients
            .AsNoTracking()
            .Select(s => new PatientDto(
                s.PatientId, s.ClinicId, s.PatientCode, s.FullName,
                s.Gender, s.Dob, s.PrimaryPhone, s.Email, s.Note,
                s.AddressLine1, s.Clinic))
            .ToListAsync();

        return Results.Ok(new ApiResponse<List<PatientDto>>(true, "Patients retrieved successfully", patients));
    }

    public async Task<IResult> CreatePatientAsync(CreatePatientDto request)
    {
        var patient = new Patients
        {
            PatientId = Guid.NewGuid(),
            ClinicId = request.ClinicId,
            PatientCode = request.PatientCode,
            FullName = request.FullName,
            Gender = request.Gender,
            Dob = request.Dob.HasValue ? DateTime.SpecifyKind(request.Dob.Value, DateTimeKind.Utc) : null,
            PrimaryPhone = request.PrimaryPhone,
            Email = request.Email,
            AddressLine1 = request.AddressLine1,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<object>(true, "Patient created successfully", new { id = patient.PatientId }));
    }

    public async Task<IResult> UpdatePatientAsync(Guid patientId, CreatePatientDto request)
    {
        var patient = await _context.Patients.FindAsync(patientId);
        if (patient == null)
        {
            return Results.NotFound(new ApiResponse<object>(false, "Patient not found", null));
        }

        patient.FullName = request.FullName;
        patient.Gender = request.Gender;
        patient.Dob = request.Dob.HasValue ? DateTime.SpecifyKind(request.Dob.Value, DateTimeKind.Utc) : null;
        patient.PrimaryPhone = request.PrimaryPhone;
        patient.Email = request.Email;
        patient.AddressLine1 = request.AddressLine1;
        patient.Note = request.Note;
        patient.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Results.Ok(new ApiResponse<object>(true, "Patient updated successfully", new { success = true }));
    }

    public async Task<IResult> DeletePatientAsync(Guid patientId)
    {
        var patient = await _context.Patients.FindAsync(patientId);
        if (patient == null)
        {
            return Results.NotFound(new ApiResponse<object>(false, "Patient not found", null));
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();
        return Results.Ok(new ApiResponse<object>(true, "Patient deleted successfully", new { success = true }));
    }

    public async Task<IResult> GetAppointmentsAsync(ClaimsPrincipal user, Guid? patientId, string? phone)
    {
        // If user is authenticated, get their PatientId
        Guid? userPatientId = null;
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);
            userPatientId = patient?.PatientId;
        }

        // Build query
        var query = _context.Bookings
            .Include(b => b.Doctor)
            .Include(b => b.Service)
            .AsNoTracking();

        // Filter by patientId if provided, otherwise by phone, otherwise by logged-in user's patientId
        if (patientId.HasValue)
        {
            query = query.Where(b => b.PatientId == patientId.Value);
        }
        else if (!string.IsNullOrEmpty(phone))
        {
            query = query.Where(b => b.Phone == phone);
        }
        else if (userPatientId.HasValue)
        {
            query = query.Where(b => b.PatientId == userPatientId.Value);
        }
        else
        {
            return Results.BadRequest(new ApiResponse<object>(false, "PatientId or phone is required", null));
        }

        var bookings = await query
            .OrderByDescending(b => b.StartAt)
            .Select(b => new AppointmentListItemDto(
                b.BookingId,
                b.Service != null ? b.Service.Name : "Khám tổng quát",
                b.Doctor != null ? b.Doctor.FullName : "Chưa xác định",
                b.StartAt.ToString("dd/MM/yyyy"),
                b.StartAt.ToString("HH:mm"),
                b.Notes,
                b.Status.ToString().ToLower()))
            .ToListAsync();

        return Results.Ok(new ApiResponse<List<AppointmentListItemDto>>(true, "Appointments retrieved successfully", bookings));
    }

    public async Task<IResult> GetAppointmentDetailAsync(Guid id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Doctor)
            .Include(b => b.Service)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            return Results.NotFound(new ApiResponse<object>(false, "Appointment not found", null));
        }

        var response = new AppointmentListItemDto(
            booking.BookingId,
            booking.Service != null ? booking.Service.Name : "Khám tổng quát",
            booking.Doctor != null ? booking.Doctor.FullName : "Chưa xác định",
            booking.StartAt.ToString("dd/MM/yyyy"),
            booking.StartAt.ToString("HH:mm"),
            booking.Notes,
            booking.Status.ToString().ToLower());

        return Results.Ok(new ApiResponse<AppointmentListItemDto>(true, "Appointment retrieved successfully", response));
    }

    public async Task<IResult> CancelPatientAppointmentAsync(Guid id, CancelAppointmentRequest request)
    {
        var booking = await _context.Bookings
            .Include(b => b.Appointment)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            return Results.NotFound(new ApiResponse<object>(false, "Appointment not found", null));
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Appointment is already cancelled", null));
        }

        // Check if cancellation is within allowed time (not within 2 hours of appointment)
        if (booking.StartAt < DateTime.UtcNow.AddHours(2))
        {
            return Results.Conflict(new ApiResponse<object>(false, "Cannot cancel appointment within 2 hours of start time", null));
        }

        booking.Status = BookingStatus.Cancelled;
        booking.Notes = string.IsNullOrEmpty(booking.Notes) 
            ? $"Cancelled: {request.Reason}" 
            : $"{booking.Notes} | Cancelled: {request.Reason}";
        booking.UpdatedAt = DateTime.UtcNow;

        // If there's an associated appointment, cancel it too
        if (booking.Appointment != null)
        {
            booking.Appointment.Status = AppointmentStatus.Cancelled;
            booking.Appointment.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Results.Ok(new ApiResponse<object>(true, "Appointment cancelled successfully", new { success = true }));
    }
}