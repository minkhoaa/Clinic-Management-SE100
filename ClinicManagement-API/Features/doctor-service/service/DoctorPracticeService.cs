using ClinicManagement_API.Contracts;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.doctor_service.dto;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicManagement_API.Features.doctor_service.service;

public interface IDoctorPracticeService
{
    Task<IResult> GetDashboardStatsAsync(ClaimsPrincipal user);
    Task<IResult> GetQueueAsync(ClaimsPrincipal user, string? date);
    Task<IResult> StartExamAsync(ClaimsPrincipal user, Guid appointmentId);
    Task<IResult> CompleteExamAsync(ClaimsPrincipal user, Guid appointmentId);
    Task<IResult> GetPatientsAsync(ClaimsPrincipal user);
    Task<IResult> GetPatientDetailAsync(ClaimsPrincipal user, Guid patientId);
    Task<IResult> GetMedicalRecordDetailAsync(Guid recordId);
    Task<IResult> CreateMedicalRecordAsync(ClaimsPrincipal user, CreateMedicalRecordRequest request);
    Task<IResult> UpdateMedicalRecordAsync(ClaimsPrincipal user, Guid recordId, UpdateMedicalRecordRequest request);
}

public class DoctorPracticeService : IDoctorPracticeService
{
    private readonly ClinicDbContext _context;

    public DoctorPracticeService(ClinicDbContext context)
    {
        _context = context;
    }

    private async Task<Guid?> GetDoctorIdFromUser(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        // Find StaffUser linked to this user
        var staffUser = await _context.StaffUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Role == AppRoles.Doctor);

        if (staffUser == null)
            return null;

