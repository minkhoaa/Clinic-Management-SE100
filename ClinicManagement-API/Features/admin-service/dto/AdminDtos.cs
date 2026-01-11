namespace ClinicManagement_API.Features.admin_service.dto;

// ============ Dashboard Stats ============
public record AdminDashboardStatsDto(
    int TotalPatients,
    int TotalAppointments,
    decimal TotalRevenue,
    decimal SatisfactionRate,
    int TotalDoctors,
    int TotalStaff,
    int ActiveClinics
);

// ============ Patient Report ============
public record PatientReportItemDto(
    Guid Id,
    string PatientCode,
    string FullName,
    string? Phone,
    string? Email,
    string? Gender,
    DateTime? Dob,
    int TotalVisits,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

// ============ Appointment Report ============
public record AppointmentReportDto(
    string Month,
    int TotalAppointments,
    int CompletedAppointments,
    int CancelledAppointments,
    int NoShowAppointments,
    int PendingAppointments
);

// ============ Historical Stats ============
public record HistoricalStatsDto(
    string Period,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal Revenue,
    int TotalVisits,
    int NewPatients,
    int CompletedAppointments,
    int CancelledAppointments
);

// ============ Review DTOs ============
public record CreateReviewRequest(
    Guid AppointmentId,
    int Rating,
    string? Comment
);

public record ReviewDto(
    Guid ReviewId,
    Guid AppointmentId,
    int Rating,
    string? Comment,
    DateTime CreatedAt
);

public record RatingDistributionDto(
    int FiveStar,
    int FourStar,
    int ThreeStar,
    int TwoStar,
    int OneStar
);

public record ReviewStatsDto(
    decimal AverageRating,
    int TotalReviews,
    decimal SatisfactionRate,
    RatingDistributionDto RatingDistribution
);
