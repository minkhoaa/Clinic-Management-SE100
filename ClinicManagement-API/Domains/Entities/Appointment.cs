#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicManagement_API.Domains.Enums;

namespace ClinicManagement_API.Domains.Entities;

public sealed class Appointment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid AppointmentId { get; set; }
    public Guid ClinicId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? PatientId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public AppointmentSource Source { get; set; } = AppointmentSource.Web;

    public string ContactFullName { get; set; } = default!;
    public string ContactPhone { get; set; } = default!;
    public string? ContactEmail { get; set; }
    public string? Notes { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Clinic Clinic { get; set; } = default!;
    public Doctor Doctor { get; set; } = default!;
    public Service? Service { get; set; }
    public Patients? Patient { get; set; }
    public ICollection<AppointmentToken> Tokens { get; set; } = new List<AppointmentToken>();
    public IEnumerable<Bill>? Bills { get; set; }
}

