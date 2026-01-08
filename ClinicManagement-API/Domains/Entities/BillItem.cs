using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicManagement_API.Domains.Enums;

namespace ClinicManagement_API.Domains.Entities;

public sealed class BillItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid BillItemId { get; set; }

    public Guid BillId { get; set; }
    public Guid? ServiceId { get; set; }

    public BillItemType Type { get; set; } = BillItemType.Service;
    public string Name { get; set; } = default!; // Tên dịch vụ/thuốc
    public int Quantity { get; set; } = 1;
    public string Unit { get; set; } = "lần"; // Đơn vị (lần, viên, hộp...)
    public decimal UnitPrice { get; set; } // Đơn giá
    public decimal Amount { get; set; } // Thành tiền (Quantity × UnitPrice)
    public string? ToothNumber { get; set; } // Số răng (nếu là dịch vụ nha khoa)
    public string? Notes { get; set; }

    // Navigation properties
    public Bill Bill { get; set; } = default!;
    public Service? Service { get; set; }
}
