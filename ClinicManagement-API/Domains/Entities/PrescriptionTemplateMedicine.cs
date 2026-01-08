using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement_API.Domains.Entities;

public sealed class PrescriptionTemplateMedicine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid TemplateId { get; set; }
    public Guid MedicineId { get; set; }

    public string Dosage { get; set; } = default!; // "500mg", "1000mg"
    public int Quantity { get; set; } // 21, 30
    public string Instructions { get; set; } = default!; // "3 lần/ngày sau ăn"

    // Navigation properties
    public PrescriptionTemplate Template { get; set; } = default!;
    public Medicine Medicine { get; set; } = default!;
}
