namespace ClinicManagement_API.Domains.Enums;

/// <summary>
/// Actions that can be audited in the system.
/// </summary>
public enum AuditAction : byte
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    Viewed = 4,         // For sensitive data access tracking
    Exported = 5,       // For data export tracking
    StatusChanged = 6   // For status transitions
}

/// <summary>
/// Entity types that can be audited.
/// </summary>
public enum AuditEntityType : byte
{
    MedicalRecord = 1,
    Prescription = 2,
    Appointment = 3,
    Bill = 4,
    Patient = 5,
    ToothRecord = 6
}
