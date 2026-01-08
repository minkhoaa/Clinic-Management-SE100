#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Infrastructure.Persisstence;

namespace ClinicManagement_API.Domains.Entities;

public sealed class Patients
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid PatientId { get; set; }

    public Guid ClinicId { get; set; }
    public Guid? UserId { get; set; }
    public string PatientCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public Gender Gender { get; set; } = Gender.X;
    public DateTime? Dob { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? Email { get; set; }
    public string? AddressLine1 { get; set; }
    public string? Note { get; set; }

    // Profile fields
    public string? Allergy { get; set; } // Dị ứng
    public string? ChronicDisease { get; set; } // Bệnh lý nền
    public string? EmergencyName { get; set; } // Tên người liên hệ khẩn cấp
    public string? EmergencyPhone { get; set; } // SĐT liên hệ khẩn cấp
    public string? BloodGroup { get; set; } // Nhóm máu
    public string? InsuranceType { get; set; } // Loại bảo hiểm
    public string? InsuranceNumber { get; set; } // Số thẻ bảo hiểm

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Clinic Clinic { get; set; } = default!;
    public ICollection<Appointment>? Appointments { get; set; }
    public IEnumerable<Bill>? Bills { get; set; }
    public User? User { get; set; }
}

