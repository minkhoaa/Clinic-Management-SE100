#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement_API.Domains.Entities;

public sealed class MedicalRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid RecordId { get; set; }
    
    public Guid PatientId { get; set; }
    public Guid ClinicId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? AppointmentId { get; set; }  // Optional link to appointment
    
    public string Title { get; set; } = default!;        // e.g., "Trám răng", "Nhổ răng khôn"
    public DateTime RecordDate { get; set; }             // Ngày khám
    public string? Diagnosis { get; set; }               // Chẩn đoán
    public string? Treatment { get; set; }               // Điều trị
    public string? Prescription { get; set; }            // Đơn thuốc
    public string? Notes { get; set; }                   // Ghi chú
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Patients Patient { get; set; } = default!;
    public Clinic Clinic { get; set; } = default!;
    public Doctor Doctor { get; set; } = default!;
    public Appointment? Appointment { get; set; }
    public ICollection<MedicalRecordAttachment> Attachments { get; set; } = new List<MedicalRecordAttachment>();
    public IEnumerable<Bill>? Bills { get; set; }
}
