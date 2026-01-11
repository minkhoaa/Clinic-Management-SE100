using ClinicManagement_API.Contracts;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.admin_service.dto;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.admin_service.service;

public interface IAdminReportService
{
    Task<IResult> GetDashboardStatsAsync();
    Task<IResult> GetPatientReportAsync(int? month, int? year);
    Task<IResult> GetHistoricalStatsAsync(DateTime? startDate, DateTime? endDate);
    Task<IResult> GetAppointmentReportAsync(int month, int year);
}

public class AdminReportService : IAdminReportService
{
    private readonly ClinicDbContext _context;

    public AdminReportService(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> GetDashboardStatsAsync()
    {
        var totalPatients = await _context.Patients.AsNoTracking().CountAsync();
        var totalAppointments = await _context.Appointments.AsNoTracking().CountAsync();
        var totalRevenue = await _context.Bills.AsNoTracking()
            .Where(b => b.Status == BillStatus.Paid)
            .SumAsync(b => b.TotalAmount);
        var totalDoctors = await _context.Doctors.AsNoTracking().CountAsync();
        var totalStaff = await _context.StaffUsers.AsNoTracking().CountAsync();
        var activeClinics = await _context.Clinics.AsNoTracking().CountAsync();

        // Calculate satisfaction rate from reviews
        var reviews = await _context.Reviews.AsNoTracking().ToListAsync();
        decimal satisfactionRate = 0;
        if (reviews.Count > 0)
        {
            var satisfiedCount = reviews.Count(r => r.Rating >= 4);
            satisfactionRate = Math.Round((decimal)satisfiedCount / reviews.Count * 100, 1);
        }

        var stats = new AdminDashboardStatsDto(
            totalPatients,
            totalAppointments,
            totalRevenue,
            satisfactionRate,
            totalDoctors,
            totalStaff,
            activeClinics
        );

        return Results.Ok(
            new ApiResponse<AdminDashboardStatsDto>(true, "Dashboard stats retrieved successfully", stats));
    }

    public async Task<IResult> GetPatientReportAsync(int? month, int? year)
    {
        var query = _context.Patients.AsNoTracking().AsQueryable();

        // Filter by month/year if specified
        if (month.HasValue && year.HasValue)
        {
            var startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);
            query = query.Where(p => p.CreatedAt >= startDate && p.CreatedAt < endDate);
        }
        else if (year.HasValue)
        {
            var startDate = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddYears(1);
            query = query.Where(p => p.CreatedAt >= startDate && p.CreatedAt < endDate);
        }

        var patients = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        // Get visit counts for each patient
        var patientIds = patients.Select(p => p.PatientId).ToList();
        var visitCounts = await _context.Appointments.AsNoTracking()
            .Where(a => patientIds.Contains(a.PatientId!.Value) && a.Status == AppointmentStatus.Completed)
            .GroupBy(a => a.PatientId)
            .Select(g => new { PatientId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PatientId!.Value, x => x.Count);

        var result = patients.Select(p => new PatientReportItemDto(
            p.PatientId,
            p.PatientCode ?? "",
            p.FullName,
            p.PrimaryPhone,
            p.Email,
            p.Gender.ToString(),
            p.Dob,
            visitCounts.GetValueOrDefault(p.PatientId, 0),
            p.CreatedAt,
            p.UpdatedAt
        )).ToList();

        return Results.Ok(
            new ApiResponse<List<PatientReportItemDto>>(true, "Patient report retrieved successfully", result));
    }

    public async Task<IResult> GetHistoricalStatsAsync(DateTime? startDate, DateTime? endDate)
    {
        // Default: last 12 months
        var end = endDate ?? DateTime.UtcNow;
        var start = startDate ?? end.AddMonths(-11).Date;

        // Get monthly data
        var bills = await _context.Bills.AsNoTracking()
            .Where(b => b.Status == BillStatus.Paid && b.PaymentDate >= start && b.PaymentDate <= end)
            .ToListAsync();

        var appointments = await _context.Appointments.AsNoTracking()
            .Where(a => a.StartAt >= start && a.StartAt <= end)
            .ToListAsync();

        var patients = await _context.Patients.AsNoTracking()
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
            .ToListAsync();

        var result = new List<HistoricalStatsDto>();

        // Generate monthly stats
        var current = new DateTime(start.Year, start.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endMonth = new DateTime(end.Year, end.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        while (current <= endMonth)
        {
            var periodStart = current;
            var periodEnd = current.AddMonths(1).AddSeconds(-1);

            var monthBills = bills.Where(b => b.PaymentDate >= periodStart && b.PaymentDate <= periodEnd);
            var monthAppointments = appointments.Where(a => a.StartAt >= periodStart && a.StartAt <= periodEnd);
            var monthPatients = patients.Where(p => p.CreatedAt >= periodStart && p.CreatedAt <= periodEnd);

            result.Add(new HistoricalStatsDto(
                current.ToString("yyyy-MM"),
                periodStart,
                periodEnd,
                monthBills.Sum(b => b.TotalAmount),
                monthAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                monthPatients.Count(),
                monthAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                monthAppointments.Count(a => a.Status == AppointmentStatus.Cancelled)
            ));

            current = current.AddMonths(1);
        }

        return Results.Ok(
            new ApiResponse<List<HistoricalStatsDto>>(true, "Historical stats retrieved successfully", result));
    }

    public async Task<IResult> GetAppointmentReportAsync(int month, int year)
    {
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);

        var appointments = await _context.Appointments.AsNoTracking()
            .Where(a => a.StartAt >= startDate && a.StartAt < endDate)
            .ToListAsync();

        var report = new AppointmentReportDto(
            startDate.ToString("yyyy-MM"),
            appointments.Count,
            appointments.Count(a => a.Status == AppointmentStatus.Completed),
            appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
            appointments.Count(a => a.Status == AppointmentStatus.NoShow),
            appointments.Count(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed)
        );

        return Results.Ok(
            new ApiResponse<AppointmentReportDto>(true, "Appointment report retrieved successfully", report));
    }
}
