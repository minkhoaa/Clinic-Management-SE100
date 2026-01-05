#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicManagement_API.Domains.Enums;

namespace ClinicManagement_API.Domains.Entities;

public sealed class Patients
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid PatientId { get; set; }
    public Guid ClinicId { get; set; }
    public string PatientCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public Gender Gender { get; set; } = Gender.X;
    public DateTime? Dob { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? Email { get; set; }
    public string? AddressLine1 { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Clinic Clinic { get; set; } = default!;
}
