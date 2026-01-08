#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement_API.Domains.Entities;

public sealed class Medicine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid MedicineId { get; set; }
    
    public Guid ClinicId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Unit { get; set; }  // Đơn vị: viên, hộp, chai, lọ...
    public decimal? Price { get; set; }  // Giá bán
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Clinic Clinic { get; set; } = default!;
    public ICollection<BillItem>? BillItems { get; set; }
    public ICollection<PrescriptionTemplateMedicine>? PrescriptionTemplateMedicines { get; set; }
}
