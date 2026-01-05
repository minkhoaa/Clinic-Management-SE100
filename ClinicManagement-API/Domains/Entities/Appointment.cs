#nullable enable
using System;
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
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public AppointmentSource Source { get; set; } = AppointmentSource.Web;

    public string ContactFullName { get; set; } = default!;
    public string ContactPhone { get; set; } = default!;
    public string? ContactEmail { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;

    public Guid? BookingId { get; set; }


    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Clinic Clinic { get; set; } = default!;
    public Doctor Doctor { get; set; } = default!;
    public Service? Service { get; set; }
    public Booking? Booking { get; set; }
}
