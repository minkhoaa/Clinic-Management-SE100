namespace ClinicManagement_API.Features.doctor_service.dto;

// Dashboard Stats
public record DoctorDashboardStatsDto(
    int WaitingCount,
    int ExaminedToday,
    string AverageExamTime,
    int AppointmentsToday,
    List<DoctorQueueItemDto> WaitingQueue
);

public record DoctorQueueItemDto(
    Guid Id,
    int QueueNumber,
    string PatientName,
    string Service,
    string Time, // HH:mm format
    string Status // pending, confirmed, checkedin, inprogress
);

// Queue
public record DoctorQueueDetailDto(
    Guid Id,
    int QueueNumber,
    Guid AppointmentId,
    Guid? PatientId,
    string PatientName,
    string PatientPhone,
    string Service,
    Guid? ServiceId,
    DateTime ScheduledTime,
    DateTime? StartTime,
    DateTime? EndTime,
    string Status
);

// Patient List
public record DoctorPatientListItemDto(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    DateTime? LastVisit,
    int TotalVisits
);

// Patient Detail
public record DoctorPatientDetailDto(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    DateTime? Dob,
    string? Gender,
    string? Address,
    string? Allergy,
    string? ChronicDisease,
    DateTime? LastVisit,
    int TotalVisits,
    List<DoctorMedicalHistoryItemDto> MedicalHistory,
    List<DoctorUpcomingAppointmentDto> UpcomingAppointments
);

public record DoctorMedicalHistoryItemDto(
    Guid Id,
    DateTime RecordDate,
    string Doctor,
    string Service,
    string? Diagnosis,
    string? Notes
);

public record DoctorUpcomingAppointmentDto(
    Guid Id,
    DateTime StartAt,
    string Service,
    string Status
);

// Medical Record Detail
public record DoctorMedicalRecordDetailDto(
    Guid Id,
    string Title,
    string Doctor,
    DateTime RecordDate,
    string? Diagnosis,
    string? Treatment,
    string? Prescription,
    string? Notes,
    List<object> Attachments // Empty for now
);

// Create Medical Record Request
public record CreateMedicalRecordRequest(
    Guid AppointmentId,
    Guid PatientId,
    string Title,
    string? Diagnosis,
    string? Treatment,
    string? Prescription,
    string? Notes
);

// Update Medical Record Request
public record UpdateMedicalRecordRequest(
    Guid AppointmentId,
    Guid PatientId,
    string Title,
    string? Diagnosis,
    string? Treatment,
    string? Prescription,
    string? Notes
);

// Create Medical Record Response
public record CreateMedicalRecordResponse(
    Guid RecordId,
    DateTime CreatedAt
);

// Update Medical Record Response
public record UpdateMedicalRecordResponse(
    Guid RecordId,
    DateTime UpdatedAt
);

// Prescription Template DTOs
public record MedicineDto(
    string Name,
    string Dosage,
    string Quantity,
    string Instructions
);

public record PrescriptionTemplateDto(
    Guid Id,
    string Name,
    string? Category,
    List<MedicineDto> Medicines,
    string? Notes
);

public record CreatePrescriptionTemplateRequest(
    string Name,
    string? Category,
    List<MedicineDto> Medicines,
    string? Notes
);

public record CreatePrescriptionTemplateResponse(
    Guid TemplateId,
    DateTime CreatedAt
);

// Examination DTOs
public record PrescriptionInput(
    List<MedicineDto> Medicines,
    string? Notes
);

public record CreateExaminationRequest(
    Guid AppointmentId,
    Guid PatientId,
    string Title,
    string? Diagnosis,
    string? Treatment,
    Dictionary<string, string>? ToothStatus, // e.g., {"T16": "filled", "T17": "healthy"}
    PrescriptionInput? Prescription,
    string? Notes,
    bool CreateBill,
    List<Guid>? ServiceIds // Required if CreateBill=true
);

public record CreateExaminationResponse(
    Guid RecordId,
    Guid? BillId,
    DateTime CreatedAt
);
