using ClinicManagement_API.Contracts;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.receptionist_service.dto;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.receptionist_service.service;

public interface IReceptionistService
{
    Task<IResult> GetDashboardStatsAsync(Guid? clinicId);
    Task<IResult> GetTodayAppointmentsAsync(Guid? clinicId, int limit);
    Task<IResult> GetAppointmentsAsync(string date, string? doctor, string? status, string? search, Guid? clinicId);
    Task<IResult> CreateAppointmentAsync(CreateAppointmentRequest request);
    Task<IResult> UpdateAppointmentAsync(Guid id, UpdateAppointmentRequest request);
    Task<IResult> ConfirmAppointmentAsync(Guid id);
    Task<IResult> CancelAppointmentAsync(Guid id, string? reason);
    Task<IResult> CheckinAppointmentAsync(Guid id);
}

public class ReceptionistService : IReceptionistService
{
    private readonly ClinicDbContext _context;

    public ReceptionistService(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> GetDashboardStatsAsync(Guid? clinicId)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var query = _context.Appointments.AsNoTracking();
        var bookingQuery = _context.Bookings.AsNoTracking();

        if (clinicId.HasValue)
        {
            query = query.Where(a => a.ClinicId == clinicId.Value);
            bookingQuery = bookingQuery.Where(b => b.ClinicId == clinicId.Value);
        }

        // Patients waiting (CheckedIn status, today)
        var patientsWaiting = await query
            .Where(a => a.StartAt >= today && a.StartAt < tomorrow)
            .Where(a => a.Status == AppointmentStatus.CheckedIn)
            .CountAsync();

        // Today's appointments (all statuses, today)
        var todayAppointments = await query
            .Where(a => a.StartAt >= today && a.StartAt < tomorrow)
            .CountAsync();

        // Pending confirmation (Pending bookings, today or future)
        var pendingConfirmation = await bookingQuery
            .Where(b => b.StartAt >= today)
            .Where(b => b.Status == BookingStatus.Pending)
            .CountAsync();



        // Pending payment (placeholder - future feature)
        var pendingPayment = 0;

        var stats = new DashboardStatsDto(
            patientsWaiting,
            todayAppointments,
            pendingConfirmation,
            pendingPayment
        );

        return Results.Ok(new ApiResponse<DashboardStatsDto>(true, "Dashboard stats retrieved", stats));
    }

