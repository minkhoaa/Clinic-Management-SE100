public record CreateDoctorRequest(
    Guid ClinicId,
    string Code,
    string FullName,
    string? Specialty,
    string? Phone,
    string? Email);

public record UpdateDoctorRequest(
    Guid CLinicId,
    string Code,
    string FullName,
    string? Specialty,
    string? Phone,
    string? Email);

public record CreateDoctorAvailability(
    Guid ClinicId,
    Guid DoctorId,
    byte DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    short SlotSizeMin,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsActive
);

public record UpdateDoctorAvailability(
    TimeSpan EndTime,
    short SlotSizeMin,
    bool IsActive
);
