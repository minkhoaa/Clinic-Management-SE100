using System.ComponentModel.DataAnnotations;

namespace ClinicManagement_API.Domains.Entities;

/// <summary>
/// Patient review for an appointment
/// </summary>
public class Review
{
    [Key]
    public Guid ReviewId { get; set; } = Guid.NewGuid();
    
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? ClinicId { get; set; }
    
    /// <summary>
    /// Rating from 1 to 5 stars
    /// </summary>
    [Range(1, 5)]
    public int Rating { get; set; }
    
    public string? Comment { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Appointment Appointment { get; set; } = null!;
    public virtual Patients Patient { get; set; } = null!;
    public virtual Doctor? Doctor { get; set; }
    public virtual Clinic? Clinic { get; set; }
}