        // Find doctor in the same clinic with matching name
        var doctor = await _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ClinicId == staffUser.ClinicId && d.FullName == staffUser.FullName);

        return doctor?.DoctorId;
    }

    public async Task<IResult> GetDashboardStatsAsync(ClaimsPrincipal user)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var query = _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId.Value);

        // Waiting count (Confirmed or CheckedIn today)
        var waitingCount = await query
            .Where(a => a.StartAt >= today && a.StartAt < tomorrow)
            .Where(a => a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.CheckedIn)
            .CountAsync();

        // Examined today (Completed today)
        var examinedToday = await query
            .Where(a => a.StartAt >= today && a.StartAt < tomorrow)
            .Where(a => a.Status == AppointmentStatus.Completed)
            .CountAsync();

        // Total appointments today
        var appointmentsToday = await query
            .Where(a => a.StartAt >= today && a.StartAt < tomorrow)
            .CountAsync();

        // Average exam time (placeholder - would need actual start/end times)
        var averageExamTime = "30 phút";

        // Waiting queue (top 10)
        var waitingAppointments = await query
            .Include(a => a.Service)
            .Where(a => a.StartAt >= today && a.StartAt < tomorrow)
            .Where(a => a.Status == AppointmentStatus.Confirmed ||
                        a.Status == AppointmentStatus.CheckedIn ||
                        a.Status == AppointmentStatus.InProgress)
            .OrderBy(a => a.StartAt)
            .Take(10)
            .Select(a => new
            {
                a.AppointmentId,
                a.ContactFullName,
                ServiceName = a.Service != null ? a.Service.Name : "Khám tổng quát",
                a.StartAt,
                a.Status
            })
            .ToListAsync();

        var waitingQueue = waitingAppointments.Select((a, index) => new DoctorQueueItemDto(
            a.AppointmentId,
            index + 1,
            a.ContactFullName,
            a.ServiceName,
            a.StartAt.ToString("HH:mm"),
            MapQueueStatus(a.Status)
        )).ToList();

        var stats = new DoctorDashboardStatsDto(
            waitingCount,
            examinedToday,
            averageExamTime,
            appointmentsToday,
            waitingQueue
        );

        return Results.Ok(new ApiResponse<DoctorDashboardStatsDto>(true, "Dashboard stats retrieved", stats));
    }

    public async Task<IResult> GetQueueAsync(ClaimsPrincipal user, string? date)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        // Parse date or use today
        DateOnly targetDate;
        if (!string.IsNullOrEmpty(date))
        {
            if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out targetDate))
            {
                return Results.BadRequest(new ApiResponse<object>(false, "Invalid date format. Use YYYY-MM-DD", null));
            }
        }
        else
        {
            targetDate = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        var startOfDay = targetDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endOfDay = targetDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var appointments = await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Patient)
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId.Value)
            .Where(a => a.StartAt >= startOfDay && a.StartAt <= endOfDay)
            .OrderBy(a => a.StartAt)
            .Select(a => new
            {
                a.AppointmentId,
                a.PatientId,
                a.ContactFullName,
                a.ContactPhone,
                ServiceName = a.Service != null ? a.Service.Name : "Khám tổng quát",
                a.ServiceId,
                a.StartAt,
                a.ActualStartTime,
                a.ActualEndTime,
                a.Status
            })
            .ToListAsync();

        var result = appointments.Select((a, index) => new DoctorQueueDetailDto(
            a.AppointmentId,
            index + 1,
            a.AppointmentId,
            a.PatientId,
            a.ContactFullName,
            a.ContactPhone,
            a.ServiceName,
            a.ServiceId,
            a.StartAt,
            a.ActualStartTime,
            a.ActualEndTime,
            MapQueueStatus(a.Status)
        )).ToList();

        return Results.Ok(new ApiResponse<List<DoctorQueueDetailDto>>(true, "Queue retrieved", result));
    }

    public async Task<IResult> GetPatientsAsync(ClaimsPrincipal user)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        // Get unique patients from completed appointments for this doctor
        var patients = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId.Value && a.PatientId != null)
            .Where(a => a.Status == AppointmentStatus.Completed)
            .GroupBy(a => a.PatientId)
            .Select(g => new
            {
                PatientId = g.Key,
                LastVisit = g.Max(a => a.StartAt),
                TotalVisits = g.Count()
            })
            .ToListAsync();

        var patientIds = patients.Select(p => p.PatientId).ToList();

        var patientDetails = await _context.Patients
            .AsNoTracking()
            .Where(p => patientIds.Contains(p.PatientId))
            .ToDictionaryAsync(p => p.PatientId);

        var result = patients
            .Where(p => p.PatientId != null && patientDetails.ContainsKey(p.PatientId.Value))
            .Select(p =>
            {
                var patient = patientDetails[p.PatientId!.Value];
                return new DoctorPatientListItemDto(
                    patient.PatientId,
                    patient.FullName,
                    patient.PrimaryPhone ?? "",
                    patient.Email,
                    p.LastVisit,
                    p.TotalVisits
                );
            })
            .OrderByDescending(p => p.LastVisit)
            .ToList();

        return Results.Ok(new ApiResponse<List<DoctorPatientListItemDto>>(true, "Patients retrieved", result));
    }

    public async Task<IResult> GetPatientDetailAsync(ClaimsPrincipal user, Guid patientId)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PatientId == patientId);

        if (patient == null)
            return Results.NotFound(new ApiResponse<object>(false, "Patient not found", null));

        // Get medical history
        var medicalHistory = await _context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Doctor)
            .Where(m => m.PatientId == patientId)
            .OrderByDescending(m => m.RecordDate)
            .Take(20)
            .Select(m => new DoctorMedicalHistoryItemDto(
                m.RecordId,
                m.RecordDate,
                m.Doctor.FullName,
                m.Title,
                m.Diagnosis,
                m.Notes
            ))
            .ToListAsync();

        // Get upcoming appointments
        var now = DateTime.UtcNow;
        var upcomingAppointments = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Service)
            .Where(a => a.PatientId == patientId && a.StartAt >= now)
            .Where(a => a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Completed)
            .OrderBy(a => a.StartAt)
            .Take(5)
            .Select(a => new DoctorUpcomingAppointmentDto(
                a.AppointmentId,
                a.StartAt,
                a.Service != null ? a.Service.Name : "Khám tổng quát",
                MapQueueStatusStatic(a.Status)
            ))
            .ToListAsync();

        // Calculate stats
        var lastVisit = await _context.Appointments
            .Where(a => a.PatientId == patientId && a.Status == AppointmentStatus.Completed)
            .OrderByDescending(a => a.StartAt)
            .Select(a => a.StartAt)
            .FirstOrDefaultAsync();

        var totalVisits = await _context.Appointments
            .CountAsync(a => a.PatientId == patientId && a.Status == AppointmentStatus.Completed);

        var result = new DoctorPatientDetailDto(
            patient.PatientId,
            patient.FullName,
            patient.PrimaryPhone ?? "",
            patient.Email,
            patient.Dob,
            patient.Gender.ToString(),
            patient.AddressLine1,
            patient.Allergy,
            patient.ChronicDisease,
            lastVisit != default ? lastVisit : null,
            totalVisits,
            medicalHistory,
            upcomingAppointments
        );

        return Results.Ok(new ApiResponse<DoctorPatientDetailDto>(true, "Patient detail retrieved", result));
    }

    public async Task<IResult> GetMedicalRecordDetailAsync(Guid recordId)
    {
        var record = await _context.MedicalRecords
            .Include(r => r.Doctor)
            .Include(r => r.Attachments)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RecordId == recordId);

        if (record == null)
            return Results.NotFound(new ApiResponse<object>(false, "Medical record not found", null));

        var response = new DoctorMedicalRecordDetailDto(
            record.RecordId,
            record.Title,
            record.Doctor.FullName,
            record.RecordDate,
            record.Diagnosis,
            record.Treatment,
            record.Prescription,
            record.Notes,
            new List<object>() // Attachments empty for now
        );

        return Results.Ok(new ApiResponse<DoctorMedicalRecordDetailDto>(true, "Medical record retrieved", response));
    }

    private string MapQueueStatus(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Pending => "pending",
        AppointmentStatus.Confirmed => "confirmed",
        AppointmentStatus.CheckedIn => "checkedin",
        AppointmentStatus.InProgress => "inprogress",
        AppointmentStatus.Completed => "completed",
        AppointmentStatus.Cancelled => "cancelled",
        AppointmentStatus.NoShow => "noshow",
        _ => "unknown"
    };

    private static string MapQueueStatusStatic(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Pending => "pending",
        AppointmentStatus.Confirmed => "confirmed",
        AppointmentStatus.CheckedIn => "checkedin",
        AppointmentStatus.InProgress => "inprogress",
        AppointmentStatus.Completed => "completed",
        AppointmentStatus.Cancelled => "cancelled",
        AppointmentStatus.NoShow => "noshow",
        _ => "unknown"
    };

    public async Task<IResult> StartExamAsync(ClaimsPrincipal user, Guid appointmentId)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment == null)
            return Results.NotFound(new ApiResponse<object>(false, "Appointment not found", null));

        if (appointment.DoctorId != doctorId.Value)
            return Results.Forbid();

        if (appointment.Status != AppointmentStatus.CheckedIn && appointment.Status != AppointmentStatus.Confirmed)
            return Results.BadRequest(new ApiResponse<object>(false,
                "Appointment must be checked-in or confirmed to start exam", null));

        appointment.Status = AppointmentStatus.InProgress;
        appointment.ActualStartTime = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<object>(true, "Exam started", new
        {
            appointment.AppointmentId,
            Status = "inprogress",
            StartTime = appointment.ActualStartTime
        }));
    }

    public async Task<IResult> CompleteExamAsync(ClaimsPrincipal user, Guid appointmentId)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment == null)
            return Results.NotFound(new ApiResponse<object>(false, "Appointment not found", null));

        if (appointment.DoctorId != doctorId.Value)
            return Results.Forbid();

        if (appointment.Status != AppointmentStatus.InProgress)
            return Results.BadRequest(new ApiResponse<object>(false, "Appointment must be in-progress to complete",
                null));

        appointment.Status = AppointmentStatus.Completed;
        appointment.ActualEndTime = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        // Calculate duration
        var duration = appointment.ActualStartTime.HasValue
            ? (appointment.ActualEndTime.Value - appointment.ActualStartTime.Value).TotalMinutes
            : 0;

        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<object>(true, "Exam completed", new
        {
            appointment.AppointmentId,
            Status = "completed",
            EndTime = appointment.ActualEndTime,
            Duration = $"{(int)duration} minutes"
        }));
    }

    public async Task<IResult> CreateMedicalRecordAsync(ClaimsPrincipal user, CreateMedicalRecordRequest request)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        // Verify patient exists
        var patient = await _context.Patients.FindAsync(request.PatientId);
        if (patient == null)
            return Results.NotFound(new ApiResponse<object>(false, "Patient not found", null));

        // Get doctor's clinic
        var doctor = await _context.Doctors.FindAsync(doctorId.Value);
        if (doctor == null)
            return Results.NotFound(new ApiResponse<object>(false, "Doctor not found", null));

        var record = new MedicalRecord
        {
            RecordId = Guid.NewGuid(),
            PatientId = request.PatientId,
            ClinicId = doctor.ClinicId,
            DoctorId = doctorId.Value,
            AppointmentId = request.AppointmentId,
            Title = request.Title,
            RecordDate = DateTime.UtcNow,
            Diagnosis = request.Diagnosis,
            Treatment = request.Treatment,
            Prescription = request.Prescription,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();

        return Results.Created($"/api/doctor/medical-records/{record.RecordId}",
            new ApiResponse<CreateMedicalRecordResponse>(true, "Medical record created",
                new CreateMedicalRecordResponse(record.RecordId, record.CreatedAt)));
    }

    public async Task<IResult> UpdateMedicalRecordAsync(ClaimsPrincipal user, Guid recordId,
        UpdateMedicalRecordRequest request)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        var record = await _context.MedicalRecords.FindAsync(recordId);
        if (record == null)
            return Results.NotFound(new ApiResponse<object>(false, "Medical record not found", null));

        // Only allow doctor who created to edit
        if (record.DoctorId != doctorId.Value)
            return Results.Forbid();

        record.AppointmentId = request.AppointmentId;
        record.PatientId = request.PatientId;
        record.Title = request.Title;
        record.Diagnosis = request.Diagnosis;
        record.Treatment = request.Treatment;
        record.Prescription = request.Prescription;
        record.Notes = request.Notes;
        record.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<UpdateMedicalRecordResponse>(true, "Medical record updated",
            new UpdateMedicalRecordResponse(record.RecordId, record.UpdatedAt)));
    }
}
