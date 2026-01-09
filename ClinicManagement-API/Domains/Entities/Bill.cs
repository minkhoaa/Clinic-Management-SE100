using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicManagement_API.Domains.Enums;

namespace ClinicManagement_API.Domains.Entities;

public sealed class Bill
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid BillId { get; set; }

    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? MedicalRecordId { get; set; }

    public string InvoiceNumber { get; set; } = default!; // e.g., "HD-2025-0001"
    public BillStatus Status { get; set; } = BillStatus.Pending;

    // Financial
    public decimal Subtotal { get; set; } // Tổng tiền dịch vụ
    public decimal Discount { get; set; } // Giảm giá (VND)
    public decimal? DiscountPercent { get; set; } // % giảm giá
    public decimal? InsuranceCovered { get; set; } // Bảo hiểm chi trả
    public decimal TotalAmount { get; set; } // Tổng tiền phải trả

    // Payment info
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal? PaidAmount { get; set; }
    public decimal? ChangeAmount { get; set; } // Tiền thừa

    public string? Notes { get; set; }
    public Guid? CreatedById { get; set; } // Staff user who created

    public DateTime CreatedAt { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Clinic Clinic { get; set; } = default!;
    public Patients Patient { get; set; } = default!;
    public Appointment? Appointment { get; set; }
    public MedicalRecord? MedicalRecord { get; set; }
    public ICollection<BillItem> BillItems { get; set; } = new List<BillItem>();
}
