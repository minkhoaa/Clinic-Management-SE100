#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement_API.Domains.Entities;

public sealed class DoctorAvailability
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid AvailabilityId { get; set; }
    public Guid ClinicId { get; set; }
    public Guid DoctorId { get; set; }

    /// <summary>0 = Sunday ... 6 = Saturday</summary>
    public byte DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public short SlotSizeMin { get; set; } = 30;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;

    public Clinic Clinic { get; set; } = default!;
    public Doctor Doctor { get; set; } = default!;
}
