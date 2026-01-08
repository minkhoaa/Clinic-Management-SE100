#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement_API.Domains.Entities;

public sealed class PrescriptionTemplate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid TemplateId { get; set; }

    public Guid ClinicId { get; set; }
    public Guid? DoctorId { get; set; } // null = shared template for all doctors in clinic

    public string Name { get; set; } = default!;
    public string? Category { get; set; } // "dental", "general", etc.
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Clinic Clinic { get; set; } = default!;
    public Doctor? Doctor { get; set; }

    public ICollection<PrescriptionTemplateMedicine> PrescriptionTemplateMedicines { get; set; } =
        new List<PrescriptionTemplateMedicine>();
}