    public async Task<IResult> GetTodayAppointmentsAsync(Guid? clinicId, int limit)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var query = _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Service)
            .Include(a => a.Booking)
            .AsNoTracking()
            .Where(a => a.StartAt >= today && a.StartAt < tomorrow);

        if (clinicId.HasValue)
        {
            query = query.Where(a => a.ClinicId == clinicId.Value);
        }

        var appointments = await query
            .OrderBy(a => a.StartAt)
            .Take(limit > 0 ? limit : 5)
            .Select(a => new TodayAppointmentDto(
                a.AppointmentId,
                a.ContactFullName,
                a.ContactPhone,
                a.Service != null ? a.Service.Name : "Khám tổng quát",
                a.Doctor.FullName,
                a.StartAt.ToString("yyyy-MM-dd"),
                a.StartAt.ToString("HH:mm"),
                (int)(a.EndAt - a.StartAt).TotalMinutes,
                MapStatus(a.Status),
                a.Booking != null ? a.Booking.Notes : null
            ))
            .ToListAsync();

        return Results.Ok(new ApiResponse<List<TodayAppointmentDto>>(true, "Today's appointments retrieved", appointments));
    }

    public async Task<IResult> GetAppointmentsAsync(string date, string? doctor, string? status, string? search, Guid? clinicId)
    {
        if (!DateOnly.TryParse(date, out var targetDate))
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Invalid date format. Use YYYY-MM-DD", null));
        }

        var startOfDay = targetDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endOfDay = targetDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var query = _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Service)
            .Include(a => a.Booking)
            .AsNoTracking()
            .Where(a => a.StartAt >= startOfDay && a.StartAt <= endOfDay);

        if (clinicId.HasValue)
        {
            query = query.Where(a => a.ClinicId == clinicId.Value);
        }

        // Filter by doctor (ID or name)
        if (!string.IsNullOrEmpty(doctor))
        {
            if (Guid.TryParse(doctor, out var doctorId))
            {
                query = query.Where(a => a.DoctorId == doctorId);
            }
            else
            {
                query = query.Where(a => a.Doctor.FullName.ToLower().Contains(doctor.ToLower()));
            }
        }

        // Filter by status
        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = ParseStatus(status);
            if (statusEnum.HasValue)
            {
                query = query.Where(a => a.Status == statusEnum.Value);
            }
        }

        // Search by patient name or phone
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a =>
                a.ContactFullName.ToLower().Contains(search.ToLower()) ||
                a.ContactPhone.Contains(search));
        }

        var appointments = await query
            .OrderBy(a => a.StartAt)
            .Select(a => new TodayAppointmentDto(
                a.AppointmentId,
                a.ContactFullName,
                a.ContactPhone,
                a.Service != null ? a.Service.Name : "Khám tổng quát",
                a.Doctor.FullName,
                a.StartAt.ToString("yyyy-MM-dd"),
                a.StartAt.ToString("HH:mm"),
                (int)(a.EndAt - a.StartAt).TotalMinutes,
                MapStatus(a.Status),
                a.Booking != null ? a.Booking.Notes : null
            ))
            .ToListAsync();

        return Results.Ok(new ApiResponse<List<TodayAppointmentDto>>(true, "Appointments retrieved", appointments));
    }

    public async Task<IResult> CreateAppointmentAsync(CreateAppointmentRequest request)
    {
        // Validate clinic
        var clinic = await _context.Clinics.FindAsync(request.ClinicId);
        if (clinic == null)
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Clinic not found", null));
        }

        // Validate doctor
        var doctor = await _context.Doctors.FindAsync(request.DoctorId);
        if (doctor == null || doctor.ClinicId != request.ClinicId)
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Doctor not found", null));
        }

        // Validate service
        var service = await _context.Services.FindAsync(request.ServiceId);
        if (service == null || service.ClinicId != request.ClinicId)
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Service not found", null));
        }

        // Parse date and time
        if (!DateOnly.TryParse(request.Date, out var date) || !TimeOnly.TryParse(request.Time, out var time))
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Invalid date or time format", null));
        }

        var startAt = date.ToDateTime(time, DateTimeKind.Utc);
        var duration = request.Duration > 0 ? request.Duration : 30;
        var endAt = startAt.AddMinutes(duration);

        var doctorSupportServices = await _context.DoctorServices.AsNoTracking()
                                    .AnyAsync(k => k.DoctorId == request.DoctorId && k.ServiceId == request.ServiceId);
        if (!doctorSupportServices)
            return Results.BadRequest(new ApiResponse<object>(false, "Doctor does not offer this service", null));
        // kiểm tra lặp, kiểm tra lịch đặt có chồng chéo lên lịch nghỉ của bác sĩ khong  
        var isDoctorInTimeoff = await _context.DoctorTimeOffs.AsNoTracking()
                                .AnyAsync(k => k.DoctorId == request.DoctorId && startAt < k.EndAt && k.StartAt < endAt);
        if (isDoctorInTimeoff)
        {
            return Results.Conflict(new ApiResponse<object>(false, "Doctor is on time off during the selected period", null));
        }
        // Check doctor availability
        var avail = await _context.DoctorAvailabilities
            .Where(x => x.DoctorId == request.DoctorId && x.ClinicId == request.ClinicId
                && x.IsActive && x.DayOfWeek == (byte)date.DayOfWeek)
            .ToListAsync();

        var inAvail = avail.Any(x =>
            (!x.EffectiveFrom.HasValue || x.EffectiveFrom.Value.Date <= startAt.Date) &&
            (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= startAt.Date) &&
            time.ToTimeSpan() >= x.StartTime && endAt.TimeOfDay <= x.EndTime);

        if (!inAvail)
        {
            return Results.UnprocessableEntity(new ApiResponse<object>(false, "Selected time is outside doctor's availability", null));
        }

        // Check for conflicts
        var hasConflict = await _context.Appointments.AnyAsync(a =>
            a.ClinicId == request.ClinicId &&
            a.DoctorId == request.DoctorId &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.NoShow &&
            a.StartAt < endAt && a.EndAt > startAt);

        if (hasConflict)
        {
            return Results.Conflict(new ApiResponse<object>(false, "Time slot already taken", null));
        }

        var appointment = new Appointment
        {
            AppointmentId = Guid.NewGuid(),
            ClinicId = request.ClinicId,
            DoctorId = request.DoctorId,
            ServiceId = request.ServiceId,
            StartAt = startAt,
            EndAt = endAt,
            Source = AppointmentSource.FrontDesk,
            ContactFullName = request.PatientName,
            ContactPhone = request.Phone,
            Status = AppointmentStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<ActionResultDto>(true, "Appointment created", new ActionResultDto(true, appointment.AppointmentId)));
    }

    public async Task<IResult> UpdateAppointmentAsync(Guid id, UpdateAppointmentRequest request)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            return Results.NotFound(new ApiResponse<object>(false, "Appointment not found", null));
        }

        // Validate doctor
        var doctor = await _context.Doctors.FindAsync(request.DoctorId);
        if (doctor == null)
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Doctor not found", null));
        }

        // Validate service
        var service = await _context.Services.FindAsync(request.ServiceId);
        if (service == null)
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Service not found", null));
        }

        // Parse date and time
        if (!DateOnly.TryParse(request.Date, out var date) || !TimeOnly.TryParse(request.Time, out var time))
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Invalid date or time format", null));
        }

        var startAt = date.ToDateTime(time, DateTimeKind.Utc);
        var duration = request.Duration > 0 ? request.Duration : 30;
        var endAt = startAt.AddMinutes(duration);

        // Check doctor supports this service
        var doctorSupportServices = await _context.DoctorServices.AsNoTracking()
            .AnyAsync(k => k.DoctorId == request.DoctorId && k.ServiceId == request.ServiceId);
        if (!doctorSupportServices)
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Doctor does not offer this service", null));
        }

        // Check doctor time off
        var isDoctorInTimeoff = await _context.DoctorTimeOffs.AsNoTracking()
            .AnyAsync(k => k.DoctorId == request.DoctorId && startAt < k.EndAt && k.StartAt < endAt);
        if (isDoctorInTimeoff)
        {
            return Results.Conflict(new ApiResponse<object>(false, "Doctor is on time off during the selected period", null));
        }

        // Check doctor availability
        var avail = await _context.DoctorAvailabilities
            .Where(x => x.DoctorId == request.DoctorId && x.ClinicId == appointment.ClinicId
                && x.IsActive && x.DayOfWeek == (byte)date.DayOfWeek)
            .ToListAsync();

        var inAvail = avail.Any(x =>
            (!x.EffectiveFrom.HasValue || x.EffectiveFrom.Value.Date <= startAt.Date) &&
            (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= startAt.Date) &&
            time.ToTimeSpan() >= x.StartTime && endAt.TimeOfDay <= x.EndTime);

        if (!inAvail)
        {
            return Results.UnprocessableEntity(new ApiResponse<object>(false, "Selected time is outside doctor's availability", null));
        }

        // Check for conflicts in Appointments (excluding current appointment)
        var hasConflict = await _context.Appointments.AnyAsync(a =>
            a.AppointmentId != id &&
            a.ClinicId == appointment.ClinicId &&
            a.DoctorId == request.DoctorId &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.NoShow &&
            a.StartAt < endAt && a.EndAt > startAt);

        // Also check Bookings for pending/confirmed
        if (!hasConflict)
        {
            hasConflict = await _context.Bookings.AnyAsync(b =>
                b.ClinicId == appointment.ClinicId &&
                b.DoctorId == request.DoctorId &&
                (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) &&
                b.StartAt < endAt && b.EndAt > startAt);
        }

        if (hasConflict)
        {
            return Results.Conflict(new ApiResponse<object>(false, "Time slot already taken", null));
        }

        appointment.DoctorId = request.DoctorId;
        appointment.ServiceId = request.ServiceId;
        appointment.StartAt = startAt;
        appointment.EndAt = endAt;
        appointment.ContactFullName = request.PatientName;
        appointment.ContactPhone = request.Phone;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<ActionResultDto>(true, "Appointment updated", new ActionResultDto(true, appointment.AppointmentId)));
    }

    public async Task<IResult> ConfirmAppointmentAsync(Guid id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            return Results.NotFound(new ApiResponse<object>(false, "Appointment not found", null));
        }

        if (appointment.Status == AppointmentStatus.Confirmed)
        {
            return Results.Ok(new ApiResponse<ActionResultDto>(true, "Appointment already confirmed", new ActionResultDto(true, appointment.AppointmentId)));
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Cannot confirm cancelled appointment", null));
        }

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<ActionResultDto>(true, "Appointment confirmed", new ActionResultDto(true, appointment.AppointmentId)));
    }

    public async Task<IResult> CancelAppointmentAsync(Guid id, string? reason)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            return Results.NotFound(new ApiResponse<object>(false, "Appointment not found", null));
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Results.Ok(new ApiResponse<ActionResultDto>(true, "Appointment already cancelled", new ActionResultDto(true, appointment.AppointmentId)));
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<ActionResultDto>(true, "Appointment cancelled", new ActionResultDto(true, appointment.AppointmentId)));
    }

    public async Task<IResult> CheckinAppointmentAsync(Guid id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            return Results.NotFound(new ApiResponse<object>(false, "Appointment not found", null));
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Cannot check-in cancelled appointment", null));
        }

        if (appointment.Status == AppointmentStatus.CheckedIn)
        {
            return Results.Ok(new ApiResponse<ActionResultDto>(true, "Patient already checked in", new ActionResultDto(true, appointment.AppointmentId)));
        }

        appointment.Status = AppointmentStatus.CheckedIn;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<ActionResultDto>(true, "Patient checked in", new ActionResultDto(true, appointment.AppointmentId)));
    }

    private static string MapStatus(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Confirmed => "confirmed",
        AppointmentStatus.Booked => "pending",
        AppointmentStatus.CheckedIn => "checked-in",
        AppointmentStatus.Cancelled => "cancelled",
        AppointmentStatus.InProgress => "checked-in",
        AppointmentStatus.Completed => "completed",
        AppointmentStatus.NoShow => "no-show",
        _ => status.ToString().ToLower()
    };

    private static AppointmentStatus? ParseStatus(string status) => status.ToLower() switch
    {
        "confirmed" => AppointmentStatus.Confirmed,
        "pending" => AppointmentStatus.Booked,
        "checked-in" => AppointmentStatus.CheckedIn,
        "cancelled" => AppointmentStatus.Cancelled,
        "completed" => AppointmentStatus.Completed,
        "no-show" => AppointmentStatus.NoShow,
        _ => null
    };
}

