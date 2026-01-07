#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement_API.Domains.Entities;

public sealed class Clinic
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ClinicId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<DoctorAvailability> DoctorAvailabilities { get; set; } = new List<DoctorAvailability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<StaffUser> StaffUsers { get; set; } = new List<StaffUser>();
    public ICollection<Patients> Patients { get; set; } = new List<Patients>();
    public ICollection<DoctorTimeOff> DoctorTimeOffs { get; set; } = new List<DoctorTimeOff>();
}
