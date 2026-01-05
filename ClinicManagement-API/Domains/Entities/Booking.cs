#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicManagement_API.Domains.Enums;

namespace ClinicManagement_API.Domains.Entities;

public sealed class Booking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid BookingId { get; set; }
    public Guid ClinicId { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? PatientId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public string FullName { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public AppointmentSource Channel { get; set; } = AppointmentSource.Web;
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Clinic Clinic { get; set; } = default!;
    public Doctor? Doctor { get; set; }
    public Service? Service { get; set; }
    public Appointment? Appointment { get; set; }
    public Patients? Patients { get; set; }
    public ICollection<BookingToken> Tokens { get; set; } = new List<BookingToken>();
}
