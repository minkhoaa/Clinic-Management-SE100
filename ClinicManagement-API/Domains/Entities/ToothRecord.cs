#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicManagement_API.Domains.Enums;

namespace ClinicManagement_API.Domains.Entities;

/// <summary>
/// Represents the status and treatment history of a specific tooth for a patient.
/// Uses FDI (ISO 3950) notation for tooth numbering:
/// - Upper Right: 11-18
/// - Upper Left: 21-28
/// - Lower Left: 31-38
/// - Lower Right: 41-48
/// </summary>
public sealed class ToothRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ToothRecordId { get; set; }
    
    public Guid PatientId { get; set; }
    public Guid ClinicId { get; set; }
    
    /// <summary>
    /// Tooth number in FDI notation (11-48).
    /// </summary>
    public int ToothNumber { get; set; }
    
    /// <summary>
    /// Current status of the tooth.
    /// </summary>
    public ToothStatus Status { get; set; } = ToothStatus.Healthy;
    
    /// <summary>
    /// Previous status before last treatment (for history tracking).
    /// </summary>
    public ToothStatus? PreviousStatus { get; set; }
    
    /// <summary>
    /// Detailed notes about the tooth condition.
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Last treatment description (e.g., "Trám composite", "Nhổ răng khôn").
    /// </summary>
    public string? LastTreatment { get; set; }
    
    /// <summary>
    /// Reference to the medical record when last treatment was done.
    /// </summary>
    public Guid? LastMedicalRecordId { get; set; }
    
    /// <summary>
    /// Date of last treatment.
    /// </summary>
    public DateTime? LastTreatedAt { get; set; }
    
    /// <summary>
    /// Doctor who performed last treatment.
    /// </summary>
    public Guid? LastTreatedByDoctorId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Patients Patient { get; set; } = default!;
    public Clinic Clinic { get; set; } = default!;
    public MedicalRecord? LastMedicalRecord { get; set; }
    public Doctor? LastTreatedByDoctor { get; set; }
}
