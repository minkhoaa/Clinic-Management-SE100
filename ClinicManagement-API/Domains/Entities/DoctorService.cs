#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement_API.Domains.Entities;

public sealed class DoctorService
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ServiceId { get; set; }
    public Guid DoctorId { get; set; }
    public bool IsEnabled { get; set; } = true;

    public Service Service { get; set; } = default!;
    public Doctor Doctor { get; set; } = default!;
}
