#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement_API.Domains.Entities;

public sealed class Service
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ServiceId { get; set; }
    public Guid ClinicId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public short? DefaultDurationMin { get; set; }
    public decimal? DefaultPrice { get; set; }
    public bool IsActive { get; set; } = true;

    public Clinic Clinic { get; set; } = default!;
    public ICollection<DoctorService> DoctorServices { get; set; } = new List<DoctorService>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
