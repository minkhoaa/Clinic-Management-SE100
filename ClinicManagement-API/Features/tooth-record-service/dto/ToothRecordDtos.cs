using ClinicManagement_API.Domains.Enums;

namespace ClinicManagement_API.Features.tooth_record_service.dto;

// Dental Chart DTO - All teeth for a patient
public record DentalChartDto(
    Guid PatientId,
    string PatientName,
    List<ToothRecordDto> Teeth,
    DateTime? LastUpdatedAt
);

// Single tooth record
public record ToothRecordDto(
    Guid ToothRecordId,
    int ToothNumber,
    string ToothName,           // Auto-generated: "Răng hàm lớn trên phải 1"
    string Quadrant,            // "Trên phải", "Trên trái", "Dưới trái", "Dưới phải"
    ToothStatus Status,
    string StatusName,          // "Khỏe mạnh", "Sâu răng", etc.
    ToothStatus? PreviousStatus,
    string? Notes,
    string? LastTreatment,
    DateTime? LastTreatedAt,
    string? LastTreatedByDoctorName
);

// Create/Update request
public record UpdateToothRecordRequest(
    int ToothNumber,
    ToothStatus Status,
    string? Notes,
    string? LastTreatment,
    Guid? MedicalRecordId   // Optional link to current examination
);

// Batch update for multiple teeth
public record BatchUpdateToothRecordsRequest(
    Guid PatientId,
    Guid? MedicalRecordId,
    List<ToothUpdate> Teeth
);

public record ToothUpdate(
    int ToothNumber,
    ToothStatus Status,
    string? Notes,
    string? Treatment
);

// Create response
public record ToothRecordResponse(
    Guid ToothRecordId,
    int ToothNumber,
    ToothStatus Status,
    DateTime UpdatedAt
);
