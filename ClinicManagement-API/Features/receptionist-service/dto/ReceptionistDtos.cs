namespace ClinicManagement_API.Features.receptionist_service.dto;

// Dashboard stats response
public record DashboardStatsDto(
    int PatientsWaiting, // Bệnh nhân chờ (CheckedIn status)
    int TodayAppointments, // Lịch hẹn hôm nay (all statuses)
    int PendingConfirmation, // Chờ xác nhận (Pending bookings)
    int PendingPayment // Chờ thanh toán (placeholder)
);

// Today's appointment item
public record TodayAppointmentDto(
    Guid Id,
    string PatientName,
    string Phone,
    string Service,
    string Doctor,
    DateTime StartAt,
    DateTime EndAt,
    int Duration, // Duration in minutes
    string Status, // "confirmed" | "pending" | "checked-in" | "cancelled"
    string? Notes
);

// Create appointment request
public record CreateAppointmentRequest(
    string PatientName, // *Required
    string Phone, // *Required
    Guid ServiceId, // Service ID
    Guid DoctorId, // Doctor ID
    Guid ClinicId, // Clinic ID
    DateTime StartAt,
    DateTime EndAt,
    string? Notes
);

// Update appointment request
public record UpdateAppointmentRequest(
    string PatientName, // *Required
    string Phone, // *Required
    Guid ServiceId, // Service ID
    Guid DoctorId, // Doctor ID
    DateTime StartAt,
    DateTime EndAt,
    string? Notes
);

// Cancel reason request
public record CancelReasonRequest(string? Reason);

// Simple action response
public record ActionResultDto(bool Success, Guid AppointmentId, Guid? PatientId, string? Username, string? Password);

// Queue item for receptionist queue view
public record QueueItemDto(
    Guid Id,
    int Number, // Số thứ tự
    string Name,
    string Service,
    DateTime StartAt,
    string Status // "waiting" | "checked-in" | "in-progress" | "completed"
);
